using SPTS_Repository.Entities;

namespace SPTS_Repository.Interface
{
    public interface IGiangvienRepository
    {
        Task<Teacher> GetByUserIdAsync(int userId);
        Task<User> GetTeacherUserAsync(int teacherId);
        // học kỳ hiện tại
        Task<int> GetTotalStudentsByTeacherAsync(int teacherId);
        Task<int> GetNewStudentsThisMonthAsync(int teacherId);
        Task<decimal> GetAverageScoreByTeacherAsync(int teacherId);
        Task<List<SectionCardViewModelDto>> GetSectionsByTeacherAsync(int teacherId);
        //cảnh báo
        Task<int> GetAtRiskStudentsCountAsync(int teacherId);
        Task<List<AlertViewModelDto>> GetRecentAlertsByTeacherAsync(int teacherId, int top = 3);
        //bieu do
        Task<List<ChartDataViewModelDto>> GetGpaChartDataByTeacherAsync(int teacherId, int? TermId = null);
        Task<int> GetActiveSectionsCountAsync(int teacherId);
    }
    //DTO
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
}
