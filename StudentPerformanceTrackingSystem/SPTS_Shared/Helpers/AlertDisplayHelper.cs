

using SPTS_Shared.Constants;

namespace SPTS_Shared.Helpers
{
    public static class AlertDisplayHelper
    {
        public static string GetIconName(string? alertType)
        {
            if (string.IsNullOrEmpty(alertType))
                return "warning";

            return alertType switch
            {
                AlertType.LowTotal or AlertType.LowFinal or AlertType.LowGpa or AlertType.LowProcess
                    => "trending_down",
                AlertType.Absent
                    => "event_busy",
                AlertType.MissingAssignment
                    => "assignment_late",
                _ => "warning"
            };
        }

        public static string GetIconColor(string? severity)
        {
            if (string.IsNullOrEmpty(severity))
                return "gray";

            return severity switch
            {
                Severity.High => "red",
                Severity.Medium => "orange",
                Severity.Low => "yellow",
                _ => "gray"
            };
        }

        public static string GetDefaultMessage(string? alertType, decimal? actualValue)
        {
            if (string.IsNullOrEmpty(alertType))
                return "Cần chú ý";

            return alertType switch
            {
                AlertType.LowTotal => $"Điểm tổng kết dưới {actualValue:0.0}",
                AlertType.LowFinal => $"Điểm cuối kỳ dưới {actualValue:0.0}",
                AlertType.LowGpa => $"GPA dưới {actualValue:0.0}",
                AlertType.LowProcess => $"Điểm quá trình dưới {actualValue:0.0}",
                _ => "Cần chú ý"
            };
        }

        public static string GetStatusLabel(string? alertType, string? severity)
        {
            if (string.IsNullOrEmpty(alertType))
                return "Bình thường";

            return alertType switch
            {
                AlertType.LowTotal or AlertType.LowFinal or AlertType.LowGpa
                    => "Điểm kém",
                AlertType.Absent
                    => "Vắng nhiều",
                AlertType.MissingAssignment
                    => "Thiếu bài tập",
                _ => "Cảnh báo"
            };
        }

        public static string GetStatusBadgeClass(string? severity)
        {
            if (string.IsNullOrEmpty(severity))
                return "bg-slate-100 dark:bg-slate-800 text-slate-600 dark:text-slate-400 border-slate-200 dark:border-slate-700";

            return severity switch
            {
                Severity.High
                    => "bg-red-50 dark:bg-red-900/20 text-red-600 dark:text-red-400 border-red-100 dark:border-red-900/30",
                Severity.Medium
                    => "bg-orange-50 dark:bg-orange-900/20 text-orange-600 dark:text-orange-400 border-orange-100 dark:border-orange-900/30",
                Severity.Low
                    => "bg-yellow-50 dark:bg-yellow-900/20 text-yellow-600 dark:text-yellow-400 border-yellow-100 dark:border-yellow-900/30",
                _ => "bg-slate-100 dark:bg-slate-800 text-slate-600 dark:text-slate-400 border-slate-200 dark:border-slate-700"
            };
        }

        public static string GetStatusIcon(string? alertType)
        {
            return GetIconName(alertType);
        }

        public static string GetStatusDetail(string? alertType, decimal? actualValue)
        {
            if (string.IsNullOrEmpty(alertType))
                return "";

            return alertType switch
            {
                AlertType.LowTotal or AlertType.LowFinal or AlertType.LowGpa
                    => $"TB: {actualValue:F1}/10",
                AlertType.Absent
                    => $"Vắng: {actualValue} buổi",
                _ => ""
            };
        }

        public static string GetStatusColor(string? severity)
        {
            if (string.IsNullOrEmpty(severity))
                return "bg-emerald-500";

            return severity switch
            {
                Severity.High => "bg-red-500",
                Severity.Medium => "bg-orange-500",
                Severity.Low => "bg-yellow-500",
                _ => "bg-emerald-500"
            };
        }
    }
}