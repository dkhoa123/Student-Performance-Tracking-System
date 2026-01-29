using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPTS_Service.ViewModel.GiangvienVm
{
    public class TermSectionsVm
    {
        public int TermId { get; set; }
        public string TermName { get; set; }
        public DateTime? TermStartDate { get; set; }
        public List<SectionCardViewModel> Sections { get; set; }
    }
}
