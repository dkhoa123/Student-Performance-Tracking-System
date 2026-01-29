using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPTS_Service.ViewModel.QuantrivienVm
{
    public class DepartmentAlert
    {
        public string DepartmentName { get; set; }
        public string DepartmentCode { get; set; }
        public int TotalStudents { get; set; }
        public int AlertCount { get; set; }
        public decimal AlertRate { get; set; }
    }
}
