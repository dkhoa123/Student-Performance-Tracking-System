using SPTS_Repository.DTOs.Giangvien;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPTS_Repository.Interface.Giangvien
{
    public interface ISectionTeacherRepository
    {
        Task<List<SectionCardViewModelDto>> GetSectionsByTeacherAsync(int teacherId, int? termId = null);
        Task<ChiTietLopDto> GetSectionDetailAsync(int sectionId);
        Task<int> GetAlertCountBySectionAsync(int sectionId);
    }
}
