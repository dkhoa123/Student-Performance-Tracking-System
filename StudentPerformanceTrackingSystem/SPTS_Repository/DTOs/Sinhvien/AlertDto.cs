using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPTS_Repository.DTOs.Sinhvien
{
    public record AlertDto(int AlertId, string AlertType, string Severity, string? CourseCode, string? Reason, DateTime CreatedAt);
}
