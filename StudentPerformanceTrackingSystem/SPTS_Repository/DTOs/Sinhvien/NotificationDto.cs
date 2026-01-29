using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPTS_Repository.DTOs.Sinhvien
{
    public record NotificationDto(
   int NotificationId,
   string Title,
   string Content,
   bool IsRead,
   DateTime CreatedAt,
   string? AlertType,
   string? Severity
);
}
