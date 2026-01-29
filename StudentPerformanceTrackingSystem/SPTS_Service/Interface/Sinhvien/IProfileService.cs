
using SPTS_Service.ViewModel.SinhvienVm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPTS_Service.Interface.Student
{
    public interface IProfileService
    {
        Task CapNhatThongTinSinhVien(SinhVien model);

    }
}
