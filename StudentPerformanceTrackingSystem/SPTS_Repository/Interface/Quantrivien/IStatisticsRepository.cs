using SPTS_Repository.DTOs.Quantrivien;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPTS_Repository.Interface.Admin
{
    public interface IStatisticsRepository
    {
        Task<List<DepartmentGPADto>> GetDepartmentGPAs(int? termId = null);
        Task<List<DepartmentAlertDto>> GetDepartmentAlerts(int? termId = null);
        Task<int> CountCoursesAsync();
        Task<int> CountTeachingTeachersAsync(int? termId);
        Task<int> CountSectionsAsync(int? termId);
        Task<int> CountUnassignedSectionsAsync(int? termId);
    }
}
