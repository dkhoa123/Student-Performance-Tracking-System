using SPTS_Repository.Interface;
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

        Task<ThongBaoPageVm> GetThongBaoPageAsync(int teacherId, int sectionId, int page = 1, int pageSize = 10);

        Task<List<SectionOptionDto>> GetSectionsForNotificationAsync(int teacherId);
        Task<List<StudentNotificationDto>> GetStudentsWithAlertStatusAsync(int sectionId);

        Task<int> SendToSectionAsync(int sectionId, string title, string content);
        Task SendToStudentAsync(int sectionId, int studentId, string title, string content);



    }
}
