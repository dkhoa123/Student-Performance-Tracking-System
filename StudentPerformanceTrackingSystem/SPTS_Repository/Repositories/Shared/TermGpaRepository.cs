using Microsoft.EntityFrameworkCore;
using SPTS_Repository.Entities;
using SPTS_Repository.Interface.Shared;
using SPTS_Shared.Domain;
using System.Threading.Tasks;

namespace SPTS_Repository.Repositories.Shared
{
    public class TermGpaRepository : ITermGpaRepository
    {
        private readonly SptsContext _context;

        public TermGpaRepository(SptsContext context)
        {
            _context = context;
        }

        public async Task UpsertAsync(int studentId, int termId, GpaCalculationResult result)
        {
            var existing = await _context.TermGpas
                .SingleOrDefaultAsync(x => x.StudentId == studentId && x.TermId == termId);

            if (existing == null)
            {
                _context.TermGpas.Add(new TermGpa
                {
                    StudentId = studentId,
                    TermId = termId,
                    GpaValue = result.GpaValue,
                    CreditsAttempted = result.CreditsAttempted,
                    CreditsEarned = result.CreditsEarned
                });
            }
            else
            {
                existing.GpaValue = result.GpaValue;
                existing.CreditsAttempted = result.CreditsAttempted;
                existing.CreditsEarned = result.CreditsEarned;
            }

            await _context.SaveChangesAsync();
        }
    }
}