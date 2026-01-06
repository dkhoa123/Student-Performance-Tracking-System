
namespace SPTS_Service.ViewModel
{
    public class SinhVien
    {
        public int? UserId { get; set; }
        public int? StudentId { get; set; }
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? StudentCode { get; set; }
        public string? Major{ get; set; }
         public string? Status { get; set; }
        public DateOnly? DateOfBirth { get; set; }
        public string? Gender { get; set; }
        public string? Phone { get; set; }
        public string? Address { get; set; }

        // Cards
        public decimal? TermGpa { get; set; }          // TermGpa.gpa_value
        public decimal? CumulativeGpa { get; set; }    // tính từ Grades toàn bộ term
        public int CreditsEarned { get; set; }         // TermGpa.credits_earned (hoặc tự tính)
        public int CreditsAttempted { get; set; }      // TermGpa.credits_attempted
        public int AcademicAlertCount { get; set; }
        public string? CurrentTermName { get; set; }

        // Lists
        public List<CourseProgressVm> CurrentCourses { get; set; } = [];
        public List<AlertVm> Alerts { get; set; } = [];
        public GradeDistributionVm GradeDistribution { get; set; } = new();

        public List<TermOptionVm> Terms { get; set; } = [];
        public int? SelectedTermId { get; set; } // term đang chọn (để selected đúng)
    }

    public class CourseProgressVm
    {
        public string CourseCode { get; set; } = "";
        public string CourseName { get; set; } = "";
        public string TeacherName { get; set; } = "";
        public int Credit { get; set; }              // Courses.credit
        public decimal? ProcessScore { get; set; }   // Grades.process_score

        public decimal? FinalScore { get; set; }     // Grades.final_score
        public decimal? TotalScore { get; set; }   // Grades.total_score
        public decimal? GpaPoint { get; set; }     // Grades.gpa_point
        public string? Letter { get; set; }        // từ GpaScales (optional)
    }

    public class AlertVm
    {
        public int AlertId { get; set; }
        public string AlertType { get; set; } = "";
        public string Severity { get; set; } = "";
        public string? CourseCode { get; set; }
        public string? Reason { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class GradeDistributionVm
    {
        public int A { get; set; }
        public int B { get; set; }
        public int C { get; set; }
        public int DF { get; set; }
    }

    public class TermOptionVm
    {
        public int TermId { get; set; }
        public string TermName { get; set; } = "";
    }
}
