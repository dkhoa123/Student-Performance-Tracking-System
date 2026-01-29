using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPTS_Shared.Domain
{
    public class GpaCalculationResult
    {
        public decimal? GpaValue { get; set; }
        public int CreditsAttempted { get; set; }
        public int CreditsEarned { get; set; }

        public GpaCalculationResult(decimal? gpaValue, int creditsAttempted, int creditsEarned)
        {
            GpaValue = gpaValue;
            CreditsAttempted = creditsAttempted;
            CreditsEarned = creditsEarned;
        }
    }

    public class GradeData
    {
        public decimal GpaPoint { get; set; }
        public int Credits { get; set; }
        public decimal TotalScore { get; set; }
    }
}
