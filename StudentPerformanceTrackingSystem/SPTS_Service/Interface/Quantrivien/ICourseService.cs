using SPTS_Service.ViewModel.QuantrivienVm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPTS_Service.Interface.Admin
{
    public interface ICourseService
    {
        Task<AdminCourseTeacherVM> GetCourseTeacherPageAsync(
        int? termId,
        int page,
        int pageSize);

        Task<SectionDetailVM?> GetSectionDetailAsync(int sectionId);

    }
}
