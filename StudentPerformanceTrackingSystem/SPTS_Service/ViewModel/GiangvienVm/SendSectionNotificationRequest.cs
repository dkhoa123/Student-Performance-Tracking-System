using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPTS_Service.ViewModel.GiangvienVm
{
    public class SendSectionNotificationRequest
    {
        public int SectionId { get; set; }
        public string Title { get; set; } = "Thông báo từ giảng viên";
        public string Content { get; set; } = "";
    }
}
