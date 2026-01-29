using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPTS_Repository.DTOs.Quantrivien
{
    public class AcademicRankingDto
    {
        public decimal ExcellentRate { get; set; }
        public decimal GoodRate { get; set; }
        public decimal AverageRate { get; set; }
        public decimal BelowAverageRate { get; set; }
        public decimal PoorRate { get; set; }
    }

}
