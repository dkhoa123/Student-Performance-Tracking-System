using SPTS_Service.ViewModels;

namespace SPTS_Service.Interface
{
    public interface IAdminService
    {
        Task<AdminVM> GetSystemStatistics(int? termId = null);
        Task<AdminUsersVM> GetUsersPageAsync(string? role, string? status, string? keyword, int page, int pageSize);
        Task<bool> LockUserAsync(int userId);
        Task<bool> UnlockUserAsync(int userId);
        Task<List<TermOptionVM>> GetTermsForDropdownAsync();
        // quan ly giang vien
        Task<AdminCourseTeacherVM> GetCourseTeacherPageAsync(int? termId, int page, int pageSize);
        // update user
        Task<UserDetailVM?> GetUserDetailAsync(int userId);
        Task<bool> UpdateUserAsync(UserUpdateVM vm);
        Task<bool> DeleteUserAsync(int userId);
        Task<List<MajorOptionVM>> GetMajorsAsync();
        Task<List<DepartmentOptionVM>> GetDepartmentsAsync();

        // section-teacher management
        Task<List<TeacherOptionVM>> GetAvailableTeachersAsync(int? termId = null);
        Task<bool> AssignTeacherToSectionAsync(int sectionId, int teacherId);
        Task<bool> UnassignTeacherFromSectionAsync(int sectionId);
        Task<SectionDetailVM?> GetSectionDetailAsync(int sectionId);
    }
}
