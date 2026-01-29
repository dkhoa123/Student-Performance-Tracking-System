using SPTS_Service.ViewModel;
using SPTS_Service.ViewModel.SinhvienVm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPTS_Service.Interface.Student
{
    public interface INotificationService
    {
        Task<ThongBaoSinhVienVm> GetNotificationsPageAsync(
        int studentId,
        string filter = "all",
        int page = 1,
        int pageSize = 10);

        Task MarkAsReadAsync(int notificationId, int studentId);
        Task MarkAllAsReadAsync(int studentId);
    }
}
