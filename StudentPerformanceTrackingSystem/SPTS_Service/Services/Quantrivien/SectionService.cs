using SPTS_Repository.Interface.Admin;
using SPTS_Service.Interface.Admin;
using SPTS_Service.ViewModel.QuantrivienVm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPTS_Service.Services.Quantrivien
{
    public class SectionService : ISectionService
    {
        private readonly ISectionRepository _sectionRepo;
        public SectionService(ISectionRepository sectionRepo)
        {
            _sectionRepo = sectionRepo;
        }
        public Task<bool> AssignTeacherToSectionAsync(int sectionId, int teacherId)
            => _sectionRepo.AssignTeacherToSectionAsync(sectionId, teacherId);

        public async Task<List<TeacherOptionVM>> GetAvailableTeachersAsync(int? termId = null)
        {
            // Lấy tất cả giảng viên
            var teachers = await _sectionRepo.GetAvailableTeachersAsync(termId);

            // Lấy số môn đang dạy của từng giảng viên trong kỳ này
            var sectionCounts = await _sectionRepo.GetTeacherSectionCountsAsync(termId);

            var result = teachers.Select(t => new TeacherOptionVM
            {
                TeacherId = t.TeacherId,
                TeacherCode = t.TeacherCode ?? "",
                FullName = t.TeacherNavigation?.FullName ?? "",
                Email = t.TeacherNavigation?.Email ?? "",
                Degree = t.Degree,
                DepartmentName = t.DemparmentName,
                SectionCount = sectionCounts.ContainsKey(t.TeacherId) ? sectionCounts[t.TeacherId] : 0
            })
            .OrderBy(t => t.SectionCount)  // ✅ Ưu tiên GV có ít môn (0 môn lên đầu)
            .ThenBy(t => t.FullName)        // ✅ Sau đó sắp xếp theo tên
            .ToList();

            return result;
        }

        public Task<bool> UnassignTeacherFromSectionAsync(int sectionId)
            => _sectionRepo.UnassignTeacherFromSectionAsync(sectionId);

    }
}
