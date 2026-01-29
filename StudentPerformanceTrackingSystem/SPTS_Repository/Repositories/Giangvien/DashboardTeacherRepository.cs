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
    public class DashboardTeacherRepository : IDashboardTeacherRepository
    {
        private readonly SptsContext _context;
        public DashboardTeacherRepository(SptsContext context)
        {
            _context = context;
        }
        public async Task<int> GetActiveSectionsCountAsync(int teacherId)
        {
            return await _context.Sections
                .Where(sec => sec.TeacherId == teacherId && sec.Status == "OPEN")
                .CountAsync();
        }

        public async Task<decimal> GetAverageScoreByTeacherAsync(int teacherId, int termId)
        {
            var avgScore = await _context.Grades
                .Where(g => g.Section.TeacherId == teacherId
                         && g.Section.TermId == termId
                         && g.TotalScore.HasValue)
                .AverageAsync(g => g.TotalScore);

            return avgScore ?? 0m;
        }

        public async Task<int> GetNewStudentsThisMonthAsync(int teacherId)
        {
            var oneMonthAgo = DateTime.UtcNow.AddMonths(-1);

            return await _context.SectionStudents
                .Where(ss => ss.Section.TeacherId == teacherId &&
                             ss.AddedAt >= oneMonthAgo)
                .CountAsync();
        }


        public async Task<int> GetTotalStudentsByTeacherAsync(int teacherId)
        {
            return await _context.SectionStudents
                .Where(ss => ss.Section.TeacherId == teacherId && ss.Status == "ACTIVE")
                .Select(ss => ss.StudentId)
                .Distinct()
                .CountAsync();
        }
    }
}
