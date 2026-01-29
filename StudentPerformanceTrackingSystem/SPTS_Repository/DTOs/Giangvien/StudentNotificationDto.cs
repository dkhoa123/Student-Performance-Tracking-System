using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPTS_Repository.DTOs.Giangvien
{
    public record StudentNotificationDto(
       int StudentId,
       string StudentCode,
       string FullName,
       string? AlertType,
       string? Severity,
       decimal? ActualValue
   );
}
