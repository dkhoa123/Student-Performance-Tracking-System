
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPTS_Service.ViewModel.QuantrivienVm
{
    public class AdminVM
    {
        public KPIScorecard KPI { get; set; }
        public List<DepartmentGPA> DepartmentGPAs { get; set; }
        public AcademicRanking AcademicRanking { get; set; }
        public List<DepartmentAlert> DepartmentAlerts { get; set; }
    }
}
