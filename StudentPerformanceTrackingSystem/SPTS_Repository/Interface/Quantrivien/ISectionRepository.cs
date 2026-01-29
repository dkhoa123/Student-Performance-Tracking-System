using SPTS_Repository.Entities;

namespace SPTS_Repository.Interface.Admin
{
    public interface ISectionRepository
    {
        Task<List<Teacher>> GetAvailableTeachersAsync(int? termId = null);
        Task<Dictionary<int, int>> GetTeacherSectionCountsAsync(int? termId = null); // ✅ THÊM
        Task<bool> AssignTeacherToSectionAsync(int sectionId, int teacherId);
        Task<bool> UnassignTeacherFromSectionAsync(int sectionId);
    }
}
