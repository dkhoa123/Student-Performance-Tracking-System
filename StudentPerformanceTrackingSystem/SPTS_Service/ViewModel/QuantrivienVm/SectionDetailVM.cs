using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPTS_Service.ViewModel.QuantrivienVm
{
    public class SectionDetailVM
    {
        public int SectionId { get; set; }
        public string SectionCode { get; set; } = "";
        public string CourseCode { get; set; } = "";
        public string CourseName { get; set; } = "";
        public int Credits { get; set; }
        public string TermName { get; set; } = "";

        public int? TeacherId { get; set; }
        public string? TeacherName { get; set; }
        public string? TeacherCode { get; set; }
    }
}
