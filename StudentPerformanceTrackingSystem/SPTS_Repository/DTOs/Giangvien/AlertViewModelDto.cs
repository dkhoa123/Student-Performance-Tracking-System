using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPTS_Repository.DTOs.Giangvien
{
    public record AlertViewModelDto(
        string StudentName, 
        string AlertType, 
        string Message, 
        string Severity, 
        string IconName, 
        string IconColor, 
        DateTime CreatedAt);
}

