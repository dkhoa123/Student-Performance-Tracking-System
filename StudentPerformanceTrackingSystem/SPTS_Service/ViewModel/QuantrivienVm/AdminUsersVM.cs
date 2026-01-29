
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPTS_Service.ViewModel.QuantrivienVm
{
    public class AdminUsersVM
    {
        public List<UserRowVM> Users { get; set; } = new();

        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }

        public string? Role { get; set; }
        public string? Status { get; set; }
        public string? Keyword { get; set; }

        public int TotalPages => PageSize <= 0 ? 0 : (int)System.Math.Ceiling((double)TotalCount / PageSize);

        public int From => TotalCount == 0 ? 0 : (Page - 1) * PageSize + 1;
        public int To => System.Math.Min(Page * PageSize, TotalCount);
    }
}
