using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPTS_Service.ViewModel.GiangvienVm
{
    public class GiangVienProfileVm
    {
        public int TeacherId { get; set; }
        public string FullName { get; set; } = "";
        public string RoleLabel { get; set; } = "Giảng viên";
        public string Email { get; set; } = "";
        public string Status { get; set; } = "";          // Active/Inactive

        public string? TeacherCode { get; set; }          // ví dụ GV-2024-0892
        public string? Department { get; set; }           // Khoa/Bộ môn
        public string? Degree { get; set; } = "Thạc sĩ";  // Học vị
        public string? Phone { get; set; }
        public DateTime? Birthday { get; set; }
    }
}
