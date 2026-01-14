using SPTS_Repository.Entities;


namespace SPTS_Repository.Interface
{
    public interface IAdminRepository
    {
        Task<KPIScorecardDto> GetKPIScorecard(int? termId = null);
        Task<List<DepartmentGPADto>> GetDepartmentGPAs(int? termId = null);
        Task<AcademicRankingDto> GetAcademicRanking(int? termId = null);
        Task<List<DepartmentAlertDto>> GetDepartmentAlerts(int? termId = null);
        Task<(int currentStudents, int previousStudents)> GetStudentComparison(int? currentTermId, int? previousTermId);

        // User Management
        Task<(List<User> Users, int TotalCount)> GetUsersAsync(
            string? role,
            string? status,
            string? keyword,
            int page,
            int pageSize);

        Task<User?> GetUserByIdAsync(int userId);

        Task<bool> SetUserStatusAsync(int userId, string newStatus); // ACTIVE/LOCKED...

        Task<(List<Section> Sections, int TotalCount)> GetSectionsForAdminAsync(int? termId, int page, int pageSize);

        Task<int> CountCoursesAsync();
        Task<int> CountTeachingTeachersAsync(int? termId);
        Task<int> CountSectionsAsync(int? termId);
        Task<int> CountUnassignedSectionsAsync(int? termId);

        Task<List<Term>> GetTermsAsync();            // NEW
        Task<Term?> GetTermByIdAsync(int termId);    // NEW
    }

    // Sử dụng class thay vì record (recommended cho compatibility)
    public class KPIScorecardDto
    {
        public int TotalStudents { get; set; }
        public int TotalTeachers { get; set; }
        public decimal AverageGPA { get; set; }
        public decimal AlertRate { get; set; }
        public int TotalAlerts { get; set; }
        public decimal StudentTeacherRatio { get; set; }
    }

    public class DepartmentGPADto
    {
        public string DepartmentName { get; set; } = string.Empty;
        public decimal AverageGPA { get; set; }
        public int StudentCount { get; set; }
    }

    public class AcademicRankingDto
    {
        public decimal ExcellentRate { get; set; }
        public decimal GoodRate { get; set; }
        public decimal AverageRate { get; set; }
        public decimal BelowAverageRate { get; set; }
        public decimal PoorRate { get; set; }
    }

    public class DepartmentAlertDto
    {
        public string DepartmentName { get; set; } = string.Empty;
        public string DepartmentCode { get; set; } = string.Empty;
        public int TotalStudents { get; set; }
        public int AlertCount { get; set; }
        public decimal AlertRate { get; set; }
    }
}