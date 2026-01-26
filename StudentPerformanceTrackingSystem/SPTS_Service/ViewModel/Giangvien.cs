
namespace SPTS_Service.ViewModel
{
    public class Giangvien
    {
        public string TeacherName { get; set; }

        // Stats
        public int TotalStudents { get; set; }
        public int NewStudentsThisMonth { get; set; }
        public decimal AverageScore { get; set; }
        public int AtRiskStudents { get; set; }
        public int ActiveSectionsCount { get; set; } // MỚI

        // Sections nhóm theo học kỳ
        public List<TermSectionsViewModel> SectionsByTerm { get; set; }
        // THÊM MỚI - cho dropdown học kỳ
        public List<TermOptionViewModel> AvailableTerms { get; set; } = new();
        public int? SelectedTermId { get; set; }
        public List<AlertViewModel> RecentAlerts { get; set; }
        public List<ChartDataViewModel> ChartData { get; set; }
    }
    public class TermSectionsViewModel
    {
        public int TermId { get; set; }
        public string TermName { get; set; }
        public DateTime? TermStartDate { get; set; }
        public List<SectionCardViewModel> Sections { get; set; }
    }

    public class TermOptionViewModel
    {
        public int TermId { get; set; }
        public string TermName { get; set; }
        public bool IsSelected { get; set; }
    }

    public class SectionCardViewModel
    {
        public int SectionId { get; set; }
        public string CourseCode { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public string Room { get; set; } = string.Empty;
        public int StudentCount { get; set; }
        public decimal CompletionRate { get; set; }
        public string ColorClass { get; set; } = string.Empty;

        // THÊM
        public int Credits { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Schedule { get; set; } = string.Empty;
        public string TimeSlot { get; set; } = string.Empty;
        public decimal? AverageScore { get; set; }
    }

    public class AlertViewModel
    {
        public string StudentName { get; set; } = string.Empty;
        public string AlertType { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty;
        public string IconName { get; set; } = string.Empty;
        public string IconColor { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public int NewAlertsCount { get; set; }
    }

    public class ChartDataViewModel
    {
        public int TermId { get; set; }
        public string TermName { get; set; } = string.Empty;
        public decimal AverageGpa { get; set; }
    }

    public class ChiTietLopVm
    {
        public int SectionId { get; set; }
        public string CourseCode { get; set; } = "";
        public string CourseName { get; set; } = "";
        public string TermName { get; set; } = "";
        public string Room { get; set; } = "";
        public string ScheduleText { get; set; } = "";

        public string SectionStatus { get; set; } = "";  // OPEN/UPCOMING/FINISHED
        public int StudentCount { get; set; }
        public decimal? AverageScore { get; set; }       // TB TotalScore
        public decimal PassRatePercent { get; set; }     // % qua môn
        public int AlertCount { get; set; }              // số cảnh báo của lớp

        public List<StudentGradeRowVm> Students { get; set; } = new();

        // NEW: pagination + search
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalStudents { get; set; } // tổng sau filter
        public string? Search { get; set; }
    }

    public class StudentGradeRowVm
    {
        public int StudentId { get; set; }
        public string StudentCode { get; set; } = "";
        public string FullName { get; set; } = "";
        public DateOnly? DateOfBirth { get; set; }

        public decimal? ProcessScore { get; set; }
        public decimal? FinalScore { get; set; }
        public decimal? TotalScore { get; set; }
    }

    public class SendSectionNotificationRequest
    {
        public int SectionId { get; set; }
        public string Title { get; set; } = "Thông báo từ giảng viên";
        public string Content { get; set; } = "";
    }

    public class SendStudentNotificationRequest
    {
        public int SectionId { get; set; }
        public int StudentId { get; set; }   // cũng chính là UserId
        public string Title { get; set; } = "Thông báo từ giảng viên";
        public string Content { get; set; } = "";
    }

    public class ThongBaoPageVm
    {
        public int SectionId { get; set; }
        public string CourseCode { get; set; } = "";
        public string CourseName { get; set; } = "";
        public string TermName { get; set; } = "";
        public int StudentCount { get; set; }

        // Danh sách các lớp của giảng viên (để render phần "Chọn Lớp Học")
        public List<SectionOptionVm> AvailableSections { get; set; } = new();

        // Gửi broadcast
        public string? BroadcastTitle { get; set; }
        public string? BroadcastContent { get; set; }

        // Danh sách sinh viên
        public List<StudentRowVm> Students { get; set; } = new();

        // Phân trang
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class SectionOptionVm
    {
        public int SectionId { get; set; }
        public string CourseCode { get; set; } = "";
        public string CourseName { get; set; } = "";
        public int StudentCount { get; set; }
        public bool IsSelected { get; set; }
    }

    public class StudentRowVm
    {
        public int StudentId { get; set; }
        public string StudentCode { get; set; } = "";
        public string FullName { get; set; } = "";

        // Tình trạng học tập
        public string StatusLabel { get; set; } = "Bình thường";
        public string StatusBadgeClass { get; set; } = "bg-slate-100";
        public string StatusIcon { get; set; } = "remove";
        public string StatusDetail { get; set; } = "";
        public string StatusColorClass { get; set; } = "bg-slate-500";
    }

    public class GiangVienProfileVm
    {
        public int TeacherId { get; set; }
        public string FullName { get; set; } = "";
        public string RoleLabel { get; set; } = "Giảng viên";
        public string Email { get; set; } = "";
        public string Status { get; set; } = "";          // Active/Inactive

        public string? TeacherCode { get; set; }          // ví dụ GV-2024-0892
        public string? Department { get; set; }           // Khoa/Bộ môn
        public string? Degree { get; set; } = "Thạc sĩ";  // Học vị
        public string? Phone { get; set; }
        public DateTime? Birthday { get; set; }
    } 
}
