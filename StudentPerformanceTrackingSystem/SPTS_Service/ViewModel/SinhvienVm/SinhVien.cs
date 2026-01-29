

namespace SPTS_Service.ViewModel.SinhvienVm
{
    public class SinhVien
    {
        public int? UserId { get; set; }
        public int? StudentId { get; set; }
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? StudentCode { get; set; }
        public string? Major { get; set; }
        public string? Status { get; set; }
        public DateOnly? DateOfBirth { get; set; }
        public string? Gender { get; set; }
        public string? Phone { get; set; }
        public string? Address { get; set; }

        // Cards
        public decimal? TermGpa { get; set; }          // TermGpa.gpa_value
        public decimal? CumulativeGpa { get; set; }    // tính từ Grades toàn bộ term
        public int CreditsEarnedCumulative { get; set; }   // tín chỉ tích lũy (đậu)
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

        public List<TermGpaTrendVm> TermGpaTrend { get; set; } = new();
    }
}
