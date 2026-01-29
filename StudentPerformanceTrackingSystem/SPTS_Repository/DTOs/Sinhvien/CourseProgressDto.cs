using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPTS_Repository.DTOs.Sinhvien
{
    public record CourseProgressDto(
        string CourseCode, 
        string CourseName, 
        string TeacherName, 
        int Credit, 
        decimal? ProcessScore, 
        decimal? FinalScore, 
        decimal? TotalScore, 
        decimal? GpaPoint, 
        string? Letter);
}
