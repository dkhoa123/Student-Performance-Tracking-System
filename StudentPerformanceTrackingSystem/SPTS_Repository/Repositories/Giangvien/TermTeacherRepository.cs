using Microsoft.EntityFrameworkCore;
using SPTS_Repository.Entities;
using SPTS_Repository.Interface.Admin;
using SPTS_Repository.Interface.Giangvien;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPTS_Repository.Repositories.Giangvien
{
    public class TermTeacherRepository : ITermTeacherRepository
    {
        private readonly SptsContext _context;
        public TermTeacherRepository(SptsContext context)
        {
            _context = context;
        }
        public async Task<int> GetTermIdBySectionAsync(int sectionId)
        {
            var termId = await _context.Sections
                .Where(s => s.SectionId == sectionId)
                .Select(s => s.TermId)
                .FirstOrDefaultAsync();

            if (termId == 0)
                throw new Exception("Không tìm thấy TermId của lớp.");

            return termId;
        }

        public async Task<List<(int TermId, string TermName)>> GetTermsByTeacherAsync(int teacherId)
        {
            return await _context.Sections
                .Where(s => s.TeacherId == teacherId)
                .Select(s => new { s.TermId, s.Term.TermName })
                .Distinct()
                .OrderByDescending(t => t.TermId)
                .Select(t => ValueTuple.Create(t.TermId, t.TermName))
                .ToListAsync();
        }
    }
}
