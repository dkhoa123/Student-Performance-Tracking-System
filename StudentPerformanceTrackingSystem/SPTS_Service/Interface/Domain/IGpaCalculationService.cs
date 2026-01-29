using SPTS_Shared.Domain;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SPTS_Service.Interface.Domain
{
    public interface IGpaCalculationService
    {
        GpaCalculationResult Calculate(List<GradeData> grades);
        Task<GpaCalculationResult> CalculateForTermAsync(int studentId, int termId);
    }

    
}