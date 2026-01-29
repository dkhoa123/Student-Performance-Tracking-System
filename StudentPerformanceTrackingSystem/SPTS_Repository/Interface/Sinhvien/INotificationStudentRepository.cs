using SPTS_Repository.DTOs.Sinhvien;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPTS_Repository.Interface.Sinhvien
{
    public interface INotificationStudentRepository
    {
        Task<List<NotificationDto>> GetNotificationsAsync(
        int studentId, string filter, int skip, int take);

        Task<int> GetNotificationCountAsync(int studentId, string filter);
        Task<int> GetUnreadCountAsync(int studentId);
        Task MarkAsReadAsync(int notificationId, int studentId);
        Task MarkAllAsReadAsync(int studentId);
    }
}
