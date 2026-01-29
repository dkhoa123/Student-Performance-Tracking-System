using SPTS_Shared.Domain;

namespace SPTS_Repository.Interface.Shared
{
    public interface ITermGpaRepository
    {
        Task UpsertAsync(int studentId, int termId, GpaCalculationResult result);
    }
}