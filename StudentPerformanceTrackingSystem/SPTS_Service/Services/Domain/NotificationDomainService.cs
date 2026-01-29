using Microsoft.EntityFrameworkCore;
using SPTS_Repository.Entities;
using SPTS_Shared.Constants;
using SPTS_Service.Interface.Domain;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SPTS_Service.Services.Domain
{
    public class NotificationDomainService : INotificationDomainService
    {
        private readonly SptsContext _context;

        public NotificationDomainService(SptsContext context)
        {
            _context = context;
        }

        public async Task SendAlertNotificationAsync(int studentId, Alert alert)
        {
            var title = "Cảnh báo học tập";
            var content = GetAlertContent(alert.AlertType);

            var notification = new Notification
            {
                UserId = studentId,
                Title = title,
                Content = content,
                RelatedAlertId = alert.AlertId,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();
        }

        public async Task<int> SendToSectionAsync(int sectionId, string title, string content)
        {
            var studentIds = await _context.SectionStudents
                .Where(ss => ss.SectionId == sectionId && ss.Status == UserStatus.Active)
                .Select(ss => ss.StudentId)
                .Distinct()
                .ToListAsync();

            if (!studentIds.Any())
                return 0;

            var now = DateTime.UtcNow;
            var notifications = studentIds.Select(studentId => new Notification
            {
                UserId = studentId,
                Title = title,
                Content = content,
                RelatedAlertId = null,
                IsRead = false,
                CreatedAt = now
            }).ToList();

            _context.Notifications.AddRange(notifications);
            await _context.SaveChangesAsync();

            return notifications.Count;
        }

        public async Task SendToStudentAsync(int studentId, string title, string content)
        {
            var notification = new Notification
            {
                UserId = studentId,
                Title = title,
                Content = content,
                RelatedAlertId = null,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();
        }

        private string GetAlertContent(string alertType)
        {
            var threshold = GradeThresholds.AlertThreshold;

            return alertType switch
            {
                AlertType.LowProcess =>
                    $"Điểm quá trình của bạn dưới {threshold:0.0}. Vui lòng cải thiện.",
                AlertType.LowFinal =>
                    $"Điểm cuối kỳ của bạn dưới {threshold:0.0}. Vui lòng cải thiện.",
                AlertType.LowTotal =>
                    $"Bạn đang có nguy cơ trượt môn (Tổng kết dưới {threshold:0.0}).",
                _ => "Bạn có một cảnh báo học tập."
            };
        }
    }
}