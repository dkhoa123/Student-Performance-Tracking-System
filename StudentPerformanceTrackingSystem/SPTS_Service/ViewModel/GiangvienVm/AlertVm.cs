using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPTS_Service.ViewModel.GiangvienVm
{
    public class AlertViewModel
    {
        public string StudentName { get; set; } = string.Empty;
        public string AlertType { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty;
        public string IconName { get; set; } = string.Empty;
        public string IconColor { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public int NewAlertsCount { get; set; }
    }
}
