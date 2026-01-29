using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPTS_Service.ViewModel.QuantrivienVm
{
    public class KPIScorecard
    {
        public int TotalStudents { get; set; }
        public int TotalTeachers { get; set; }
        public decimal AverageGPA { get; set; }
        public decimal AlertRate { get; set; }
        public int TotalAlerts { get; set; }
        public decimal StudentTeacherRatio { get; set; }

        // So sánh với kỳ trước
        public decimal StudentGrowthRate { get; set; }
        public decimal GPAChange { get; set; }
    }
}
