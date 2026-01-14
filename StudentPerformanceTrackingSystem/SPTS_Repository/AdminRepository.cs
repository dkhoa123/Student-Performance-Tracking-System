using Microsoft.EntityFrameworkCore;
using SPTS_Repository.Entities;
using SPTS_Repository.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SPTS_Repository
{
    public class AdminRepository : IAdminRepository
    {
        private readonly SptsContext _context;

        public AdminRepository(SptsContext context)
        {
            _context = context;
        }

        public async Task<KPIScorecardDto> GetKPIScorecard(int? termId = null)
        {
            var totalStudents = await _context.Students
                .Where(s => s.StudentNavigation.Status == "ACTIVE")
                .CountAsync();

            var totalTeachers = await _context.Teachers
                .Where(t => t.TeacherNavigation.Status == "ACTIVE")
                .CountAsync();

            var gpasQuery = _context.TermGpas.AsQueryable();
            if (termId.HasValue)
                gpasQuery = gpasQuery.Where(tg => tg.TermId == termId.Value);

            var averageGPA = await gpasQuery.AverageAsync(tg => (decimal?)tg.GpaValue) ?? 0;

            var alertsQuery = _context.Alerts
                .Where(a => a.Status == "NEW" || a.Status == "IN_PROGRESS");

            if (termId.HasValue)
                alertsQuery = alertsQuery.Where(a => a.TermId == termId.Value);

            var totalAlerts = await alertsQuery.CountAsync();
            var alertRate = totalStudents > 0 ? (decimal)totalAlerts / totalStudents * 100 : 0;

            return new KPIScorecardDto
            {
                TotalStudents = totalStudents,
                TotalTeachers = totalTeachers,
                AverageGPA = Math.Round(averageGPA, 2),
                AlertRate = Math.Round(alertRate, 1),
                TotalAlerts = totalAlerts,
                StudentTeacherRatio = totalTeachers > 0 ? Math.Round((decimal)totalStudents / totalTeachers, 1) : 0
            };
        }

        public async Task<List<DepartmentGPADto>> GetDepartmentGPAs(int? termId = null)
        {
            IQueryable<DepartmentGPADto> query;

            if (termId.HasValue)
            {
                query = from s in _context.Students
                        join tg in _context.TermGpas on s.StudentId equals tg.StudentId
                        where s.StudentNavigation.Status == "ACTIVE" && tg.TermId == termId.Value
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
                        where s.StudentNavigation.Status == "ACTIVE"
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

        public async Task<AcademicRankingDto> GetAcademicRanking(int? termId = null)
        {
            var gpasQuery = _context.TermGpas.AsQueryable();

            if (termId.HasValue)
                gpasQuery = gpasQuery.Where(tg => tg.TermId == termId.Value);

            var totalCount = await gpasQuery.CountAsync();

            if (totalCount == 0)
            {
                return new AcademicRankingDto
                {
                    ExcellentRate = 0,
                    GoodRate = 0,
                    AverageRate = 0,
                    BelowAverageRate = 0,
                    PoorRate = 0
                };
            }

            var excellentCount = await gpasQuery.CountAsync(tg => tg.GpaValue >= 3.6m);
            var goodCount = await gpasQuery.CountAsync(tg => tg.GpaValue >= 3.2m && tg.GpaValue < 3.6m);
            var averageCount = await gpasQuery.CountAsync(tg => tg.GpaValue >= 2.5m && tg.GpaValue < 3.2m);
            var belowAvgCount = await gpasQuery.CountAsync(tg => tg.GpaValue >= 2.0m && tg.GpaValue < 2.5m);
            var poorCount = await gpasQuery.CountAsync(tg => tg.GpaValue < 2.0m);

            return new AcademicRankingDto
            {
                ExcellentRate = Math.Round((decimal)excellentCount / totalCount * 100, 1),
                GoodRate = Math.Round((decimal)goodCount / totalCount * 100, 1),
                AverageRate = Math.Round((decimal)averageCount / totalCount * 100, 1),
                BelowAverageRate = Math.Round((decimal)belowAvgCount / totalCount * 100, 1),
                PoorRate = Math.Round((decimal)poorCount / totalCount * 100, 1)
            };
        }

        public async Task<List<DepartmentAlertDto>> GetDepartmentAlerts(int? termId = null)
        {
            // ⚠️ FIX: Tách query thành 2 bước - query DB trước, process sau
            var rawData = termId.HasValue
                ? await (from s in _context.Students
                         join a in _context.Alerts on s.StudentId equals a.StudentId
                         where s.StudentNavigation.Status == "ACTIVE" && a.TermId == termId.Value
                         group new { s.StudentId, s.Major } by s.Major into g
                         select new
                         {
                             Major = g.Key,
                             TotalStudents = g.Select(x => x.StudentId).Distinct().Count(),
                             AlertCount = g.Count()
                         }).ToListAsync()
                : await (from s in _context.Students
                         join a in _context.Alerts on s.StudentId equals a.StudentId
                         where s.StudentNavigation.Status == "ACTIVE"
                         group new { s.StudentId, s.Major } by s.Major into g
                         select new
                         {
                             Major = g.Key,
                             TotalStudents = g.Select(x => x.StudentId).Distinct().Count(),
                             AlertCount = g.Count()
                         }).ToListAsync();

            // Process in memory (không còn LINQ to SQL)
            return rawData.Select(x => new DepartmentAlertDto
            {
                DepartmentName = "Khoa " + (x.Major ?? "Chưa xác định"),
                DepartmentCode = GenerateDepartmentCode(x.Major),  // ✅ OK vì đã ToList()
                TotalStudents = x.TotalStudents,
                AlertCount = x.AlertCount,
                AlertRate = x.TotalStudents > 0
                    ? Math.Round((decimal)x.AlertCount / x.TotalStudents * 100, 1)
                    : 0
            })
            .OrderByDescending(x => x.AlertRate)
            .ToList();
        }

        public async Task<(int currentStudents, int previousStudents)> GetStudentComparison(int? currentTermId, int? previousTermId)
        {
            var current = await _context.Students
                .CountAsync(s => s.StudentNavigation.Status == "ACTIVE");

            var previous = current;

            if (previousTermId.HasValue)
            {
                previous = await _context.SectionStudents
                    .Where(ss => ss.Section.TermId == previousTermId.Value)
                    .Select(ss => ss.StudentId)
                    .Distinct()
                    .CountAsync();
            }

            return (current, previous);
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

        public async Task<(List<User> Users, int TotalCount)> GetUsersAsync(
            string? role,
            string? status,
            string? keyword,
            int page,
            int pageSize)
        {
            if (page <= 0) page = 1;
            if (pageSize <= 0) pageSize = 10;

            var query = _context.Users.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(role) && role != "ALL")
                query = query.Where(u => u.Role == role);

            if (!string.IsNullOrWhiteSpace(status) && status != "ALL")
                query = query.Where(u => u.Status == status);

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                keyword = keyword.Trim();
                query = query.Where(u =>
                    u.FullName.Contains(keyword) ||
                    u.Email.Contains(keyword));
            }

            var total = await query.CountAsync();

            var users = await query
                .OrderByDescending(u => u.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (users, total);
        }

        public Task<User?> GetUserByIdAsync(int userId)
        {
            return _context.Users.FirstOrDefaultAsync(u => u.UserId == userId);
        }

        public async Task<bool> SetUserStatusAsync(int userId, string newStatus)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId);
            if (user == null) return false;

            user.Status = newStatus;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<(List<Section> Sections, int TotalCount)> GetSectionsForAdminAsync(int? termId, int page, int pageSize)
        {
            if (page <= 0) page = 1;
            if (pageSize <= 0) pageSize = 10;

            var query = _context.Sections
                .AsNoTracking()
                .Include(s => s.Course)
                .Include(s => s.Teacher)
                    .ThenInclude(t => t.TeacherNavigation) // Users
                .AsQueryable();

            if (termId.HasValue)
                query = query.Where(s => s.TermId == termId.Value);

            var total = await query.CountAsync();

            var sections = await query
                .OrderByDescending(s => s.SectionId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (sections, total);
        }

        public Task<int> CountCoursesAsync()
            => _context.Courses.AsNoTracking().CountAsync();

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

        public async Task<int> CountSectionsAsync(int? termId)
        {
            var query = _context.Sections.AsNoTracking().AsQueryable();
            if (termId.HasValue) query = query.Where(s => s.TermId == termId.Value);
            return await query.CountAsync();
        }

        public async Task<int> CountUnassignedSectionsAsync(int? termId)
        {
            var query = _context.Sections.AsNoTracking().AsQueryable();
            if (termId.HasValue) query = query.Where(s => s.TermId == termId.Value);

            return await query.Where(s => s.TeacherId == null).CountAsync();
        }

        public async Task<List<Term>> GetTermsAsync()
        {
            return await _context.Terms
                .AsNoTracking()
                .OrderByDescending(t => t.StartDate)
                .ToListAsync();
        }

        public Task<Term?> GetTermByIdAsync(int termId)
        {
            return _context.Terms.AsNoTracking().FirstOrDefaultAsync(t => t.TermId == termId);
        }
    }
}