using SPTS_Repository.DTOs.Giangvien;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPTS_Repository.Interface.Giangvien
{
    public interface INotificationTeacherRepository
    {
        Task<List<SectionOptionDto>> GetSectionsForNotificationAsync(int teacherId);
        Task<List<StudentNotificationDto>> GetStudentsWithAlertStatusAsync(int sectionId);

        Task<int> SendToSectionAsync(int sectionId, string title, string content);
        Task SendToStudentAsync(int sectionId, int studentId, string title, string content);
    }
}
