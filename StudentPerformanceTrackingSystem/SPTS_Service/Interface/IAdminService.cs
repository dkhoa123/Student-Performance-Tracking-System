using SPTS_Service.ViewModels;

namespace SPTS_Service.Interface
{
    public interface IAdminService
    {
        Task<AdminVM> GetSystemStatistics(int? termId = null);
        Task<AdminUsersVM> GetUsersPageAsync(string? role, string? status, string? keyword, int page, int pageSize);
        Task<bool> LockUserAsync(int userId);
        Task<bool> UnlockUserAsync(int userId);

        // quan ly giang vien
        Task<AdminCourseTeacherVM> GetCourseTeacherPageAsync(int? termId, int page, int pageSize);
    }
}
