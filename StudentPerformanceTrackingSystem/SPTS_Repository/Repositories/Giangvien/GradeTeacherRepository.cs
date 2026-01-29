using Microsoft.EntityFrameworkCore;
using SPTS_Repository.Entities;
using SPTS_Repository.Interface.Giangvien;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPTS_Repository.Repositories.Giangvien
{
    public class GradeTeacherRepository : IGradeTeacherRepository
    {
        private readonly SptsContext _context;
        public GradeTeacherRepository(SptsContext context)
        {
            _context = context;
        }
        public async Task<GradeRule?> GetActiveGradeRuleBySectionAsync(int sectionId)
        {
            // Lấy rule theo CourseId của Section
            return await _context.GradeRules
                .Where(r => r.Active
                    && _context.Sections.Any(s => s.SectionId == sectionId && s.CourseId == r.CourseId))
                .OrderByDescending(r => r.RuleId) // nếu có nhiều rule active, lấy rule mới nhất
                .FirstOrDefaultAsync();
        }

        public async Task<decimal?> GetGpaPointByTotalAsync(decimal totalScore)
        {
            var scale = await _context.GpaScales
                .Where(sc => totalScore >= sc.MinScore && totalScore <= sc.MaxScore)
                .Select(sc => (decimal?)sc.GpaPoint) // nếu cột tên khác thì đổi
                .FirstOrDefaultAsync();

            return scale;
        }

        public async Task RecalculateAndUpsertTermGpaAsync(int studentId, int termId)
        {
            // A) Có TotalScore là tính
            var rows = await (
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
                select new
                {
                    c.Credits,
                    TotalScore = g.TotalScore!.Value,
                    scale!.GpaPoint
                }
            ).ToListAsync();

            var creditsAttempted = rows.Sum(x => x.Credits);
            var creditsEarned = rows.Where(x => x.TotalScore >= 5m).Sum(x => x.Credits);

            decimal? gpaValue = null;
            if (creditsAttempted > 0)
            {
                var numerator = rows.Sum(x => x.GpaPoint * x.Credits);
                gpaValue = Math.Round(numerator / creditsAttempted, 2);
            }

            var existing = await _context.TermGpas
                .SingleOrDefaultAsync(x => x.StudentId == studentId && x.TermId == termId);

            if (existing == null)
            {
                _context.TermGpas.Add(new TermGpa
                {
                    StudentId = studentId,
                    TermId = termId,
                    GpaValue = gpaValue,
                    CreditsAttempted = creditsAttempted,
                    CreditsEarned = creditsEarned
                });
            }
            else
            {
                existing.GpaValue = gpaValue;
                existing.CreditsAttempted = creditsAttempted;
                existing.CreditsEarned = creditsEarned;
            }

            await _context.SaveChangesAsync();
        }

        public async Task UpsertGradeAsync(int sectionId, int studentId, decimal? process, decimal? final, decimal? total, decimal? gpaPoint)
        {
            var grade = await _context.Grades
                .SingleOrDefaultAsync(g => g.SectionId == sectionId && g.StudentId == studentId);

            if (grade == null)
            {
                grade = new Grade
                {
                    SectionId = sectionId,
                    StudentId = studentId,
                    ProcessScore = process,
                    FinalScore = final,
                    TotalScore = total,
                    GpaPoint = gpaPoint
                };
                _context.Grades.Add(grade);
            }
            else
            {
                grade.ProcessScore = process;
                grade.FinalScore = final;
                grade.TotalScore = total;
                grade.GpaPoint = gpaPoint;
            }

            await _context.SaveChangesAsync();
        }
    }
}
