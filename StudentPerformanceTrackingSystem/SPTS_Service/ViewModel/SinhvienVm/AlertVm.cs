using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPTS_Service.ViewModel.SinhvienVm
{
    public class AlertVm
    {
        public int AlertId { get; set; }
        public string AlertType { get; set; } = "";
        public string Severity { get; set; } = "";
        public string? CourseCode { get; set; }
        public string? Reason { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
