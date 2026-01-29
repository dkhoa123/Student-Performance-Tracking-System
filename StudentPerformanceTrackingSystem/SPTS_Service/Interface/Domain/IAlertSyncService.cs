using System.Threading.Tasks;

namespace SPTS_Service.Interface.Domain
{
    public interface IAlertSyncService
    {
        Task SyncAlertsForGradeAsync(
            int sectionId,
            int studentId,
            decimal? processScore,
            decimal? finalScore,
            decimal? totalScore);
    }
}