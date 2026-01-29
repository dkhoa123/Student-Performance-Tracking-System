using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPTS_Service.ViewModel.GiangvienVm
{
    public class SendStudentNotificationRequest
    {
        public int SectionId { get; set; }
        public int StudentId { get; set; }   // cũng chính là UserId
        public string Title { get; set; } = "Thông báo từ giảng viên";
        public string Content { get; set; } = "";
    }
}
