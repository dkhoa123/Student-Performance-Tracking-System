using SPTS_Repository.DTOs.Quantrivien;
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
    public class DashboardService : IDashboardService
    {
        private readonly IKPIRepository _kpiRepo;
        private readonly IStatisticsRepository _staticRepo;
        private readonly ITermRepository _termRepo;
        public DashboardService(
            IKPIRepository kpiRepo,
            IStatisticsRepository staticRepo,
            ITermRepository termRepo)
        {
            _kpiRepo = kpiRepo;
            _staticRepo = staticRepo;
            _termRepo = termRepo;
        }
        public async Task<AdminVM> GetSystemStatistics(int? termId = null)
        {
            // ✅ Khi termId = null → query toàn bộ dữ liệu
            // ✅ Khi termId có giá trị → query theo kỳ cụ thể

            var kpiDto = await _kpiRepo.GetKPIScorecard(termId);
            var deptGPADtos = await _staticRepo.GetDepartmentGPAs(termId);
            var rankingDto = await _kpiRepo.GetAcademicRanking(termId);
            var alertDtos = await _staticRepo.GetDepartmentAlerts(termId);

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

        // ✅ Dùng TermOptionVM có sẵn
        public async Task<List<TermOptionVM>> GetTermsForDropdownAsync()
        {
            var terms = await _termRepo.GetTermsAsync();

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
