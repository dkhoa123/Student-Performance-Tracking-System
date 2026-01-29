using Microsoft.EntityFrameworkCore;
using SPTS_Repository.Entities;
using SPTS_Repository.Interface.Admin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPTS_Repository.Repositories.Admin
{
    public class SectionManagementRepository : ISectionManagementRepository
    {
        private readonly SptsContext _context;

        public SectionManagementRepository(SptsContext context)
        {
            _context = context;
        }
        public async Task<Section?> GetSectionByIdAsync(int sectionId)
        {
            return await _context.Sections
                .AsNoTracking()
                .Include(s => s.Course)
                .Include(s => s.Teacher)
                    .ThenInclude(t => t.TeacherNavigation)
                .Include(s => s.Term)
                .FirstOrDefaultAsync(s => s.SectionId == sectionId);
        }

        public async Task<(List<Section> Sections, int TotalCount)> GetSectionsForAdminAsync(int? termId, int page, int pageSize)
        {
            if (page <= 0) page = 1;
            if (pageSize <= 0) pageSize = 10;

            var query = _context.Sections
                .AsNoTracking()
                .Include(s => s.Course)
                .Include(s => s.Teacher)
                    .ThenInclude(t => t.TeacherNavigation) // Users
                .AsQueryable();

            if (termId.HasValue)
                query = query.Where(s => s.TermId == termId.Value);

            var total = await query.CountAsync();

            var sections = await query
                .OrderByDescending(s => s.SectionId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (sections, total);
        }
    }
}
