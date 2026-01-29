using SPTS_Repository.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPTS_Repository.Interface.Admin
{
    public interface ISectionManagementRepository
    {
        Task<(List<Section> Sections, int TotalCount)> GetSectionsForAdminAsync(int? termId, int page, int pageSize);
        Task<Section?> GetSectionByIdAsync(int sectionId);

    }
}
