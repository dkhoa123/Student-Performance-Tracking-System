using SPTS_Service.ViewModel.QuantrivienVm;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPTS_Service.Interface.Admin
{
    public interface IDashboardService
    {
        Task<AdminVM> GetSystemStatistics(int? termId = null);
        Task<List<TermOptionVM>> GetTermsForDropdownAsync();
    }
}
