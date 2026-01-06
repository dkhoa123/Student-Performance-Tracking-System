
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
}
