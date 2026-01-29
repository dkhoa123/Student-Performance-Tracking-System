using SPTS_Repository.DTOs.Quantrivien;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPTS_Repository.Interface.Admin
{
    public interface IKPIRepository
    {
        Task<KPIScorecardDto> GetKPIScorecard(int? termId = null);
        Task<AcademicRankingDto> GetAcademicRanking(int? termId = null);
        Task<(int currentStudents, int previousStudents)> GetStudentComparison(int? currentTermId, int? previousTermId);

    }
}
