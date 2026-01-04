using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPTS_Service.ViewModel
{
    public class NhapDiem
    {
        public int? SelectedTermId { get; set; }
        public int? SelectedSectionId { get; set; }
        public string? SelectedGradeType { get; set; }

        public string? CourseName { get; set; }

        public int TotalStudents { get; set; }
        public int EnteredCount { get; set; }
        public decimal AverageScore { get; set; }

        public List<TermOptionVm> Terms { get; set; } = new();
        public List<SectionOptionVm> Sections { get; set; } = new();
        public List<SelectOptionVm> GradeTypes { get; set; } = new();

        public List<StudentGradeRowVm> Students { get; set; } = new();
    }

    public class TermOption
    {
        public int TermId { get; set; }
        public string TermName { get; set; } = "";
    }

    public class SectionOptionVm
    {
        public int SectionId { get; set; }
        public string DisplayName { get; set; } = ""; // ví dụ: "CT101 - Toán - 10A1"
    }

    public class SelectOptionVm
    {
        public string Value { get; set; } = "";
        public string Text { get; set; } = "";
    }

    public class StudentGradeRowVm
    {
        public int StudentId { get; set; }
        public string StudentCode { get; set; } = "";
        public string FullName { get; set; } = "";
        public decimal? Score { get; set; }
        public string? Note { get; set; }
    }

    public class SaveGradesRequestVm
    {
        public int TermId { get; set; }
        public int SectionId { get; set; }
        public string GradeType { get; set; } = "";

        public List<SaveGradeItemVm> Items { get; set; } = new();
    }

    public class SaveGradeItemVm
    {
        public int StudentId { get; set; }
        public decimal? Score { get; set; }
        public string? Note { get; set; }
    }
}