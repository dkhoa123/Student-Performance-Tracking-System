using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPTS_Service.ViewModel.GiangvienVm
{
    public class StudentRowVm
    {
        public int StudentId { get; set; }
        public string StudentCode { get; set; } = "";
        public string FullName { get; set; } = "";

        // Tình trạng học tập
        public string StatusLabel { get; set; } = "Bình thường";
        public string StatusBadgeClass { get; set; } = "bg-slate-100";
        public string StatusIcon { get; set; } = "remove";
        public string StatusDetail { get; set; } = "";
        public string StatusColorClass { get; set; } = "bg-slate-500";
    }
}
