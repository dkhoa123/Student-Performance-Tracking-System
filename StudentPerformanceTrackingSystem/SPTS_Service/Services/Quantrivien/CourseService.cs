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
    public class CourseService : ICourseService
    {
        private readonly ISectionRepository _sectionRepo;
        private readonly ITermRepository _termRepo;
        private readonly IStatisticsRepository _staticRepo;
        private readonly ISectionManagementRepository _secmanaRepo;
        public CourseService(
            ISectionRepository sectionRepo, 
            ITermRepository termRepo,
            IStatisticsRepository staticRepo,
            ISectionManagementRepository secmanaRepo)
        {
            _sectionRepo = sectionRepo;
            _termRepo = termRepo;
            _staticRepo = staticRepo;
            _secmanaRepo = secmanaRepo;
        }
        public async Task<AdminCourseTeacherVM> GetCourseTeacherPageAsync(int? termId, int page, int pageSize)
        {
            var terms = await _termRepo.GetTermsAsync();

            // Nếu chưa chọn termId => lấy term mới nhất (nếu có)
            if (!termId.HasValue && terms.Count > 0)
                termId = terms[0].TermId;

            var selectedTerm = termId.HasValue ? await _termRepo.GetTermByIdAsync(termId.Value) : null;

            var totalCourses = await _staticRepo.CountCoursesAsync();
            var totalSections = await _staticRepo.CountSectionsAsync(termId);
            var teachingTeachers = await _staticRepo.CountTeachingTeachersAsync(termId);
            var unassigned = await _staticRepo.CountUnassignedSectionsAsync(termId);

            var (sections, totalCount) = await _secmanaRepo.GetSectionsForAdminAsync(termId, page, pageSize);

            return new AdminCourseTeacherVM
            {
                TermId = termId,
                TermName = selectedTerm?.TermName,

                Terms = terms.Select(t => new TermOptionVM
                {
                    TermId = t.TermId,
                    TermName = t.TermName
                }).ToList(),

                Page = page <= 0 ? 1 : page,
                PageSize = pageSize <= 0 ? 10 : pageSize,
                TotalCount = totalCount,

                TotalCourses = totalCourses,
                TotalSections = totalSections,
                TeachingTeachers = teachingTeachers,
                UnassignedSections = unassigned,

                Rows = sections.Select(s =>
                {
                    var assigned = s.TeacherId != null;
                    return new CourseTeacherRowVM
                    {
                        SectionId = s.SectionId,
                        CourseCode = s.Course?.CourseCode ?? "",
                        CourseName = s.Course?.CourseName ?? "",
                        Credits = s.Course?.Credits ?? 0,

                        TeacherId = s.TeacherId,
                        TeacherName = assigned ? s.Teacher?.TeacherNavigation?.FullName ?? "N/A" : null,

                        StatusText = assigned ? "Đang dạy" : "Chờ sắp xếp",
                        StatusBadge = assigned ? "GREEN" : "YELLOW"
                    };
                }).ToList()
            };
        }

        public async Task<SectionDetailVM?> GetSectionDetailAsync(int sectionId)
        {
            var section = await _secmanaRepo.GetSectionByIdAsync(sectionId);
            if (section == null) return null;

            return new SectionDetailVM
            {
                SectionId = section.SectionId,
                SectionCode = section.SectionCode ?? "",
                CourseCode = section.Course?.CourseCode ?? "",
                CourseName = section.Course?.CourseName ?? "",
                Credits = section.Course?.Credits ?? 0,
                TermName = section.Term?.TermName ?? "",
                TeacherId = section.TeacherId,
                TeacherName = section.Teacher?.TeacherNavigation?.FullName,
                TeacherCode = section.Teacher?.TeacherCode
            };
        }
    }
}
