using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPTS_Service.ViewModel.SinhvienVm
{
    public class CourseProgressVm
    {
        public string CourseCode { get; set; } = "";
        public string CourseName { get; set; } = "";
        public string TeacherName { get; set; } = "";
        public int Credit { get; set; }              // Courses.credit
        public decimal? ProcessScore { get; set; }   // Grades.process_score

        public decimal? FinalScore { get; set; }     // Grades.final_score
        public decimal? TotalScore { get; set; }   // Grades.total_score
        public decimal? GpaPoint { get; set; }     // Grades.gpa_point
        public string? Letter { get; set; }        // từ GpaScales (optional)
    }
}
