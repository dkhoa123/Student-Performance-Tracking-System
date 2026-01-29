using SPTS_Service.ViewModel.QuantrivienVm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPTS_Service.Interface.Admin
{
    public interface ISectionService
    {
        Task<List<TeacherOptionVM>> GetAvailableTeachersAsync(int? termId = null);
        Task<bool> AssignTeacherToSectionAsync(int sectionId, int teacherId);
        Task<bool> UnassignTeacherFromSectionAsync(int sectionId);
    }
}
