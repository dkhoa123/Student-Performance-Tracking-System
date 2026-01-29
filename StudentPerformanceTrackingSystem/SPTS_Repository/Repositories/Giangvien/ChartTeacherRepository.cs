using Microsoft.EntityFrameworkCore;
using SPTS_Repository.DTOs.Giangvien;
using SPTS_Repository.Entities;
using SPTS_Repository.Interface;
using SPTS_Repository.Interface.Giangvien;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPTS_Repository.Repositories.Giangvien
{
    public class ChartTeacherRepository : IChartTeacherRepository
    {
        private readonly SptsContext _context;
        public ChartTeacherRepository(SptsContext context)
        {
            _context = context;
        }
        public async Task<List<ChartDataViewModelDto>> GetGpaChartDataByTeacherAsync(int teacherId, int? TermId = null)
        {
            return await _context.TermGpas
                .Where(tg => _context.SectionStudents
                    .Any(ss => ss.StudentId == tg.StudentId &&
                               ss.Section.TeacherId == teacherId))
                .GroupBy(tg => new { tg.TermId, tg.Term.TermName, tg.Term.StartDate })
                .OrderBy(g => g.Key.StartDate)
                .Select(g => new ChartDataViewModelDto(
                            g.Key.TermId,
                            g.Key.TermName,
                            g.Average(tg => tg.GpaValue) ?? 0
                    )).ToListAsync();
        }
    }
}
