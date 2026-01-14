using SPTS_Repository.Entities;
using SPTS_Service.ViewModel;


namespace SPTS_Service.Interface
{
    public interface ISinhVienService
    {
        Task CapNhatThongTinSinhVien(SinhVien model);
        Task<SinhVien> GetDashboardAsync(int studentId, int? termId = null);

        Task<ThongBaoSinhVienVm> GetNotificationsPageAsync(int studentId, string filter = "all", int page = 1, int pageSize = 10);
        Task MarkAsReadAsync(int notificationId, int studentId);
        Task MarkAllAsReadAsync(int studentId);
    }

   
}
