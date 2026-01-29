using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPTS_Repository.DTOs.Quantrivien
{
    public class UserUpdateDto
    {
        public int UserId { get; set; }
        public string FullName { get; set; } = "";
        public string Email { get; set; } = "";
        public string Role { get; set; } = "";
        public string Status { get; set; } = "";

        public string? StudentCode { get; set; }
        public string? Major { get; set; }           // ✅ THÊM
        public int? CohortYear { get; set; }         // ✅ THÊM
        public int? DepartmentId { get; set; }       // ✅ THÊM

        public string? TeacherCode { get; set; }
        public string? Degree { get; set; }          // ✅ THÊM
        public string? DepartmentName { get; set; }  // ✅ THÊM
    }
}
