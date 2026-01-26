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


            // ✅ Chỉ filter khi termId có giá trị
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

        public async Task<UserDetailDto?> GetUserDetailAsync(int userId)
        {
            var user = await _context.Users
                .AsNoTracking()
                .Include(u => u.Student)
                .Include(u => u.Teacher)
                .Where(u => u.UserId == userId)
                .FirstOrDefaultAsync();

            if (user == null) return null;

            return new UserDetailDto
            {
                UserId = user.UserId,
                FullName = user.FullName ?? "",
                Email = user.Email ?? "",
                Role = user.Role ?? "",
                Status = user.Status ?? "",

                // Student info
                StudentCode = user.Student?.StudentCode,
                Major = user.Student?.Major,
                CohortYear = user.Student?.CohortYear,
                DepartmentId = user.Student?.DepartmentId,

                // Teacher info
                TeacherCode = user.Teacher?.TeacherCode,
                Degree = user.Teacher?.Degree,
                DepartmentName = user.Teacher?.DemparmentName
            };
        }

        // ✅ Cập nhật UpdateUserAsync
        public async Task<bool> UpdateUserAsync(UserUpdateDto dto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var user = await _context.Users
                    .Include(u => u.Student)
                    .Include(u => u.Teacher)
                    .FirstOrDefaultAsync(u => u.UserId == dto.UserId);

                if (user == null) return false;

                var emailExists = await _context.Users
                    .AnyAsync(u => u.Email == dto.Email && u.UserId != dto.UserId);
                if (emailExists) throw new Exception("Email đã tồn tại");

                var oldRole = user.Role;
                var newRole = dto.Role;

                user.FullName = dto.FullName;
                user.Email = dto.Email;
                user.Status = dto.Status;

                // STUDENT → TEACHER
                if (oldRole == "STUDENT" && newRole == "TEACHER")
                {
                    await ConvertStudentToTeacher(user, dto.TeacherCode, dto.Degree, dto.DepartmentName);
                }
                // STUDENT → ADMIN
                else if (oldRole == "STUDENT" && newRole == "ADMIN")
                {
                    await RemoveStudentData(user);
                }
                // TEACHER → STUDENT
                else if (oldRole == "TEACHER" && newRole == "STUDENT")
                {
                    await ConvertTeacherToStudent(user, dto.StudentCode, dto.Major, dto.CohortYear, dto.DepartmentId);
                }
                // TEACHER → ADMIN
                else if (oldRole == "TEACHER" && newRole == "ADMIN")
                {
                    await RemoveTeacherData(user);
                }
                // ADMIN → STUDENT
                else if (oldRole == "ADMIN" && newRole == "STUDENT")
                {
                    await CreateStudentData(user, dto.StudentCode, dto.Major, dto.CohortYear, dto.DepartmentId);
                }
                // ADMIN → TEACHER
                else if (oldRole == "ADMIN" && newRole == "TEACHER")
                {
                    await CreateTeacherData(user, dto.TeacherCode, dto.Degree, dto.DepartmentName);
                }
                // Same role - update info
                else if (oldRole == newRole)
                {
                    if (newRole == "STUDENT")
                    {
                        await UpdateStudentCode(user, dto.StudentCode, dto.Major, dto.CohortYear, dto.DepartmentId);
                    }
                    else if (newRole == "TEACHER")
                    {
                        await UpdateTeacherCode(user, dto.TeacherCode, dto.Degree, dto.DepartmentName);
                    }
                }

                user.Role = newRole;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        // ========== HELPER METHODS ==========

        // ✅ Cập nhật các conversion methods
        private async Task ConvertStudentToTeacher(User user, string? teacherCode, string? degree = null, string? departmentName = null)
        {
            if (user.Student != null)
            {
                var hasGrades = await _context.Grades.AnyAsync(g => g.StudentId == user.UserId);
                var hasAlerts = await _context.Alerts.AnyAsync(a => a.StudentId == user.UserId);

                if (hasGrades || hasAlerts)
                {
                    throw new Exception("Không thể chuyển sang Giảng viên do sinh viên có dữ liệu điểm/cảnh báo. Vui lòng xóa dữ liệu trước.");
                }

                var sectionStudents = _context.SectionStudents.Where(ss => ss.StudentId == user.UserId);
                _context.SectionStudents.RemoveRange(sectionStudents);

                var termGpas = _context.TermGpas.Where(tg => tg.StudentId == user.UserId);
                _context.TermGpas.RemoveRange(termGpas);

                _context.Students.Remove(user.Student);
            }

            await CreateTeacherData(user, teacherCode, degree, departmentName);
        }

        private async Task ConvertTeacherToStudent(User user, string? studentCode, string? major = null, int? cohortYear = null, int? departmentId = null)
        {
            if (user.Teacher != null)
            {
                var hasSections = await _context.Sections.AnyAsync(s => s.TeacherId == user.UserId);
                var isDepartmentHead = await _context.Departments.AnyAsync(d => d.HeadTeacherId == user.UserId);

                if (hasSections)
                {
                    throw new Exception("Không thể chuyển sang Sinh viên do giảng viên đang có lớp học phần. Vui lòng gỡ phân công trước.");
                }

                if (isDepartmentHead)
                {
                    throw new Exception("Không thể chuyển sang Sinh viên do giảng viên đang là trưởng khoa. Vui lòng chuyển quyền trước.");
                }

                _context.Teachers.Remove(user.Teacher);
            }

            await CreateStudentData(user, studentCode, major, cohortYear, departmentId);
        }

        private async Task RemoveStudentData(User user)
        {
            if (user.Student != null)
            {
                // Kiểm tra ràng buộc
                var hasGrades = await _context.Grades.AnyAsync(g => g.StudentId == user.UserId);
                var hasAlerts = await _context.Alerts.AnyAsync(a => a.StudentId == user.UserId);

                if (hasGrades || hasAlerts)
                {
                    throw new Exception("Không thể chuyển sang Admin do sinh viên có dữ liệu điểm/cảnh báo. Vui lòng xóa dữ liệu trước.");
                }

                // Xóa dữ liệu liên quan
                var sectionStudents = _context.SectionStudents.Where(ss => ss.StudentId == user.UserId);
                _context.SectionStudents.RemoveRange(sectionStudents);

                var termGpas = _context.TermGpas.Where(tg => tg.StudentId == user.UserId);
                _context.TermGpas.RemoveRange(termGpas);

                _context.Students.Remove(user.Student);
            }
        }

        private async Task RemoveTeacherData(User user)
        {
            if (user.Teacher != null)
            {
                var hasSections = await _context.Sections.AnyAsync(s => s.TeacherId == user.UserId);
                var isDepartmentHead = await _context.Departments.AnyAsync(d => d.HeadTeacherId == user.UserId);

                if (hasSections)
                {
                    throw new Exception("Không thể chuyển sang Admin do giảng viên đang có lớp học phần.");
                }

                if (isDepartmentHead)
                {
                    throw new Exception("Không thể chuyển sang Admin do giảng viên đang là trưởng khoa.");
                }

                _context.Teachers.Remove(user.Teacher);
            }
        }

        // ✅ Sửa CreateStudentData để nhận thêm thông tin
        private async Task CreateStudentData(User user, string? studentCode, string? major = null, int? cohortYear = null, int? departmentId = null)
        {
            // Validate student code
            if (string.IsNullOrWhiteSpace(studentCode))
            {
                throw new Exception("Mã sinh viên không được để trống");
            }

            // Validate major
            if (string.IsNullOrWhiteSpace(major))
            {
                throw new Exception("Ngành học không được để trống");
            }

            // Check unique student code
            var codeExists = await _context.Students.AnyAsync(s => s.StudentCode == studentCode);
            if (codeExists)
            {
                throw new Exception($"Mã sinh viên '{studentCode}' đã tồn tại");
            }

            // Tạo Student record
            var student = new Student
            {
                StudentId = user.UserId,
                StudentCode = studentCode.Trim(),
                Major = major.Trim(),
                CohortYear = cohortYear ?? DateTime.Now.Year,
                DepartmentId = departmentId,
                Gender = null,
                Phone = null,
                Address = null
            };

            _context.Students.Add(student);
        }

        // ✅ Sửa CreateTeacherData để nhận thêm thông tin
        private async Task CreateTeacherData(User user, string? teacherCode, string? degree = null, string? departmentName = null)
        {
            // Validate teacher code
            if (string.IsNullOrWhiteSpace(teacherCode))
            {
                throw new Exception("Mã giảng viên không được để trống");
            }

            // Check unique teacher code
            var codeExists = await _context.Teachers.AnyAsync(t => t.TeacherCode == teacherCode);
            if (codeExists)
            {
                throw new Exception($"Mã giảng viên '{teacherCode}' đã tồn tại");
            }

            // Tạo Teacher record
            var teacher = new Teacher
            {
                TeacherId = user.UserId,
                TeacherCode = teacherCode.Trim(),
                Degree = degree?.Trim(),
                DemparmentName = departmentName?.Trim(),
                Phone = null
            };

            _context.Teachers.Add(teacher);
        }


        // ✅ Sửa UpdateStudentCode để update thêm thông tin
        private async Task UpdateStudentCode(User user, string? studentCode, string? major = null, int? cohortYear = null, int? departmentId = null)
        {
            if (user.Student == null) return;

            if (string.IsNullOrWhiteSpace(studentCode))
            {
                throw new Exception("Mã sinh viên không được để trống");
            }

            // Check unique
            var codeExists = await _context.Students
                .AnyAsync(s => s.StudentCode == studentCode && s.StudentId != user.UserId);
            if (codeExists)
            {
                throw new Exception($"Mã sinh viên '{studentCode}' đã tồn tại");
            }

            user.Student.StudentCode = studentCode.Trim();

            if (!string.IsNullOrWhiteSpace(major))
                user.Student.Major = major.Trim();

            if (cohortYear.HasValue)
                user.Student.CohortYear = cohortYear.Value;

            if (departmentId.HasValue)
                user.Student.DepartmentId = departmentId.Value;
        }

        // ✅ Sửa UpdateTeacherCode để update thêm thông tin
        private async Task UpdateTeacherCode(User user, string? teacherCode, string? degree = null, string? departmentName = null)
        {
            if (user.Teacher == null) return;

            if (string.IsNullOrWhiteSpace(teacherCode))
            {
                throw new Exception("Mã giảng viên không được để trống");
            }

            // Check unique
            var codeExists = await _context.Teachers
                .AnyAsync(t => t.TeacherCode == teacherCode && t.TeacherId != user.UserId);
            if (codeExists)
            {
                throw new Exception($"Mã giảng viên '{teacherCode}' đã tồn tại");
            }

            user.Teacher.TeacherCode = teacherCode.Trim();

            if (!string.IsNullOrWhiteSpace(degree))
                user.Teacher.Degree = degree.Trim();

            if (!string.IsNullOrWhiteSpace(departmentName))
                user.Teacher.DemparmentName = departmentName.Trim();
        }


        public async Task<bool> DeleteUserAsync(int userId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId);
                if (user == null) return false;

                // Delete related records based on role
                if (user.Role == "STUDENT")
                {
                    // ❌ Cách CŨ (sai): 
                    // var student = await _context.Students.FirstOrDefaultAsync(s => s.StudentId == userId);

                    // ✅ Cách MỚI: Load student với Include để tránh lỗi FK
                    var student = await _context.Students
                        .Include(s => s.SectionStudents)
                        .Include(s => s.Grades)
                        .Include(s => s.TermGpas)
                        .Include(s => s.Alerts)
                        .FirstOrDefaultAsync(s => s.StudentId == userId);

                    if (student != null)
                    {
                        // Xóa các bảng phụ thuộc TRƯỚC
                        // 1. Xóa Grades (phụ thuộc vào SectionStudents)
                        if (student.Grades.Any())
                        {
                            _context.Grades.RemoveRange(student.Grades);
                        }

                        // 2. Xóa SectionStudents
                        if (student.SectionStudents.Any())
                        {
                            _context.SectionStudents.RemoveRange(student.SectionStudents);
                        }

                        // 3. Xóa TermGpas
                        if (student.TermGpas.Any())
                        {
                            _context.TermGpas.RemoveRange(student.TermGpas);
                        }

                        // 4. Xóa Alerts
                        if (student.Alerts.Any())
                        {
                            // Xóa Reminders trước (phụ thuộc vào Alerts)
                            var alertIds = student.Alerts.Select(a => a.AlertId).ToList();
                            var reminders = await _context.Reminders
                                .Where(r => alertIds.Contains(r.AlertId))
                                .ToListAsync();
                            if (reminders.Any())
                            {
                                _context.Reminders.RemoveRange(reminders);
                            }

                            // Xóa Notifications liên quan đến Alerts
                            var notifications = await _context.Notifications
                                .Where(n => n.RelatedAlertId.HasValue && alertIds.Contains(n.RelatedAlertId.Value))
                                .ToListAsync();
                            if (notifications.Any())
                            {
                                _context.Notifications.RemoveRange(notifications);
                            }

                            _context.Alerts.RemoveRange(student.Alerts);
                        }

                        // 5. Xóa record Student (quan hệ 1-1 với User)
                        _context.Students.Remove(student);
                    }
                }
                else if (user.Role == "TEACHER")
                {
                    var teacher = await _context.Teachers
                        .Include(t => t.Sections)
                        .Include(t => t.Grades)
                        .Include(t => t.Departments)
                        .FirstOrDefaultAsync(t => t.TeacherId == userId);

                    if (teacher != null)
                    {
                        // Kiểm tra nếu giảng viên là trưởng khoa
                        if (teacher.Departments.Any())
                        {
                            throw new Exception("Không thể xóa giảng viên đang là trưởng khoa. Vui lòng chuyển quyền trưởng khoa trước.");
                        }

                        // Kiểm tra nếu có lớp học phần đang dạy
                        if (teacher.Sections.Any())
                        {
                            throw new Exception("Không thể xóa giảng viên đang có lớp học phần. Vui lòng gỡ phân công giảng dạy trước.");
                        }

                        // Xóa Grades do teacher nhập
                        if (teacher.Grades.Any())
                        {
                            // Chỉ set UpdatedBy = null thay vì xóa điểm
                            foreach (var grade in teacher.Grades)
                            {
                                grade.UpdatedBy = null;
                            }
                        }

                        // Xóa record Teacher (quan hệ 1-1 với User)
                        _context.Teachers.Remove(teacher);
                    }
                }

                // Xóa Notifications của user
                var userNotifications = await _context.Notifications
                    .Where(n => n.UserId == userId)
                    .ToListAsync();
                if (userNotifications.Any())
                {
                    _context.Notifications.RemoveRange(userNotifications);
                }

                // Cuối cùng xóa User
                _context.Users.Remove(user);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new Exception($"Lỗi khi xóa người dùng: {ex.Message}", ex);
            }
        }

        public async Task<List<Department>> GetDepartmentsAsync()
        {
            return await _context.Departments
                .AsNoTracking()
                .Where(d => d.Status == "ACTIVE")
                .OrderBy(d => d.DepartmentName)
                .ToListAsync();
        }

        public async Task<List<string>> GetMajorsAsync()
        {
            return await _context.Students
                .AsNoTracking()
                .Where(s => !string.IsNullOrEmpty(s.Major))
                .Select(s => s.Major!)
                .Distinct()
                .OrderBy(m => m)
                .ToListAsync();
        }
    }
}