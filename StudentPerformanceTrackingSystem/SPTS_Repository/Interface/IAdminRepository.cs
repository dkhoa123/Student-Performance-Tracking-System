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


        // Update user 
        Task<UserDetailDto?> GetUserDetailAsync(int userId);
        Task<bool> UpdateUserAsync(UserUpdateDto dto);
        Task<bool> DeleteUserAsync(int userId);
        Task<List<Department>> GetDepartmentsAsync();
        Task<List<string>> GetMajorsAsync();

        // ✅ THÊM: Quản lý Section-Teacher
        Task<List<Teacher>> GetAvailableTeachersAsync(int? termId = null);
        Task<Dictionary<int, int>> GetTeacherSectionCountsAsync(int? termId = null); // ✅ THÊM
        Task<bool> AssignTeacherToSectionAsync(int sectionId, int teacherId);
        Task<bool> UnassignTeacherFromSectionAsync(int sectionId);
        Task<Section?> GetSectionByIdAsync(int sectionId);
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
        public string DepartmentName { get; set; } = "";
        public string DepartmentCode { get; set; } = "";
        public int TotalStudents { get; set; }      // Tổng SV của khoa
        public int AlertCount { get; set; }         // Số SV bị cảnh báo (distinct)
        public decimal AlertRate { get; set; }      // Tỷ lệ % SV bị CB
    }

    public class UserUpdateDto
    {
        public int UserId { get; set; }
        public string FullName { get; set; } = "";
        public string Email { get; set; } = "";
        public string Role { get; set; } = "";
        public string Status { get; set; } = "";

        public string? StudentCode { get; set; }
        public string? Major { get; set; }           // ✅ THÊM
        public int? CohortYear { get; set; }         // ✅ THÊM
        public int? DepartmentId { get; set; }       // ✅ THÊM

        public string? TeacherCode { get; set; }
        public string? Degree { get; set; }          // ✅ THÊM
        public string? DepartmentName { get; set; }  // ✅ THÊM
    }

    public class UserDetailDto
    {
        public int UserId { get; set; }
        public string FullName { get; set; } = "";
        public string Email { get; set; } = "";
        public string Role { get; set; } = "";
        public string Status { get; set; } = "";

        public string? StudentCode { get; set; }
        public string? Major { get; set; }
        public int? CohortYear { get; set; }
        public int? DepartmentId { get; set; }

        public string? TeacherCode { get; set; }
        public string? Degree { get; set; }
        public string? DepartmentName { get; set; }
    }
}