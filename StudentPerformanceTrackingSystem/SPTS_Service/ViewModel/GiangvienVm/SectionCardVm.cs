using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPTS_Service.ViewModel.GiangvienVm
{
    public class SectionCardViewModel
    {
        public int SectionId { get; set; }
        public string CourseCode { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public string Room { get; set; } = string.Empty;
        public int StudentCount { get; set; }
        public decimal CompletionRate { get; set; }
        public string ColorClass { get; set; } = string.Empty;

        // THÊM
        public int Credits { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Schedule { get; set; } = string.Empty;
        public string TimeSlot { get; set; } = string.Empty;
        public decimal? AverageScore { get; set; }
    }
}
