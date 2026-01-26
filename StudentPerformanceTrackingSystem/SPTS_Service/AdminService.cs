using SPTS_Repository.Interface;
using SPTS_Service.Interface;
using SPTS_Service.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SPTS_Service
{
    public class AdminService : IAdminService
    {
        private readonly IAdminRepository _adRepo;

        public AdminService(IAdminRepository adRepo)
        {
            _adRepo = adRepo;
        }

        public async Task<AdminVM> GetSystemStatistics(int? termId = null)
        {
            // ✅ Khi termId = null → query toàn bộ dữ liệu
            // ✅ Khi termId có giá trị → query theo kỳ cụ thể

            var kpiDto = await _adRepo.GetKPIScorecard(termId);
            var deptGPADtos = await _adRepo.GetDepartmentGPAs(termId);
            var rankingDto = await _adRepo.GetAcademicRanking(termId);
            var alertDtos = await _adRepo.GetDepartmentAlerts(termId);

            return new AdminVM
            {
                KPI = MapToKPIScorecard(kpiDto),
                DepartmentGPAs = MapToDepartmentGPAs(deptGPADtos),
                AcademicRanking = MapToAcademicRanking(rankingDto),
                DepartmentAlerts = MapToDepartmentAlerts(alertDtos)
            };
        }

        private KPIScorecard MapToKPIScorecard(KPIScorecardDto dto)
            => new KPIScorecard
            {
                TotalStudents = dto.TotalStudents,
                TotalTeachers = dto.TotalTeachers,
                AverageGPA = dto.AverageGPA,
                AlertRate = dto.AlertRate,
                TotalAlerts = dto.TotalAlerts,
                StudentTeacherRatio = dto.StudentTeacherRatio,
                StudentGrowthRate = 0,
                GPAChange = 0
            };

        private List<DepartmentGPA> MapToDepartmentGPAs(List<DepartmentGPADto> dtos)
            => dtos.Select(dto => new DepartmentGPA
            {
                DepartmentName = dto.DepartmentName,
                AverageGPA = dto.AverageGPA,
                StudentCount = dto.StudentCount
            }).ToList();

        private AcademicRanking MapToAcademicRanking(AcademicRankingDto dto)
            => new AcademicRanking
            {
                ExcellentRate = dto.ExcellentRate,
                GoodRate = dto.GoodRate,
                AverageRate = dto.AverageRate,
                BelowAverageRate = dto.BelowAverageRate,
                PoorRate = dto.PoorRate
            };

        private List<DepartmentAlert> MapToDepartmentAlerts(List<DepartmentAlertDto> dtos)
            => dtos.Select(dto => new DepartmentAlert
            {
                DepartmentName = dto.DepartmentName,
                DepartmentCode = dto.DepartmentCode,
                TotalStudents = dto.TotalStudents,
                AlertCount = dto.AlertCount,
                AlertRate = dto.AlertRate
            }).ToList();

        public async Task<AdminUsersVM> GetUsersPageAsync(string? role, string? status, string? keyword, int page, int pageSize)
        {
            var (users, total) = await _adRepo.GetUsersAsync(role, status, keyword, page, pageSize);

            return new AdminUsersVM
            {
                Role = role,
                Status = status,
                Keyword = keyword,
                Page = page <= 0 ? 1 : page,
                PageSize = pageSize <= 0 ? 10 : pageSize,
                TotalCount = total,
                Users = users.Select(u => new UserRowVM
                {
                    UserId = u.UserId,
                    FullName = u.FullName ?? "",
                    Email = u.Email ?? "",
                    Role = u.Role ?? "",
                    Status = u.Status ?? "",
                    Initials = BuildInitials(u.FullName)
                }).ToList()
            };
        }

        public Task<bool> LockUserAsync(int userId)
            => _adRepo.SetUserStatusAsync(userId, "LOCKED");

        public Task<bool> UnlockUserAsync(int userId)
            => _adRepo.SetUserStatusAsync(userId, "ACTIVE");

        private static string BuildInitials(string? fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName)) return "?";
            var parts = fullName.Trim().Split(' ', System.StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 1) return parts[0].Substring(0, 1).ToUpper();
            return (parts[0].Substring(0, 1) + parts[^1].Substring(0, 1)).ToUpper();
        }

        public async Task<AdminCourseTeacherVM> GetCourseTeacherPageAsync(int? termId, int page, int pageSize)
        {
            var terms = await _adRepo.GetTermsAsync();

            // Nếu chưa chọn termId => lấy term mới nhất (nếu có)
            if (!termId.HasValue && terms.Count > 0)
                termId = terms[0].TermId;

            var selectedTerm = termId.HasValue ? await _adRepo.GetTermByIdAsync(termId.Value) : null;

            var totalCourses = await _adRepo.CountCoursesAsync();
            var totalSections = await _adRepo.CountSectionsAsync(termId);
            var teachingTeachers = await _adRepo.CountTeachingTeachersAsync(termId);
            var unassigned = await _adRepo.CountUnassignedSectionsAsync(termId);

            var (sections, totalCount) = await _adRepo.GetSectionsForAdminAsync(termId, page, pageSize);

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
                        TeacherName = assigned ? (s.Teacher?.TeacherNavigation?.FullName ?? "N/A") : null,

                        StatusText = assigned ? "Đang dạy" : "Chờ sắp xếp",
                        StatusBadge = assigned ? "GREEN" : "YELLOW"
                    };
                }).ToList()
            };
        }

        public async Task<List<MajorOptionVM>> GetMajorsAsync()
        {
            var majors = await _adRepo.GetMajorsAsync();
            return majors.Select(m => new MajorOptionVM
            {
                MajorCode = m,
                MajorName = m
            }).ToList();
        }

        public async Task<List<DepartmentOptionVM>> GetDepartmentsAsync()
        {
            var departments = await _adRepo.GetDepartmentsAsync();
            return departments.Select(d => new DepartmentOptionVM
            {
                DepartmentId = d.DepartmentId,
                DepartmentName = d.DepartmentName ?? "",
                DepartmentCode = d.DepartmentCode ?? ""
            }).ToList();
        }
        public async Task<UserDetailVM?> GetUserDetailAsync(int userId)
        {
            var dto = await _adRepo.GetUserDetailAsync(userId);
            if (dto == null) return null;

            return new UserDetailVM
            {
                UserId = dto.UserId,
                FullName = dto.FullName,
                Email = dto.Email,
                Role = dto.Role,
                Status = dto.Status,
                StudentCode = dto.StudentCode,
                TeacherCode = dto.TeacherCode
            };
        }

        public Task<bool> UpdateUserAsync(UserUpdateVM vm)
        {
            return _adRepo.UpdateUserAsync(new UserUpdateDto
            {
                UserId = vm.UserId,
                FullName = vm.FullName,
                Email = vm.Email,
                Role = vm.Role,
                Status = vm.Status,
                StudentCode = vm.StudentCode,
                Major = vm.Major,                  // ✅ THÊM
                CohortYear = vm.CohortYear,        // ✅ THÊM
                DepartmentId = vm.DepartmentId,    // ✅ THÊM
                TeacherCode = vm.TeacherCode,
                Degree = vm.Degree,                // ✅ THÊM
                DepartmentName = vm.DepartmentName // ✅ THÊM
            });
        }

        public Task<bool> DeleteUserAsync(int userId)
        {
            return _adRepo.DeleteUserAsync(userId);
        }

        // ✅ Dùng TermOptionVM có sẵn
        public async Task<List<TermOptionVM>> GetTermsForDropdownAsync()
        {
            var terms = await _adRepo.GetTermsAsync();

            var result = new List<TermOptionVM>
            {
                new TermOptionVM
                {
                    TermId = 0, // Giá trị đặc biệt cho "Tất cả"
                    TermName = "-- Tất cả học kỳ --"
                }
            };

            result.AddRange(terms.Select(t => new TermOptionVM
            {
                TermId = t.TermId,
                TermName = t.TermName ?? $"Học kỳ {t.TermId}"
            }));

            return result;
        }
    }
}