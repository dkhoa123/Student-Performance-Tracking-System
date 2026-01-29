using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPTS_Repository.DTOs.Quantrivien
{
    public class DepartmentGPADto
    {
        public string DepartmentName { get; set; } = string.Empty;
        public decimal AverageGPA { get; set; }
        public int StudentCount { get; set; }
    }
}
