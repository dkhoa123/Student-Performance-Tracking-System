using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPTS_Service.ViewModel.QuantrivienVm
{
    public class AcademicRanking
    {
        public decimal ExcellentRate { get; set; }  // GPA >= 3.6
        public decimal GoodRate { get; set; }        // 3.2 - 3.59
        public decimal AverageRate { get; set; }     // 2.5 - 3.19
        public decimal BelowAverageRate { get; set; }// 2.0 - 2.49
        public decimal PoorRate { get; set; }        // < 2.0
    }
}
