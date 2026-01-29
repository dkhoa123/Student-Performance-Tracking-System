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
    }
}
