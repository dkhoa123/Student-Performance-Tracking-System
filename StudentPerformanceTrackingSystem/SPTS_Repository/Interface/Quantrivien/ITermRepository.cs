using SPTS_Repository.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPTS_Repository.Interface.Admin
{
    public interface ITermRepository
    {
        Task<List<Term>> GetTermsAsync();            // NEW
        Task<Term?> GetTermByIdAsync(int termId);    // NEW
    }
}
