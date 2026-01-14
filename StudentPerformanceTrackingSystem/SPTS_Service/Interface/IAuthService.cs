using SPTS_Repository.Entities;
using SPTS_Service.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPTS_Service.Interface
{
    public interface IAuthService
    {
        Task DangKysv(DangKySinhVien model);
        Task<string?> CreateStuCode(int? CohortYear);
        Task<User> DangNhap(string emailSv, string matKhau);
    }
}
