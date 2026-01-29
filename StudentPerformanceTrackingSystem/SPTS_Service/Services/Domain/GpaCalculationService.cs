using Microsoft.EntityFrameworkCore;
using SPTS_Repository.Entities;
using SPTS_Shared.Constants;
using SPTS_Service.Interface.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SPTS_Shared.Domain;

namespace SPTS_Service.Services.Domain
{
    public class GpaCalculationService : IGpaCalculationService
    {
        private readonly SptsContext _context;

        public GpaCalculationService(SptsContext context)
        {
            _context = context;
        }

        public GpaCalculationResult Calculate(List<GradeData> grades)
        {
            if (grades == null || !grades.Any())
                return new GpaCalculationResult(null, 0, 0);

            var creditsAttempted = grades.Sum(x => x.Credits);
            var creditsEarned = grades
                .Where(x => x.TotalScore >= GradeThresholds.PassingScore)
                .Sum(x => x.Credits);

            decimal? gpaValue = null;
            if (creditsAttempted > 0)
            {
                var numerator = grades.Sum(x => x.GpaPoint * x.Credits);
                gpaValue = Math.Round(numerator / creditsAttempted, GradeThresholds.GpaRoundingScale);
            }

            return new GpaCalculationResult(gpaValue, creditsAttempted, creditsEarned);
        }

        public async Task<GpaCalculationResult> CalculateForTermAsync(int studentId, int termId)
        {
            var grades = await (
                from g in _context.Grades
                join s in _context.Sections on g.SectionId equals s.SectionId
                join c in _context.Courses on s.CourseId equals c.CourseId
                from scale in _context.GpaScales
                    .Where(sc => g.TotalScore != null
                              && g.TotalScore >= sc.MinScore
                              && g.TotalScore <= sc.MaxScore)
                    .DefaultIfEmpty()
                where g.StudentId == studentId
                   && s.TermId == termId
                   && g.TotalScore != null
                   && scale != null
                select new GradeData
                {
                    Credits = c.Credits,
                    TotalScore = g.TotalScore!.Value,
                    GpaPoint = scale!.GpaPoint
                }
            ).ToListAsync();

            return Calculate(grades);
        }
    }
}