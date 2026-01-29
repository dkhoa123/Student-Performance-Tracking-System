using SPTS_Repository.Interface.Sinhvien;
using SPTS_Service.Interface.Student;
using SPTS_Service.ViewModel.SinhvienVm;

namespace SPTS_Service.Services.Sinhvien
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationStudentRepository _notiRepo;
        public NotificationService(INotificationStudentRepository notiRepo)
        {
            _notiRepo = notiRepo;
        }
        public async Task<ThongBaoSinhVienVm> GetNotificationsPageAsync(int studentId, string filter = "all", int page = 1, int pageSize = 10)
        {
            var skip = (page - 1) * pageSize;

            var notifications = await _notiRepo.GetNotificationsAsync(studentId, filter, skip, pageSize);
            var totalCount = await _notiRepo.GetNotificationCountAsync(studentId, filter);
            var unreadCount = await _notiRepo.GetUnreadCountAsync(studentId);

            return new ThongBaoSinhVienVm
            {
                Notifications = notifications.Select(n => new NotificationItemVm
                {
                    NotificationId = n.NotificationId,
                    Title = n.Title,
                    Content = n.Content,
                    IsRead = n.IsRead,
                    CreatedAt = n.CreatedAt,
                    AlertType = n.AlertType,
                    Severity = n.Severity,
                    IconName = GetIconName(n.AlertType),
                    IconColor = GetIconColor(n.Severity),
                    BorderColor = GetBorderColor(n.Severity),
                    BadgeText = GetBadgeText(n.AlertType, n.Severity),
                    BadgeClass = GetBadgeClass(n.Severity),
                    TimeAgo = GetTimeAgo(n.CreatedAt)
                }).ToList(),

                TotalCount = totalCount,
                UnreadCount = unreadCount,
                CurrentFilter = filter,
                CurrentPage = page,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
                PageSize = pageSize
            };
        }

        public Task MarkAsReadAsync(int notificationId, int studentId)
            => _notiRepo.MarkAsReadAsync(notificationId, studentId);

        public Task MarkAllAsReadAsync(int studentId)
            => _notiRepo.MarkAllAsReadAsync(studentId);

        // Helper methods
        private static string GetIconName(string? alertType)
        {
            return alertType switch
            {
                "LOW_TOTAL" or "LOW_FINAL" or "LOW_PROCESS" or "LOW_GPA" => "warning",
                "ABSENT" => "school",
                "MISSING_ASSIGNMENT" => "assignment_late",
                _ => "notifications"
            };
        }

        private static string GetIconColor(string? severity)
        {
            return severity switch
            {
                "HIGH" => "bg-red-100 dark:bg-red-900/30 text-red-600 dark:text-red-400",
                "MEDIUM" => "bg-orange-100 dark:bg-orange-900/30 text-orange-600 dark:text-orange-400",
                "LOW" => "bg-yellow-100 dark:bg-yellow-900/30 text-yellow-600 dark:text-yellow-400",
                _ => "bg-blue-50 dark:bg-blue-900/20 text-blue-600 dark:text-blue-400"
            };
        }

        private static string GetBorderColor(string? severity)
        {
            return severity switch
            {
                "HIGH" => "border-l-red-500",
                "MEDIUM" => "border-l-orange-500",
                "LOW" => "border-l-yellow-500",
                _ => ""
            };
        }

        private static string GetBadgeText(string? alertType, string? severity)
        {
            if (string.IsNullOrEmpty(severity)) return "Thông báo chung";

            return severity switch
            {
                "HIGH" => "Nghiêm trọng",
                "MEDIUM" => "Cần chú ý",
                "LOW" => "Lưu ý",
                _ => "Thông báo"
            };
        }

        private static string GetBadgeClass(string? severity)
        {
            return severity switch
            {
                "HIGH" => "bg-red-50 text-red-700 ring-1 ring-inset ring-red-600/10 dark:bg-red-900/30 dark:text-red-400 dark:ring-red-500/20",
                "MEDIUM" => "bg-orange-50 text-orange-700 ring-1 ring-inset ring-orange-600/10 dark: bg-orange-900/30 dark:text-orange-400 dark:ring-orange-500/20",
                "LOW" => "bg-yellow-50 text-yellow-700 ring-1 ring-inset ring-yellow-600/10 dark:bg-yellow-900/30 dark:text-yellow-400 dark:ring-yellow-500/20",
                _ => "bg-[#f0f2f4] text-[#617589] dark:bg-[#374151] dark:text-[#9ca3af]"
            };
        }

        private static string GetTimeAgo(DateTime createdAt)
        {
            var diff = DateTime.UtcNow - createdAt;

            if (diff.TotalMinutes < 1) return "Vừa xong";
            if (diff.TotalMinutes < 60) return $"{(int)diff.TotalMinutes} phút trước";
            if (diff.TotalHours < 24) return $"{(int)diff.TotalHours} giờ trước";
            if (diff.TotalDays < 7) return $"{(int)diff.TotalDays} ngày trước";

            return createdAt.ToString("dd 'Th'MM");
        }
    }
}
