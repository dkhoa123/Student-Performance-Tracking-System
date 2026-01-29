using SPTS_Service.ViewModel;
using SPTS_Service.ViewModel.GiangvienVm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPTS_Service.Interface.Giangvien
{
    public interface ISectionTeacherService
    {
        Task<ChiTietLopVm> GetSectionDetailAsync(
        int sectionId,
        int page = 1,
        int pageSize = 10,
        string? search = null);
    }
}
