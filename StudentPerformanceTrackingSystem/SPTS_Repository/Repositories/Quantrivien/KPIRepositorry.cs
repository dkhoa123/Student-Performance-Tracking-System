using Microsoft.EntityFrameworkCore;
using SPTS_Repository.DTOs.Quantrivien;
using SPTS_Repository.Entities;
using SPTS_Repository.Interface;
using SPTS_Repository.Interface.Admin;
using SPTS_Shared.Constants;

namespace SPTS_Repository.Repositories.Admin
{
    public class KPIRepositorry : IKPIRepository
    {
        private readonly SptsContext _context;

        public KPIRepositorry(SptsContext context)
        {
            _context = context;
        }
        public async Task<AcademicRankingDto> GetAcademicRanking(int? termId = null)
        {
            var gpasQuery = _context.TermGpas.AsQueryable();

            if (termId.HasValue)
                gpasQuery = gpasQuery.Where(tg => tg.TermId == termId.Value);

            var totalCount = await gpasQuery.CountAsync();

            if (totalCount == 0)
            {
                return new AcademicRankingDto
                {
                    ExcellentRate = 0,
                    GoodRate = 0,
                    AverageRate = 0,
                    BelowAverageRate = 0,
                    PoorRate = 0
                };
            }

            var excellentCount = await gpasQuery.CountAsync(tg => tg.GpaValue >= 3.6m);
            var goodCount = await gpasQuery.CountAsync(tg => tg.GpaValue >= 3.2m && tg.GpaValue < 3.6m);
            var averageCount = await gpasQuery.CountAsync(tg => tg.GpaValue >= 2.5m && tg.GpaValue < 3.2m);
            var belowAvgCount = await gpasQuery.CountAsync(tg => tg.GpaValue >= 2.0m && tg.GpaValue < 2.5m);
            var poorCount = await gpasQuery.CountAsync(tg => tg.GpaValue < 2.0m);

            return new AcademicRankingDto
            {
                ExcellentRate = Math.Round((decimal)excellentCount / totalCount * 100, 1),
                GoodRate = Math.Round((decimal)goodCount / totalCount * 100, 1),
                AverageRate = Math.Round((decimal)averageCount / totalCount * 100, 1),
                BelowAverageRate = Math.Round((decimal)belowAvgCount / totalCount * 100, 1),
                PoorRate = Math.Round((decimal)poorCount / totalCount * 100, 1)
            };
        }

        public async Task<KPIScorecardDto> GetKPIScorecard(int? termId = null)
        {
            var totalStudents = await _context.Students
                .Where(s => s.StudentNavigation.Status == UserStatus.Active)
                .CountAsync();

            var totalTeachers = await _context.Teachers
                .Where(t => t.TeacherNavigation.Status == UserStatus.Active)
                .CountAsync();

            var gpasQuery = _context.TermGpas.AsQueryable();
            if (termId.HasValue)
                gpasQuery = gpasQuery.Where(tg => tg.TermId == termId.Value);

            var averageGPA = await gpasQuery.AverageAsync(tg => tg.GpaValue) ?? 0;

            var alertsQuery = _context.Alerts
              .Where(a => a.Status == AlertStatus.New || a.Status == AlertStatus.InProgress);


            // ✅ Chỉ filter khi termId có giá trị
            if (termId.HasValue)
                alertsQuery = alertsQuery.Where(a => a.TermId == termId.Value);

            var totalAlerts = await alertsQuery.CountAsync();

            var alertRate = totalStudents > 0 ? (decimal)totalAlerts / totalStudents * 100 : 0;


            return new KPIScorecardDto
            {
                TotalStudents = totalStudents,
                TotalTeachers = totalTeachers,
                AverageGPA = Math.Round(averageGPA, 2),
                AlertRate = Math.Round(alertRate, 1),
                TotalAlerts = totalAlerts,
                StudentTeacherRatio = totalTeachers > 0 ? Math.Round((decimal)totalStudents / totalTeachers, 1) : 0
            };
        }

        public async Task<(int currentStudents, int previousStudents)> GetStudentComparison(int? currentTermId, int? previousTermId)
        {
            var current = await _context.Students
                .CountAsync(s => s.StudentNavigation.Status == UserStatus.Active);

            var previous = current;

            if (previousTermId.HasValue)
            {
                previous = await _context.SectionStudents
                    .Where(ss => ss.Section.TermId == previousTermId.Value)
                    .Select(ss => ss.StudentId)
                    .Distinct()
                    .CountAsync();
            }

            return (current, previous);
        }
    }
}