using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPTS_Repository.DTOs.Quantrivien
{
    public class UserDetailDto
    {
        public int UserId { get; set; }
        public string FullName { get; set; } = "";
        public string Email { get; set; } = "";
        public string Role { get; set; } = "";
        public string Status { get; set; } = "";

        public string? StudentCode { get; set; }
        public string? Major { get; set; }
        public int? CohortYear { get; set; }
        public int? DepartmentId { get; set; }

        public string? TeacherCode { get; set; }
        public string? Degree { get; set; }
        public string? DepartmentName { get; set; }
    }
}
