using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPTS_Service.ViewModel.GiangvienVm
{
    public class ChartDataVm
    {
        public int TermId { get; set; }
        public string TermName { get; set; } = string.Empty;
        public decimal AverageGpa { get; set; }
    }
}
