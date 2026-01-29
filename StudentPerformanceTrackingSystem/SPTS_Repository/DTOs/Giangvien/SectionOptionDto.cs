using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPTS_Repository.DTOs.Giangvien
{
    public record SectionOptionDto(
        int SectionId, 
        string CourseCode,
        string CourseName, 
        int StudentCount);
}
