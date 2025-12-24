using SPTS_Repository.Entities;
using SPTS_Service.ViewModel;


namespace SPTS_Service.Interface
{
    public interface ISinhVienService
    {
        Task DangKysv(DangKySinhVien model);
        Task<string?> CreateStuCode(int? CohortYear);
        Task<User> DangNhap(string emailSv, string matKhau);
        
    }
}
