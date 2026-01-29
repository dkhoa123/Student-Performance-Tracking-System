using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPTS_Repository.DTOs.Giangvien
{
    public record GiangVienProfileDto(
        int TeacherId,
        string TeacherName,
        string Email,
        string Status,
        string Rolelabel,
        string TeacherCode,
        string Department,
        string Degree,
        string Phone,
        DateTime Birthday
    );
}
