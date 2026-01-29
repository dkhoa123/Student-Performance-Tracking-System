using SPTS_Repository.Interface.Giangvien;
using SPTS_Service.Interface.Giangvien;
using SPTS_Service.ViewModel.GiangvienVm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPTS_Service.Services.Giangvien
{
    public class SectionTeacherService : ISectionTeacherService
    {
        private readonly ISectionTeacherRepository _repo;
        public SectionTeacherService(ISectionTeacherRepository repo)
        {
            _repo = repo;
        }
        public async Task<ChiTietLopVm> GetSectionDetailAsync(int sectionId, int page = 1, int pageSize = 10, string? search = null)
        {
            var dto = await _repo.GetSectionDetailAsync(sectionId);
            var alertCount = await _repo.GetAlertCountBySectionAsync(sectionId);

            page = page <= 0 ? 1 : page;
            pageSize = pageSize <= 0 ? 10 : pageSize;

            // FILTER (search theo MSSV hoặc tên)
            var all = dto.Students.AsEnumerable();
            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim();
                all = all.Where(x =>
                    !string.IsNullOrEmpty(x.StudentCode) && x.StudentCode.Contains(s, StringComparison.OrdinalIgnoreCase) ||
                    !string.IsNullOrEmpty(x.FullName) && x.FullName.Contains(s, StringComparison.OrdinalIgnoreCase));
            }

            var total = all.Count();
            var totalPages = (int)Math.Ceiling(total / (double)pageSize);
            totalPages = Math.Max(totalPages, 1);
            page = Math.Min(page, totalPages);

            var studentsPaged = all
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var vm = new ChiTietLopVm
            {
                SectionId = dto.SectionId,
                CourseCode = dto.CourseCode,
                CourseName = dto.CourseName,
                TermName = dto.TermName,
                Room = dto.Room,
                ScheduleText = dto.ScheduleText,
                SectionStatus = dto.SectionStatus,
                AlertCount = alertCount,

                Students = studentsPaged.Select(s => new StudentGradeRowVm
                {
                    StudentId = s.StudentId,
                    StudentCode = s.StudentCode,
                    FullName = s.FullName,
                    DateOfBirth = s.DateOfBirth,
                    ProcessScore = s.ProcessScore,
                    FinalScore = s.FinalScore,
                    TotalScore = s.TotalScore
                }).ToList()
            };

            // Stats header nên tính trên TOÀN BỘ lớp (dto.Students), không phải trang hiện tại
            vm.StudentCount = dto.Students.Count;

            var gradedAll = dto.Students.Where(x => x.TotalScore.HasValue).ToList();
            vm.AverageScore = gradedAll.Any()
                ? Math.Round(gradedAll.Average(x => x.TotalScore!.Value), 1)
                : (decimal?)null;

            vm.PassRatePercent = gradedAll.Any()
                ? Math.Round((decimal)gradedAll.Count(x => x.TotalScore >= 5m) * 100 / gradedAll.Count, 0)
                : 0;

            // Pagination fields (cần thêm property vào ChiTietLopVm ở bước 3)
            vm.CurrentPage = page;
            vm.TotalPages = totalPages;
            vm.PageSize = pageSize;
            vm.TotalStudents = total;
            vm.Search = search;

            return vm;
        }
    }
}
