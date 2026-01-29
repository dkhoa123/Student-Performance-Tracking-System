using SPTS_Service.ViewModel;
using SPTS_Service.ViewModel.GiangvienVm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPTS_Service.Interface.Giangvien
{
    public interface IDashboardTeacherService
    {
        Task<GiangVien> GetDashboardAsync(int teacherId, int? termId = null);
    }
}
