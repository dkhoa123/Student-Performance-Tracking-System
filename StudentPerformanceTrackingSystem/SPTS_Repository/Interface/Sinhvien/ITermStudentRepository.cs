using SPTS_Repository.DTOs.Sinhvien;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPTS_Repository.Interface.Sinhvien
{
    public interface ITermStudentRepository
    {
        Task<int> GetCurrentTermIdAsync();
        Task<CurrentTermDto?> GetCurrentTermAsync();
        Task<List<TermOptionDto>> GetTermsAsync();
    }
}
