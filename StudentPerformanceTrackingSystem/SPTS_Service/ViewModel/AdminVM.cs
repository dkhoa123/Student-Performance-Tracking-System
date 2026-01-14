namespace SPTS_Service.ViewModels
{
    public class AdminVM
    {
        public KPIScorecard KPI { get; set; }
        public List<DepartmentGPA> DepartmentGPAs { get; set; }
        public AcademicRanking AcademicRanking { get; set; }
        public List<DepartmentAlert> DepartmentAlerts { get; set; }
    }

    public class KPIScorecard
    {
        public int TotalStudents { get; set; }
        public int TotalTeachers { get; set; }
        public decimal AverageGPA { get; set; }
        public decimal AlertRate { get; set; }
        public int TotalAlerts { get; set; }
        public decimal StudentTeacherRatio { get; set; }

        // So sánh với kỳ trước
        public decimal StudentGrowthRate { get; set; }
        public decimal GPAChange { get; set; }
    }

    public class DepartmentGPA
    {
        public string DepartmentName { get; set; }
        public decimal AverageGPA { get; set; }
        public int StudentCount { get; set; }
    }

    public class AcademicRanking
    {
        public decimal ExcellentRate { get; set; }  // GPA >= 3.6
        public decimal GoodRate { get; set; }        // 3.2 - 3.59
        public decimal AverageRate { get; set; }     // 2.5 - 3.19
        public decimal BelowAverageRate { get; set; }// 2.0 - 2.49
        public decimal PoorRate { get; set; }        // < 2.0
    }

    public class DepartmentAlert
    {
        public string DepartmentName { get; set; }
        public string DepartmentCode { get; set; }
        public int TotalStudents { get; set; }
        public int AlertCount { get; set; }
        public decimal AlertRate { get; set; }
    }

    public class AdminUsersVM
    {
        public List<UserRowVM> Users { get; set; } = new();

        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }

        public string? Role { get; set; }
        public string? Status { get; set; }
        public string? Keyword { get; set; }

        public int TotalPages => PageSize <= 0 ? 0 : (int)System.Math.Ceiling((double)TotalCount / PageSize);

        public int From => TotalCount == 0 ? 0 : (Page - 1) * PageSize + 1;
        public int To => System.Math.Min(Page * PageSize, TotalCount);
    }

    public class UserRowVM
    {
        public int UserId { get; set; }
        public string FullName { get; set; } = "";
        public string Email { get; set; } = "";
        public string Role { get; set; } = "";
        public string Status { get; set; } = "";

        public string Initials { get; set; } = ""; // fallback avatar
    }

    public class AdminCourseTeacherVM
    {
        // Stats
        public int TotalCourses { get; set; }
        public int TeachingTeachers { get; set; }
        public int UnassignedSections { get; set; }
        public int TotalSections { get; set; }

        // Filters
        public int? TermId { get; set; }
        public string? TermName { get; set; }

        // dropdown/filter data
        public List<TermOptionVM> Terms { get; set; } = new();

        // Paging
        public int TotalCount { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;

        public int TotalPages => PageSize <= 0 ? 0 : (int)Math.Ceiling((double)TotalCount / PageSize);
        public int From => TotalCount == 0 ? 0 : (Page - 1) * PageSize + 1;
        public int To => Math.Min(Page * PageSize, TotalCount);

        // Table
        public List<CourseTeacherRowVM> Rows { get; set; } = new();

        // helper to render page numbers like: 1 2 3 ... 13
        public List<int> GetPageNumbers(int window = 1)
        {
            var pages = new List<int>();
            if (TotalPages <= 0) return pages;

            void add(int p) { if (!pages.Contains(p)) pages.Add(p); }

            add(1);
            for (int p = Page - window; p <= Page + window; p++)
                if (p > 1 && p < TotalPages) add(p);
            if (TotalPages > 1) add(TotalPages);

            pages.Sort();
            return pages;
        }
    }

    public class TermOptionVM
    {
        public int TermId { get; set; }
        public string TermName { get; set; } = "";
    }

    public class CourseTeacherRowVM
    {
        public int SectionId { get; set; }
        public string CourseCode { get; set; } = "";
        public string CourseName { get; set; } = "";
        public int Credits { get; set; }

        public int? TeacherId { get; set; }
        public string? TeacherName { get; set; }

        public string StatusText { get; set; } = "";
        public string StatusBadge { get; set; } = ""; // GREEN / YELLOW
    }
}