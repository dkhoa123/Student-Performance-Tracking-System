using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPTS_Repository.DTOs.Quantrivien
{
    public class KPIScorecardDto
    {
        public int TotalStudents { get; set; }
        public int TotalTeachers { get; set; }
        public decimal AverageGPA { get; set; }
        public decimal AlertRate { get; set; }
        public int TotalAlerts { get; set; }
        public decimal StudentTeacherRatio { get; set; }
    }
}
