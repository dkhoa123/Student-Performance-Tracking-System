using SPTS_Repository.DTOs.Giangvien;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPTS_Repository.Interface.Giangvien
{
    public interface IAlertTeacherRepository
    {
        Task SyncAlertsForGradeAsync(
        int sectionId,
        int studentId,
        decimal? process,
        decimal? final,
        decimal? total);

        Task<int> GetAtRiskStudentsCountAsync(int teacherId);
        Task<List<AlertViewModelDto>> GetRecentAlertsByTeacherAsync(int teacherId, int top = 3);
        Task<int> GetNewAlertsCountAsync(int teacherId);
    }
}
