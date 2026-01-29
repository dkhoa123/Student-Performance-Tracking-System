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
