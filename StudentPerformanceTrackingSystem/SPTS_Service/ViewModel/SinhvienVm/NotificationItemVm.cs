using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPTS_Service.ViewModel.SinhvienVm
{
    public class NotificationItemVm
    {
        public int NotificationId { get; set; }
        public string Title { get; set; } = "";
        public string Content { get; set; } = "";
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }

        // T�� RelatedAlert (nếu có)
        public string? AlertType { get; set; }
        public string? Severity { get; set; }

        // UI helpers
        public string IconName { get; set; } = "notifications";
        public string IconColor { get; set; } = "bg-blue-50 dark:bg-blue-900/20 text-blue-600 dark: text-blue-400";
        public string BorderColor { get; set; } = "";
        public string BadgeText { get; set; } = "Thông báo chung";
        public string BadgeClass { get; set; } = "bg-[#f0f2f4] text-[#617589] dark:bg-[#374151] dark:text-[#9ca3af]";
        public string TimeAgo { get; set; } = "";
    }
}
