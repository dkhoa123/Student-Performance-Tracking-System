using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPTS_Service.ViewModel.GiangvienVm
{
    public class ChiTietLopVm
    {
        public int SectionId { get; set; }
        public string CourseCode { get; set; } = "";
        public string CourseName { get; set; } = "";
        public string TermName { get; set; } = "";
        public string Room { get; set; } = "";
        public string ScheduleText { get; set; } = "";

        public string SectionStatus { get; set; } = "";  // OPEN/UPCOMING/FINISHED
        public int StudentCount { get; set; }
        public decimal? AverageScore { get; set; }       // TB TotalScore
        public decimal PassRatePercent { get; set; }     // % qua môn
        public int AlertCount { get; set; }              // số cảnh báo của lớp

        public List<StudentGradeRowVm> Students { get; set; } = new();

        // NEW: pagination + search
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalStudents { get; set; } // tổng sau filter
        public string? Search { get; set; }
    }
}
