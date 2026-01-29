using Microsoft.EntityFrameworkCore;
using SPTS_Repository.DTOs.Quantrivien;
using SPTS_Repository.Entities;
using SPTS_Repository.Interface.Admin;
using SPTS_Shared.Constants;

namespace SPTS_Repository.Repositories.Quantrivien
{
    public class StatisticsRepository : IStatisticsRepository
    {
        private readonly SptsContext _context;

        public StatisticsRepository(SptsContext context)
        {
            _context = context;
        }
        public Task<int> CountCoursesAsync()
            => _context.Courses.AsNoTracking().CountAsync();

        public async Task<int> CountSectionsAsync(int? termId)
        {
            var query = _context.Sections.AsNoTracking().AsQueryable();
            if (termId.HasValue) query = query.Where(s => s.TermId == termId.Value);
            return await query.CountAsync();
        }

        public async Task<int> CountTeachingTeachersAsync(int? termId)
        {
            var query = _context.Sections.AsNoTracking().AsQueryable();
            if (termId.HasValue) query = query.Where(s => s.TermId == termId.Value);

            return await query
                .Where(s => s.TeacherId != null)
                .Select(s => s.TeacherId)
                .Distinct()
                .CountAsync();
        }

        public async Task<int> CountUnassignedSectionsAsync(int? termId)
        {
            var query = _context.Sections.AsNoTracking().AsQueryable();
            if (termId.HasValue) query = query.Where(s => s.TermId == termId.Value);

            return await query.Where(s => s.TeacherId == null).CountAsync();
        }

        public async Task<List<DepartmentAlertDto>> GetDepartmentAlerts(int? termId = null)
        {
            // ✅ FIX: Tách query thành 2 bước và đếm DISTINCT sinh viên
            var rawData = termId.HasValue
                ? await (from s in _context.Students
                         join a in _context.Alerts on s.StudentId equals a.StudentId
                         where s.StudentNavigation.Status == UserStatus.Active && a.TermId == termId.Value
                         group new { s.StudentId, s.Major } by s.Major into g
                         select new
                         {
                             Major = g.Key,
                             TotalStudents = g.Select(x => x.StudentId).Distinct().Count(),
                             AlertedStudents = g.Select(x => x.StudentId).Distinct().Count(), // ✅ Số SV bị CB
                             AlertCount = g.Count() // ✅ Tổng số cảnh báo
                         }).ToListAsync()
                : await (from s in _context.Students
                         join a in _context.Alerts on s.StudentId equals a.StudentId
                         where s.StudentNavigation.Status == UserStatus.Active
                         group new { s.StudentId, s.Major } by s.Major into g
                         select new
                         {
                             Major = g.Key,
                             TotalStudents = g.Select(x => x.StudentId).Distinct().Count(),
                             AlertedStudents = g.Select(x => x.StudentId).Distinct().Count(),
                             AlertCount = g.Count()
                         }).ToListAsync();

            // ✅ Thêm: Lấy tổng số sinh viên của mỗi khoa (bao gồm cả SV không bị CB)
            var totalStudentsByMajor = await _context.Students
                .Where(s => s.StudentNavigation.Status == UserStatus.Active)
                .GroupBy(s => s.Major)
                .Select(g => new
                {
                    Major = g.Key,
                    Total = g.Count()
                })
                .ToListAsync();

            var totalStudentsDict = totalStudentsByMajor.ToDictionary(x => x.Major ?? "", x => x.Total);

            // Process in memory
            return rawData.Select(x =>
            {
                var major = x.Major ?? "Chưa xác định";
                var totalStudentsInMajor = totalStudentsDict.ContainsKey(x.Major ?? "")
                    ? totalStudentsDict[x.Major ?? ""]
                    : x.TotalStudents;

                return new DepartmentAlertDto
                {
                    DepartmentName = "Khoa " + major,
                    DepartmentCode = GenerateDepartmentCode(x.Major),
                    TotalStudents = totalStudentsInMajor, // ✅ Tổng SV của khoa
                    AlertCount = x.AlertedStudents,       // ✅ Số SV bị cảnh báo
                    AlertRate = totalStudentsInMajor > 0
                        ? Math.Round((decimal)x.AlertedStudents / totalStudentsInMajor * 100, 1)
                        : 0
                };
            })
            .OrderByDescending(x => x.AlertRate)
            .ToList();
        }
        private string GenerateDepartmentCode(string? major)
        {
            if (string.IsNullOrEmpty(major)) return "UNKNOWN";

            var departmentMapping = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "Công nghệ thông tin", "CNTT" },
                { "CNTT", "CNTT" },
                { "Điện tử viễn thông", "DTVT" },
                { "Điện tử", "DTVT" },
                { "Cơ khí", "CKDL" },
                { "Quản trị", "QTKD" },
                { "Kinh tế", "KT" },
                { "Ngoại ngữ", "NN" },
                { "Y dược", "YD" },
                { "Y", "YD" },
                { "Xây dựng", "XD" },
                { "Hóa", "HH" },
                { "Môi trường", "MT" }
            };

            foreach (var kvp in departmentMapping)
            {
                if (major.Contains(kvp.Key, StringComparison.OrdinalIgnoreCase))
                {
                    return kvp.Value;
                }
            }

            var words = major.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var code = string.Join("", words.Select(w => char.ToUpper(w[0])));
            return code.Length > 0 ? code : "UNKNOWN";
        }

        public async Task<List<DepartmentGPADto>> GetDepartmentGPAs(int? termId = null)
        {
            IQueryable<DepartmentGPADto> query;

            if (termId.HasValue)
            {
                query = from s in _context.Students
                        join tg in _context.TermGpas on s.StudentId equals tg.StudentId
                        where s.StudentNavigation.Status == UserStatus.Active && tg.TermId == termId.Value
                        group tg by s.Major into g
                        select new DepartmentGPADto
                        {
                            DepartmentName = g.Key ?? "Chưa xác định",
                            AverageGPA = (decimal)g.Average(x => x.GpaValue),
                            StudentCount = g.Count()
                        };
            }
            else
            {
                query = from s in _context.Students
                        join tg in _context.TermGpas on s.StudentId equals tg.StudentId
                        where s.StudentNavigation.Status == UserStatus.Active
                        group tg by s.Major into g
                        select new DepartmentGPADto
                        {
                            DepartmentName = g.Key ?? "Chưa xác định",
                            AverageGPA = (decimal)g.Average(x => x.GpaValue),
                            StudentCount = g.Select(x => x.StudentId).Distinct().Count()
                        };
            }

            return await query
                .OrderByDescending(x => x.AverageGPA)
                .ToListAsync();
        }
    }
}
