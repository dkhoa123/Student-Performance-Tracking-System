using SPTS_Repository.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPTS_Repository.Interface.Giangvien
{
    public interface IGradeTeacherRepository
    {
        Task<GradeRule?> GetActiveGradeRuleBySectionAsync(int sectionId);
        Task UpsertGradeAsync(
            int sectionId,
            int studentId,
            decimal? process,
            decimal? final,
            decimal? total,
            decimal? gpaPoint);

        Task<decimal?> GetGpaPointByTotalAsync(decimal totalScore);
    }
}
