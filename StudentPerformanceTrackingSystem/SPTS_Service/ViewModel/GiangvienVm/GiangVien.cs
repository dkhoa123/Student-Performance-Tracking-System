using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPTS_Service.ViewModel.GiangvienVm
{
    public class GiangVien
    {
        public string TeacherName { get; set; }

        // Stats
        public int TotalStudents { get; set; }
        public int NewStudentsThisMonth { get; set; }
        public decimal AverageScore { get; set; }
        public int AtRiskStudents { get; set; }
        public int ActiveSectionsCount { get; set; } // MỚI

        // Sections nhóm theo học kỳ
        public List<TermSectionsVm> SectionsByTerm { get; set; }
        // THÊM MỚI - cho dropdown học kỳ
        public List<TermOptionVm> AvailableTerms { get; set; } = new();
        public int? SelectedTermId { get; set; }
        public List<AlertViewModel> RecentAlerts { get; set; }
        public List<ChartDataVm> ChartData { get; set; }
    }
}
