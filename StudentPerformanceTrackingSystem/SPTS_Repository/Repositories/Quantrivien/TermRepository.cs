using Microsoft.EntityFrameworkCore;
using SPTS_Repository.Entities;
using SPTS_Repository.Interface.Admin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPTS_Repository.Repositories.Quantrivien
{
    public class TermRepository : ITermRepository
    {
        private readonly SptsContext _context;

        public TermRepository(SptsContext context)
        {
            _context = context;
        }
        public Task<Term?> GetTermByIdAsync(int termId)
        {
            return _context.Terms.AsNoTracking().FirstOrDefaultAsync(t => t.TermId == termId);
        }

        public async Task<List<Term>> GetTermsAsync()
        {
            return await _context.Terms
                .AsNoTracking()
                .OrderByDescending(t => t.StartDate)
                .ToListAsync();
        }
    }
}
