using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPTS_Service.ViewModel.GiangvienVm
{
    public class SectionOptionVm
    {
        public int SectionId { get; set; }
        public string CourseCode { get; set; } = "";
        public string CourseName { get; set; } = "";
        public int StudentCount { get; set; }
        public bool IsSelected { get; set; }
    }
}
