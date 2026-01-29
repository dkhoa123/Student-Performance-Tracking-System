using Microsoft.EntityFrameworkCore;
using SPTS_Repository.Entities;
using SPTS_Shared.Constants;
using SPTS_Service.Interface.Domain;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SPTS_Service.Services.Domain
{
    public class AlertSyncService : IAlertSyncService
    {
        private readonly SptsContext _context;
        private readonly INotificationDomainService _notificationService;

        public AlertSyncService(
            SptsContext context,
            INotificationDomainService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }

        public async Task SyncAlertsForGradeAsync(
            int sectionId,
            int studentId,
            decimal? processScore,
            decimal? finalScore,
            decimal? totalScore)
        {
            var termId = await GetTermIdAsync(sectionId);

            // Process score alert
            var (newProcess, processAlert) = await UpsertOrDeleteAlertAsync(
                sectionId,
                studentId,
                termId,
                AlertType.LowProcess,
                Severity.Low,
                processScore,
                $"Điểm quá trình dưới {GradeThresholds.AlertThreshold:0.0}"
            );

            // Final score alert
            var (newFinal, finalAlert) = await UpsertOrDeleteAlertAsync(
                sectionId,
                studentId,
                termId,
                AlertType.LowFinal,
                Severity.Medium,
                finalScore,
                $"Điểm cuối kỳ dưới {GradeThresholds.AlertThreshold:0.0}"
            );

            // Total score alert (only when both scores present)
            (bool newTotal, Alert? totalAlert) = (false, null);
            if (processScore.HasValue && finalScore.HasValue)
            {
                (newTotal, totalAlert) = await UpsertOrDeleteAlertAsync(
                    sectionId,
                    studentId,
                    termId,
                    AlertType.LowTotal,
                    Severity.High,
                    totalScore,
                    $"Điểm tổng kết dưới {GradeThresholds.AlertThreshold:0.0}"
                );
            }
            else
            {
                await DeleteAlertsByTypeAsync(sectionId, studentId, AlertType.LowTotal);
            }

            await _context.SaveChangesAsync();

            // Send notifications for new alerts only
            if (newProcess && processAlert != null)
                await _notificationService.SendAlertNotificationAsync(studentId, processAlert);

            if (newFinal && finalAlert != null)
                await _notificationService.SendAlertNotificationAsync(studentId, finalAlert);

            if (newTotal && totalAlert != null)
                await _notificationService.SendAlertNotificationAsync(studentId, totalAlert);
        }

        private async Task<int?> GetTermIdAsync(int sectionId)
        {
            return await _context.Sections
                .Where(s => s.SectionId == sectionId)
                .Select(s => (int?)s.TermId)
                .FirstOrDefaultAsync();
        }

        private async Task<(bool createdNew, Alert? alert)> UpsertOrDeleteAlertAsync(
            int sectionId,
            int studentId,
            int? termId,
            string alertType,
            string severity,
            decimal? actualValue,
            string reason)
        {
            var existing = await _context.Alerts
                .Where(a => a.SectionId == sectionId
                         && a.StudentId == studentId
                         && a.AlertType == alertType)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();

            // Delete if score is good or not present
            if (!actualValue.HasValue || actualValue.Value >= GradeThresholds.AlertThreshold)
            {
                if (existing.Any())
                    _context.Alerts.RemoveRange(existing);
                return (false, null);
            }

            var now = DateTime.UtcNow;

            // Create new alert
            if (!existing.Any())
            {
                var alert = new Alert
                {
                    StudentId = studentId,
                    SectionId = sectionId,
                    TermId = termId,
                    AlertType = alertType,
                    Severity = severity,
                    ThresholdValue = GradeThresholds.AlertThreshold,
                    ActualValue = actualValue,
                    Reason = reason,
                    Status = AlertStatus.New,
                    CreatedAt = now
                };
                _context.Alerts.Add(alert);
                return (true, alert);
            }

            // Update existing alert
            var existingAlert = existing[0];
            existingAlert.TermId = termId;
            existingAlert.Severity = severity;
            existingAlert.ThresholdValue = GradeThresholds.AlertThreshold;
            existingAlert.ActualValue = actualValue;
            existingAlert.Reason = reason;
            existingAlert.Status = AlertStatus.New;
            existingAlert.CreatedAt = now;

            // Remove duplicates
            if (existing.Count > 1)
                _context.Alerts.RemoveRange(existing.Skip(1));

            return (false, existingAlert);
        }

        private async Task DeleteAlertsByTypeAsync(int sectionId, int studentId, string alertType)
        {
            var alerts = await _context.Alerts
                .Where(a => a.SectionId == sectionId
                         && a.StudentId == studentId
                         && a.AlertType == alertType)
                .ToListAsync();

            if (alerts.Any())
                _context.Alerts.RemoveRange(alerts);
        }
    }
}