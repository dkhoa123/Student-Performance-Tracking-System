using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPTS_Service.ViewModel.SinhvienVm
{
    public class TermGpaTrendVm
    {
        public int TermId { get; set; }
        public string TermName { get; set; } = "";
        public decimal? Gpa { get; set; } // 0..4
    }
}
