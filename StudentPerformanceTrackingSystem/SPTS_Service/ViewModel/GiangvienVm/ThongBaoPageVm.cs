using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPTS_Service.ViewModel.GiangvienVm
{
    public class ThongBaoPageVm
    {
        public int SectionId { get; set; }
        public string CourseCode { get; set; } = "";
        public string CourseName { get; set; } = "";
        public string TermName { get; set; } = "";
        public int StudentCount { get; set; }

        // Danh sách các lớp của giảng viên (để render phần "Chọn Lớp Học")
        public List<SectionOptionVm> AvailableSections { get; set; } = new();

        // Gửi broadcast
        public string? BroadcastTitle { get; set; }
        public string? BroadcastContent { get; set; }

        // Danh sách sinh viên
        public List<StudentRowVm> Students { get; set; } = new();

        // Phân trang
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
