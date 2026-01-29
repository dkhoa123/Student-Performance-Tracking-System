using Microsoft.EntityFrameworkCore;
using SPTS_Repository.DTOs.Sinhvien;
using SPTS_Repository.Entities;
using SPTS_Repository.Interface.Sinhvien;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPTS_Repository.Repositories.Sinhvien
{
    public class NotificationStudentRepository : INotificationStudentRepository
    {
        private readonly SptsContext _db;
        public NotificationStudentRepository(SptsContext context)
        {
            _db = context;
        }
        public async Task<int> GetNotificationCountAsync(int studentId, string filter)
        {
            var query = _db.Notifications.Where(n => n.UserId == studentId);

            query = filter switch
            {
                "unread" => query.Where(n => !n.IsRead),
                "alert" => query.Where(n => n.RelatedAlertId != null),
                _ => query
            };

            return await query.CountAsync();
        }

        public async Task<List<NotificationDto>> GetNotificationsAsync(int studentId, string filter, int skip, int take)
        {
            var query = _db.Notifications
                .Include(n => n.RelatedAlert)
                .Where(n => n.UserId == studentId);

            // Apply filter
            query = filter switch
            {
                "unread" => query.Where(n => !n.IsRead),
                "alert" => query.Where(n => n.RelatedAlertId != null),
                _ => query // "all"
            };

            return await query
                .OrderByDescending(n => n.CreatedAt)
                .Skip(skip)
                .Take(take)
                .Select(n => new NotificationDto(
                    n.NotificationId,
                    n.Title,
                    n.Content,
                    n.IsRead,
                    n.CreatedAt,
                    n.RelatedAlert != null ? n.RelatedAlert.AlertType : null,
                    n.RelatedAlert != null ? n.RelatedAlert.Severity : null
                ))
                .ToListAsync();
        }

        public async Task<int> GetUnreadCountAsync(int studentId)
        {
            return await _db.Notifications
                .Where(n => n.UserId == studentId && !n.IsRead)
                .CountAsync();
        }

        public async Task MarkAllAsReadAsync(int studentId)
        {
            await _db.Notifications
                .Where(n => n.UserId == studentId && !n.IsRead)
                .ExecuteUpdateAsync(setters => setters.SetProperty(n => n.IsRead, true));
        }

        public async Task MarkAsReadAsync(int notificationId, int studentId)
        {
            var noti = await _db.Notifications
                .FirstOrDefaultAsync(n => n.NotificationId == notificationId && n.UserId == studentId);

            if (noti != null && !noti.IsRead)
            {
                noti.IsRead = true;
                await _db.SaveChangesAsync();
            }
        }
    }
}
