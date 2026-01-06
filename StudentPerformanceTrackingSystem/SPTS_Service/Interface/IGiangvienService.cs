using SPTS_Service.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPTS_Service.Interface
{

    public interface IGiangvienService
    {
        Task<Giangvien> GetDashboardAsync(int teacherId);
        Task<ChiTietLopVm> GetSectionDetailAsync(int sectionId);
        Task SaveGradesAsync(int sectionId, List<StudentGradeRowVm> students);
    }
}
