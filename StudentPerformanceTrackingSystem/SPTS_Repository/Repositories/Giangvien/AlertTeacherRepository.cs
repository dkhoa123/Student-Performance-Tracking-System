using Microsoft.EntityFrameworkCore;
using SPTS_Repository.DTOs.Giangvien;
using SPTS_Repository.Entities;
using SPTS_Repository.Interface;
using SPTS_Repository.Interface.Giangvien;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPTS_Repository.Repositories.Giangvien
{
    public class AlertTeacherRepository : IAlertTeacherRepository
    {
        private readonly SptsContext _context;
        public AlertTeacherRepository(SptsContext context)
        {
            _context = context;
        }
        public async Task<int> GetAtRiskStudentsCountAsync(int teacherId)
        {
            return await _context.Alerts
                .Where(a => a.Section.TeacherId == teacherId &&
                            a.Status != "CLOSED" &&
                            (a.Severity == "HIGH" || a.Severity == "MEDIUM"))
                .Select(a => a.StudentId)
                .Distinct()
                .CountAsync();
        }

        public async Task<int> GetNewAlertsCountAsync(int teacherId)
        {
            return await _context.Alerts
                .Where(a => a.Section.TeacherId == teacherId &&
                            (a.Status == "NEW" || a.Status == "SENT"))
                .CountAsync();
        }

        public async Task<List<AlertViewModelDto>> GetRecentAlertsByTeacherAsync(int teacherId, int top = 3)
        {
            return await _context.Alerts
                .Include(a => a.Student)
                .Include(a => a.Section)
                .Where(a => a.Section.TeacherId == teacherId &&
                            (a.Status == "NEW" || a.Status == "SENT"))
                .OrderByDescending(a => a.Severity)
                .ThenByDescending(a => a.CreatedAt)
                .Take(top)
                .Join(
                    _context.Users,
                    alert => alert.StudentId,
                    user => user.UserId,
                    (alert, user) => new { alert, user }
                )
                .Select(x => new AlertViewModelDto(
                             x.user.FullName,
                             x.alert.AlertType,
                             x.alert.Reason ?? GetDefaultMessage(x.alert.AlertType, x.alert.ActualValue),
                             x.alert.Severity,
                             GetIconName(x.alert.AlertType),
                             GetIconColor(x.alert.Severity),
                             x.alert.CreatedAt
                    )).ToListAsync();
        }
        private static string GetIconColor(string severity)
        {
            return severity switch
            {
                "HIGH" => "red",
                "MEDIUM" => "orange",
                "LOW" => "yellow",
                _ => "gray"
            };
        }

        private static string GetIconName(string alertType)
        {
            return alertType switch
            {
                "LOW_TOTAL" or "LOW_FINAL" or "LOW_GPA" => "trending_down",
                "ABSENT" => "event_busy",
                "MISSING_ASSIGNMENT" => "assignment_late",
                _ => "warning"
            };
        }

        private static string GetDefaultMessage(string alertType, decimal? actualValue)
        {
            return alertType switch
            {
                "LOW_TOTAL" => $"Điểm tổng kết dưới {actualValue}",
                "LOW_FINAL" => $"Điểm cuối kỳ dưới {actualValue}",
                "LOW_GPA" => $"GPA dưới {actualValue}",
                _ => "Cần chú ý"
            };
        }

        public async Task SyncAlertsForGradeAsync(int sectionId, int studentId, decimal? process, decimal? final, decimal? total)
        {
            const decimal threshold = 5m;

            var termId = await _context.Sections
                .Where(s => s.SectionId == sectionId)
                .Select(s => (int?)s.TermId)
                .FirstOrDefaultAsync();

            // helper: returns (createdNew, alertEntityIfCreatedOrUpdated)
            async Task<(bool createdNew, Alert? alert)> UpsertOrDelete(string alertType, string severity, decimal? actualValue, string reason)
            {
                var existing = await _context.Alerts
                    .Where(a => a.SectionId == sectionId
                             && a.StudentId == studentId
                             && a.AlertType == alertType)
                    .OrderByDescending(a => a.CreatedAt)
                    .ToListAsync();

                if (!actualValue.HasValue || actualValue.Value >= threshold)
                {
                    if (existing.Count > 0)
                        _context.Alerts.RemoveRange(existing);
                    return (false, null);
                }

                var now = DateTime.UtcNow;

                if (existing.Count == 0)
                {
                    var a = new Alert
                    {
                        StudentId = studentId,
                        SectionId = sectionId,
                        TermId = termId,
                        AlertType = alertType,
                        Severity = severity,
                        ThresholdValue = threshold,
                        ActualValue = actualValue,
                        Reason = reason,
                        Status = "NEW",
                        CreatedAt = now
                    };
                    _context.Alerts.Add(a);
                    return (true, a);
                }
                else
                {
                    var a = existing[0];
                    a.TermId = termId;
                    a.Severity = severity;
                    a.ThresholdValue = threshold;
                    a.ActualValue = actualValue;
                    a.Reason = reason;
                    a.Status = "NEW";
                    a.CreatedAt = now;

                    if (existing.Count > 1)
                        _context.Alerts.RemoveRange(existing.Skip(1));

                    return (false, a);
                }
            }

            // 1) Process
            var (newProcess, processAlert) = await UpsertOrDelete(
                "LOW_PROCESS", "LOW", process, $"Điểm quá trình dưới {threshold:0.0}"
            );

            // 2) Final
            var (newFinal, finalAlert) = await UpsertOrDelete(
                "LOW_FINAL", "MEDIUM", final, $"Điểm cuối kỳ dưới {threshold:0.0}"
            );

            // 3) Total: chỉ khi đủ 2 cột
            (bool newTotal, Alert? totalAlert) = (false, null);
            if (process.HasValue && final.HasValue)
            {
                (newTotal, totalAlert) = await UpsertOrDelete(
                    "LOW_TOTAL", "HIGH", total, $"Điểm tổng kết dưới {threshold:0.0}"
                );
            }
            else
            {
                var existingTotal = await _context.Alerts
                    .Where(a => a.SectionId == sectionId
                             && a.StudentId == studentId
                             && a.AlertType == "LOW_TOTAL")
                    .ToListAsync();
                if (existingTotal.Count > 0)
                    _context.Alerts.RemoveRange(existingTotal);
            }

            // save to get AlertId for new alerts
            await _context.SaveChangesAsync();

            // gửi notification chỉ cho alerts NEWLY created
            await CreateNotificationIfNewAsync(newProcess, processAlert);
            await CreateNotificationIfNewAsync(newFinal, finalAlert);
            await CreateNotificationIfNewAsync(newTotal, totalAlert);

            await _context.SaveChangesAsync();

            async Task CreateNotificationIfNewAsync(bool createdNew, Alert? alert)
            {
                if (!createdNew || alert == null) return;

                // UserId của Notification chính là studentId (vì StudentId == UserId trong hệ của bạn)
                var title = "Cảnh báo học tập";
                var content = alert.AlertType switch
                {
                    "LOW_PROCESS" => $"Điểm quá trình của bạn dưới {threshold:0.0}. Vui lòng cải thiện.",
                    "LOW_FINAL" => $"Điểm cuối kỳ của bạn dưới {threshold:0.0}. Vui lòng cải thiện.",
                    "LOW_TOTAL" => $"Bạn đang có nguy cơ trượt môn (Tổng kết dưới {threshold:0.0}).",
                    _ => "Bạn có một cảnh báo học tập."
                };

                _context.Notifications.Add(new Notification
                {
                    UserId = studentId,
                    Title = title,
                    Content = content,
                    RelatedAlertId = alert.AlertId, // ✅ link alert
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow
                });

                await Task.CompletedTask;
            }
        }
    }
}
