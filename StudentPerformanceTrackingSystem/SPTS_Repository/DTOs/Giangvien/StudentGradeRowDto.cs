using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPTS_Repository.DTOs.Giangvien
{
    public record StudentGradeRowDto(
        int StudentId,
        string StudentCode,
        string FullName,
        DateOnly? DateOfBirth,
        decimal? ProcessScore,
        decimal? FinalScore,
        decimal? TotalScore
    );
}
