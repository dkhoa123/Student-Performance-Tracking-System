using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPTS_Repository.DTOs.Sinhvien
{
    public record StudentIdentityDto(
        int? StudentId, 
        int? UserId, 
        string StudentCode, 
        string FullName, 
        string Email, 
        string Major, 
        DateOnly? DateOfBirth, 
        string? Gender, 
        string? Phone, 
        string? Address, 
        string status);
}
