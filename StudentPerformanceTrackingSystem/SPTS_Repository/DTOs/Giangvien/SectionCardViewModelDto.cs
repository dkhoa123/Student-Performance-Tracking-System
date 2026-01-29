using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPTS_Repository.DTOs.Giangvien
{
    public record SectionCardViewModelDto(
     int SectionId,
     string CourseCode,
     string CourseName,
     string Room,
     int StudentCount,
     decimal CompletionRate,
     string ColorClass,

     int Credits,              // Số tín chỉ
     string Status,            // OPEN, UPCOMING, FINISHED
     string? Schedule,         // "Thứ 2, Tiết 1-3"
     string? TimeSlot,         // "7:00 - 9:30"
     decimal? AverageScore,    // Điểm TB của lớp
     int TermId,              // ID học kỳ
     string TermName,         // Tên học kỳ
     DateTime? StartDate      // Ngày bắt đầu
 );
}
