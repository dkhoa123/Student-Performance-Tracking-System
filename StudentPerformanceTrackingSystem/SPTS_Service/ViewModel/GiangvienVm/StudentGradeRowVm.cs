using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPTS_Service.ViewModel.GiangvienVm
{
    public class StudentGradeRowVm
    {
        public int StudentId { get; set; }
        public string StudentCode { get; set; } = "";
        public string FullName { get; set; } = "";
        public DateOnly? DateOfBirth { get; set; }

        public decimal? ProcessScore { get; set; }
        public decimal? FinalScore { get; set; }
        public decimal? TotalScore { get; set; }
    }
}
