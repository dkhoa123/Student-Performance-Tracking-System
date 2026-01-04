using SPTS_Repository.Entities;

namespace SPTS_Repository.Interface
{
    public interface ISinhVienRepository
    {
        Task DangKysv(User user, Student student);
        Task<string?> LayMaLonNhat(string prefix);
        Task<User?> TimEmail(string email);

        Task<int> GetCurrentTermIdAsync();

        Task<StudentIdentityDto> GetStudentIdentityAsync(int studentId);

        Task<TermGpaDto?> GetTermGpaAsync(int studentId, int termId);

        Task<List<CourseProgressDto>> GetCourseProgressAsync(int studentId, int termId);

        Task<List<AlertDto>> GetAlertsAsync(int studentId, int termId, int take = 10);

        Task<CumulativeGpaDto?> GetCumulativeGpaAsync(int studentId);
        public Task<int> GetCreditsEarnedCumulativeAsync(int studentId);

        Task<CurrentTermDto?> GetCurrentTermAsync();

        Task<List<TermOptionDto>> GetTermsAsync();

        Task UpdateStudentAsync(StudentIdentityDto dto);
    }
    public record StudentIdentityDto(int? StudentId, string StudentCode, string FullName, string Email, string Major, DateOnly? DateOfBirth, string? Gender, string? Phone, string? Address, string status);
    public record TermGpaDto(decimal? GpaValue, int? CreditsAttempted, int? CreditsEarned);
    public record CourseProgressDto(string CourseCode, string CourseName, string TeacherName, int Credit, decimal? ProcessScore, decimal? FinalScore, decimal? TotalScore, decimal? GpaPoint, string? Letter);
    public record AlertDto(int AlertId, string AlertType, string Severity, string? CourseCode, string? Reason, DateTime CreatedAt);
    public record CumulativeGpaDto(decimal? GpaValue);
    // DTO
    public record CurrentTermDto(int TermId, string TermName);

    public record TermOptionDto(int TermId, string TermName);
}
