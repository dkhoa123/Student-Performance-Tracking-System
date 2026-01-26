using Microsoft.EntityFrameworkCore;
using SPTS_Repository.Entities;

namespace SPTS_Repository.Interface
{
    public interface IGiangvienRepository
    {

        Task<GiangVienProfileDto> GetProfileAsync(int teacherId);
        Task<string> GetTeacherUserAsync(int teacherId);
        // học kỳ hiện tại
        Task<List<(int TermId, string TermName)>> GetTermsByTeacherAsync(int teacherId);
        Task<int> GetTotalStudentsByTeacherAsync(int teacherId);
        Task<int> GetNewStudentsThisMonthAsync(int teacherId);
        Task<decimal> GetAverageScoreByTeacherAsync(int teacherId, int termId);
        Task<List<SectionCardViewModelDto>> GetSectionsByTeacherAsync(int teacherId);
        Task<decimal?> GetGpaPointByTotalAsync(decimal totalScore);

        Task<int> GetTermIdBySectionAsync(int sectionId);
        Task RecalculateAndUpsertTermGpaAsync(int studentId, int termId);

        //cảnh báo
        Task SyncAlertsForGradeAsync(int sectionId, int studentId, decimal? process, decimal? final, decimal? total);
        Task<int> GetAtRiskStudentsCountAsync(int teacherId);
        Task<List<AlertViewModelDto>> GetRecentAlertsByTeacherAsync(int teacherId, int top = 3);
        Task<int> GetNewAlertsCountAsync(int teacherId);
        //bieu do
        Task<List<ChartDataViewModelDto>> GetGpaChartDataByTeacherAsync(int teacherId, int? TermId = null);
        Task<int> GetActiveSectionsCountAsync(int teacherId);

        Task<ChiTietLopDto> GetSectionDetailAsync(int sectionId);
        Task<int> GetAlertCountBySectionAsync(int sectionId);
        Task<GradeRule?> GetActiveGradeRuleBySectionAsync(int sectionId);
        Task UpsertGradeAsync(int sectionId, int studentId, decimal? process, decimal? final, decimal? total, decimal? gpaPoint);

        Task<List<SectionOptionDto>> GetSectionsForNotificationAsync(int teacherId);
        Task<List<StudentNotificationDto>> GetStudentsWithAlertStatusAsync(int sectionId);

        Task<int> SendToSectionAsync(int sectionId, string title, string content);
        Task SendToStudentAsync(int sectionId, int studentId, string title, string content);

    }
    //DTO

    public record GiangVienProfileDto(
        int TeacherId,
        string TeacherName,
        string Email,
        string Status,
        string Rolelabel,
        string TeacherCode,
        string Department,
        string Degree,
        string Phone,
        DateTime Birthday
    );
    public record SectionCardViewModelDto(
     int SectionId,
     string CourseCode,
     string CourseName,
     string Room,
     int StudentCount,
     decimal CompletionRate,
     string ColorClass,

     int Credits,              // Số tín chỉ
     string Status,            // OPEN, UPCOMING, FINISHED
     string? Schedule,         // "Thứ 2, Tiết 1-3"
     string? TimeSlot,         // "7:00 - 9:30"
     decimal? AverageScore,    // Điểm TB của lớp
     int TermId,              // ID học kỳ
     string TermName,         // Tên học kỳ
     DateTime? StartDate      // Ngày bắt đầu
 );
    public record AlertViewModelDto(string StudentName, string AlertType, string Message, string Severity, string IconName, string IconColor, DateTime CreatedAt);
    public record ChartDataViewModelDto(int TermId, string TermName, decimal AverageGpa);
    public record ChiTietLopDto(
    int SectionId,
    string CourseCode,
    string CourseName,
    string TermName,
    string Room,
    string ScheduleText,
    string SectionStatus,
    List<StudentGradeRowDto> Students
);

    public record StudentGradeRowDto(
        int StudentId,
        string StudentCode,
        string FullName,
        DateOnly? DateOfBirth,
        decimal? ProcessScore,
        decimal? FinalScore,
        decimal? TotalScore
    );

    public record SectionOptionDto(int SectionId, string CourseCode, string CourseName, int StudentCount);

    public record StudentNotificationDto(
        int StudentId,
        string StudentCode,
        string FullName,
        string? AlertType,
        string? Severity,
        decimal? ActualValue
    );

}
