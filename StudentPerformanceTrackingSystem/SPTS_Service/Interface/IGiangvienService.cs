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
    }
}
