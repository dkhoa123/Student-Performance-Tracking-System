using SPTS_Repository.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPTS_Repository.DTOs.Giangvien
{
    public record ChiTietLopDto(
    int SectionId,
    string CourseCode,
    string CourseName,
    string TermName,
    string Room,
    string ScheduleText,
    string SectionStatus,
    List<StudentGradeRowDto> Students
);
}
