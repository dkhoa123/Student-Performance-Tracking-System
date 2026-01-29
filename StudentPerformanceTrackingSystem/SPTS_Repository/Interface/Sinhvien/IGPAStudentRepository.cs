using SPTS_Repository.DTOs.Sinhvien;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPTS_Repository.Interface.Sinhvien
{
    public interface IGPAStudentRepository
    {
        Task<TermGpaDto?> GetTermGpaAsync(int studentId, int termId);
        Task<CumulativeGpaDto?> GetCumulativeGpaAsync(int studentId);
        Task<int> GetCreditsEarnedCumulativeAsync(int studentId);
        Task<List<TermGpaTrendRowDto>> GetTermGpaTrendAsync(int studentId, int take = 5);

    }
}
