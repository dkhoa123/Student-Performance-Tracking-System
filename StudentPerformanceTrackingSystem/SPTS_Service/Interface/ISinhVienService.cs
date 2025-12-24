using SPTS_Repository.Entities;
using StudentPerformanceTrackingSystem.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPTS_Service.Interface
{
    public interface ISinhVienService
    {
        Task DangKysv(DangKySinhVien model);
        Task<string?> CreateStuCode(int? CohortYear);
    }
}
