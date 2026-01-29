using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPTS_Service.ViewModel.QuantrivienVm
{
    public class TeacherOptionVM
    {
        public int TeacherId { get; set; }
        public string TeacherCode { get; set; } = "";
        public string FullName { get; set; } = "";
        public string Email { get; set; } = "";
        public string? Degree { get; set; }
        public string? DepartmentName { get; set; }
        public int SectionCount { get; set; } // Số lớp đang dạy
    }
}
