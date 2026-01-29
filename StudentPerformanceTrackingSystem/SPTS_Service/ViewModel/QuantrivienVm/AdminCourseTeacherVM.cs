
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPTS_Service.ViewModel.QuantrivienVm
{
    public class AdminCourseTeacherVM
    {
        // Stats
        public int TotalCourses { get; set; }
        public int TeachingTeachers { get; set; }
        public int UnassignedSections { get; set; }
        public int TotalSections { get; set; }

        // Filters
        public int? TermId { get; set; }
        public string? TermName { get; set; }

        // dropdown/filter data
        public List<TermOptionVM> Terms { get; set; } = new();

        // Paging
        public int TotalCount { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;

        public int TotalPages => PageSize <= 0 ? 0 : (int)Math.Ceiling((double)TotalCount / PageSize);
        public int From => TotalCount == 0 ? 0 : (Page - 1) * PageSize + 1;
        public int To => Math.Min(Page * PageSize, TotalCount);

        // Table
        public List<CourseTeacherRowVM> Rows { get; set; } = new();

        // helper to render page numbers like: 1 2 3 ... 13
        public List<int> GetPageNumbers(int window = 1)
        {
            var pages = new List<int>();
            if (TotalPages <= 0) return pages;

            void add(int p) { if (!pages.Contains(p)) pages.Add(p); }

            add(1);
            for (int p = Page - window; p <= Page + window; p++)
                if (p > 1 && p < TotalPages) add(p);
            if (TotalPages > 1) add(TotalPages);

            pages.Sort();
            return pages;
        }
    }
}
