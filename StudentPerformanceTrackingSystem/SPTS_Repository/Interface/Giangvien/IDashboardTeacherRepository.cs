using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPTS_Repository.Interface.Giangvien
{
    public interface IDashboardTeacherRepository
    {
        Task<int> GetTotalStudentsByTeacherAsync(int teacherId);
        Task<int> GetNewStudentsThisMonthAsync(int teacherId);
        Task<decimal> GetAverageScoreByTeacherAsync(int teacherId, int termId);
        Task<int> GetActiveSectionsCountAsync(int teacherId);
    }
}
