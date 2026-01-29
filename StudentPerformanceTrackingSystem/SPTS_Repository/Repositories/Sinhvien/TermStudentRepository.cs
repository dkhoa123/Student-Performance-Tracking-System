using Microsoft.EntityFrameworkCore;
using SPTS_Repository.DTOs.Sinhvien;
using SPTS_Repository.Entities;
using SPTS_Repository.Interface.Sinhvien;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPTS_Repository.Repositories.Sinhvien
{
    public class TermStudentRepository : ITermStudentRepository
    {
        private readonly SptsContext _db;
        public TermStudentRepository(SptsContext db)
        {
            _db = db;
        }
        public async Task<CurrentTermDto?> GetCurrentTermAsync()
        {
            return await _db.Terms
                .OrderByDescending(t => t.StartDate)
                .Select(t => new CurrentTermDto(t.TermId, t.TermName))
                .FirstOrDefaultAsync();
        }

        public Task<int> GetCurrentTermIdAsync()
        {
            return _db.Terms
                    .OrderByDescending(t => t.StartDate)
                    .Select(t => t.TermId)
                    .FirstOrDefaultAsync();
        }

        public Task<List<TermOptionDto>> GetTermsAsync()
        {
            return _db.Terms
            .OrderByDescending(t => t.StartDate)
            .Select(t => new TermOptionDto(t.TermId, t.TermName))
            .ToListAsync();
        }
    }
}
