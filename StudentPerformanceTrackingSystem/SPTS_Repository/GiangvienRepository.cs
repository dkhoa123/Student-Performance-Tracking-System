using Microsoft.EntityFrameworkCore;
using SPTS_Repository.Entities;
using SPTS_Repository.Interface;
using System.Linq;
using static System.Collections.Specialized.BitVector32;

namespace SPTS_Repository
{
    public class GiangvienRepository : IGiangvienRepository
    {
        private readonly SptsContext _context;
        public GiangvienRepository(SptsContext context)
        {
            _context = context;
        }

        public async Task<int> GetAtRiskStudentsCountAsync(int teacherId)
        {
            return await _context.Alerts
                .Where(a => a.Section.TeacherId == teacherId &&
                            a.Status != "CLOSED" &&
                            (a.Severity == "HIGH" || a.Severity == "MEDIUM"))
                .Select(a => a.StudentId)
                .Distinct()
                .CountAsync();
        }

        public async Task<decimal> GetAverageScoreByTeacherAsync(int teacherId)
        {
            var avgScore = await _context.Grades
                .Where(g => g.Section.TeacherId == teacherId &&
                            g.TotalScore.HasValue)
                .AverageAsync(g => (decimal?)g.TotalScore);

            return avgScore ?? 0;
        }

        public Task<Teacher> GetByUserIdAsync(int userId)
        {
            throw new NotImplementedException();
        }

        public async Task<List<ChartDataViewModelDto>> GetGpaChartDataByTeacherAsync(int teacherId, int? TermId = null)
        {
            return await _context.TermGpas
                .Where(tg => _context.SectionStudents
                    .Any(ss => ss.StudentId == tg.StudentId &&
                               ss.Section.TeacherId == teacherId))
                .GroupBy(tg => new { tg.TermId, tg.Term.TermName, tg.Term.StartDate })
                .OrderBy(g => g.Key.StartDate)
                .Select(g => new ChartDataViewModelDto(
                            g.Key.TermId,
                            g.Key.TermName,
                            g.Average(tg => (decimal?)tg.GpaValue) ?? 0
                    )).ToListAsync();
        }

        public async Task<int> GetNewStudentsThisMonthAsync(int teacherId)
        {
            var oneMonthAgo = DateTime.UtcNow.AddMonths(-1);

            return await _context.SectionStudents
                .Where(ss => ss.Section.TeacherId == teacherId &&
                             ss.AddedAt >= oneMonthAgo)
                .CountAsync();
        }

        public async Task<List<AlertViewModelDto>> GetRecentAlertsByTeacherAsync(int teacherId, int top = 3)
        {
            return await _context.Alerts
                .Include(a => a.Student)
                .Include(a => a.Section)
                .Where(a => a.Section.TeacherId == teacherId &&
                            (a.Status == "NEW" || a.Status == "SENT"))
                .OrderByDescending(a => a.Severity)
                .ThenByDescending(a => a.CreatedAt)
                .Take(top)
                .Join(
                    _context.Users,
                    alert => alert.StudentId,
                    user => user.UserId,
                    (alert, user) => new { alert, user }
                )
                .Select(x => new AlertViewModelDto(
                             x.user.FullName,
                             x.alert.AlertType,
                             x.alert.Reason ?? GetDefaultMessage(x.alert.AlertType, x.alert.ActualValue),
                             x.alert.Severity,
                             GetIconName(x.alert.AlertType),
                             GetIconColor(x.alert.Severity),
                             x.alert.CreatedAt
                    )).ToListAsync();
        }

        private static string GetIconColor(string severity)
        {
            return severity switch
            {
                "HIGH" => "red",
                "MEDIUM" => "orange",
                "LOW" => "yellow",
                _ => "gray"
            };
        }

        private static string GetIconName(string alertType)
        {
            return alertType switch
            {
                "LOW_TOTAL" or "LOW_FINAL" or "LOW_GPA" => "trending_down",
                "ABSENT" => "event_busy",
                "MISSING_ASSIGNMENT" => "assignment_late",
                _ => "warning"
            };
        }

        private static string GetDefaultMessage(string alertType, decimal? actualValue)
        {
            return alertType switch
            {
                "LOW_TOTAL" => $"Điểm tổng kết dưới {actualValue}",
                "LOW_FINAL" => $"Điểm cuối kỳ dưới {actualValue}",
                "LOW_GPA" => $"GPA dưới {actualValue}",
                _ => "Cần chú ý"
            };
        }

        public async Task<List<SectionCardViewModelDto>> GetSectionsByTeacherAsync(int teacherId)
        {
            // Bước 1: Query data về memory
            var sections = await _context.Sections
                .Include(sec => sec.SectionSchedules)
                .Include(sec => sec.Course)
                .Include(sec => sec.Term)
                .Include(sec => sec.SectionStudents)
                .Include(sec => sec.Grades)
                .Where(sec => sec.TeacherId == teacherId)
                .ToListAsync();

            // Bước 2: Map sang DTO trong memory
            return sections.Select(sec => new SectionCardViewModelDto(
                sec.SectionId,
                sec.Course.CourseCode,
                sec.Course.CourseName,

                // Room
                sec.SectionSchedules
                    .OrderBy(ss => GetDayOrder(ss.DayOfWeek))
                    .ThenBy(ss => ss.StartPeriod)
                    .FirstOrDefault()?.Room ?? "TBA",

                // Student count
                sec.SectionStudents.Count(ss => ss.Status == "ACTIVE"),

                // Completion rate
                sec.SectionStudents.Any()
                    ? (decimal)sec.Grades.Count(g => g.TotalScore != null) * 100 / sec.SectionStudents.Count()
                    : 0,

                GetRandomColor(),
                sec.Course.Credits,
                sec.Status,

                // Schedule:  "Thứ 2, Tiết 1-3; Thứ 5, Tiết 4-6"
                string.Join("; ", sec.SectionSchedules
                    .OrderBy(ss => GetDayOrder(ss.DayOfWeek))
                    .ThenBy(ss => ss.StartPeriod)
                    .Select(ss =>
                        GetDayNameVietnamese(ss.DayOfWeek) +
                        ", Tiết " + ss.StartPeriod + "-" + ss.EndPeriod
                    )),

                // Time slot:  "07:00 - 09:30"
                // ✅ FIX: Sử dụng string interpolation thay vì ToString với format
                sec.SectionSchedules
                    .OrderBy(ss => GetDayOrder(ss.DayOfWeek))
                    .ThenBy(ss => ss.StartPeriod)
                    .Select(ss => $"{ss.StartTime:hh\\:mm} - {ss.EndTime:hh\\:mm}")
                    .FirstOrDefault() ?? "",

                // Average score
                sec.Grades.Any(g => g.TotalScore.HasValue)
                    ? sec.Grades.Where(g => g.TotalScore.HasValue)
                                .Average(g => g.TotalScore!.Value)
                    : (decimal?)null,

                sec.TermId,
                sec.Term.TermName,
                // ✅ FIX: Convert DateOnly to DateTime
                sec.Term.StartDate.HasValue
                    ? sec.Term.StartDate.Value.ToDateTime(TimeOnly.MinValue)
                    : (DateTime?)null
            ))
            .OrderByDescending(s => s.StartDate)
            .ToList();
        }

        // ✅ Helper methods
        private static int GetDayOrder(string dayOfWeek)
        {
            return dayOfWeek switch
            {
                "MONDAY" => 1,
                "TUESDAY" => 2,
                "WEDNESDAY" => 3,
                "THURSDAY" => 4,
                "FRIDAY" => 5,
                "SATURDAY" => 6,
                "SUNDAY" => 7,
                _ => 8
            };
        }

        private static string GetDayNameVietnamese(string dayOfWeek)
        {
            return dayOfWeek switch
            {
                "MONDAY" => "Thứ 2",
                "TUESDAY" => "Thứ 3",
                "WEDNESDAY" => "Thứ 4",
                "THURSDAY" => "Thứ 5",
                "FRIDAY" => "Thứ 6",
                "SATURDAY" => "Thứ 7",
                "SUNDAY" => "CN",
                _ => ""
            };
        }

        // ✅ FIX: Xóa duplicate method, chỉ giữ 1 instance method
        private string GetRandomColor()
        {
            var colors = new[] { "purple", "orange", "blue", "green", "pink" };
            return colors[new Random().Next(colors.Length)];
        }

        // THÊM method đếm số lớp đang hoạt động
        public async Task<int> GetActiveSectionsCountAsync(int teacherId)
        {
            return await _context.Sections
                .Where(sec => sec.TeacherId == teacherId && sec.Status == "OPEN")
                .CountAsync();
        }

        public Task<User> GetTeacherUserAsync(int teacherId)
        {
            throw new NotImplementedException();
        }

        public async Task<int> GetTotalStudentsByTeacherAsync(int teacherId)
        {
            return await _context.SectionStudents
                .Where(ss => ss.Section.TeacherId == teacherId && ss.Status == "ACTIVE")
                .Select(ss => ss.StudentId)
                .Distinct()
                .CountAsync();
        }

        public async Task<ChiTietLopDto> GetSectionDetailAsync(int sectionId)
        {
            var sec = await _context.Sections
                .Include(x => x.Course)
                .Include(x => x.Term)
                .Include(x => x.SectionSchedules)
                .SingleOrDefaultAsync(x => x.SectionId == sectionId);

            if (sec == null) throw new Exception("Không tìm thấy lớp.");

            // room + schedule (lấy cái đầu tiên, hoặc join string như bạn làm ở dashboard)
            var room = sec.SectionSchedules
                .OrderBy(ss => GetDayOrder(ss.DayOfWeek))
                .ThenBy(ss => ss.StartPeriod)
                .FirstOrDefault()?.Room ?? "TBA";

            var scheduleText = sec.SectionSchedules.Any()
                ? string.Join("; ", sec.SectionSchedules
                    .OrderBy(ss => GetDayOrder(ss.DayOfWeek))
                    .ThenBy(ss => ss.StartPeriod)
                    .Select(ss => $"{GetDayNameVietnamese(ss.DayOfWeek)}, Tiết {ss.StartPeriod}-{ss.EndPeriod}"))
                : "Chưa có lịch";

            var students = await (from ss in _context.SectionStudents
                                  join st in _context.Students on ss.StudentId equals st.StudentId
                                  join u in _context.Users on st.StudentId equals u.UserId
                                  join g in _context.Grades.Where(g => g.SectionId == sectionId)
                                       on st.StudentId equals g.StudentId into gj
                                  from g in gj.DefaultIfEmpty()
                                  where ss.SectionId == sectionId && ss.Status == "ACTIVE"
                                  orderby u.FullName
                                  select new StudentGradeRowDto(
                                      st.StudentId,
                                      st.StudentCode,
                                      u.FullName,
                                      st.DateOfBirth,
                                      g.ProcessScore,
                                      g.FinalScore,
                                      g.TotalScore
                                  )).ToListAsync();

            return new ChiTietLopDto(
                sec.SectionId,
                sec.Course.CourseCode,
                sec.Course.CourseName,
                sec.Term.TermName,
                room,
                scheduleText,
                sec.Status,
                students
            );
        }

        public Task<int> GetAlertCountBySectionAsync(int sectionId)
        {
            return _context.Alerts
                .Where(a => a.SectionId == sectionId && a.Status != "CLOSED")
                .Select(a => a.StudentId)
                .Distinct()
                .CountAsync();
        }

        public async Task UpsertGradeAsync(int sectionId, int studentId, decimal? process, decimal? final, decimal? total)
        {
            var grade = await _context.Grades
                .SingleOrDefaultAsync(g => g.SectionId == sectionId && g.StudentId == studentId);

            if (grade == null)
            {
                grade = new Grade
                {
                    SectionId = sectionId,
                    StudentId = studentId,
                    ProcessScore = process,
                    FinalScore = final,
                    TotalScore = total
                };
                _context.Grades.Add(grade);
            }
            else
            {
                grade.ProcessScore = process;
                grade.FinalScore = final;
                grade.TotalScore = total;
            }

            await _context.SaveChangesAsync();
        }

        public async Task<GradeRule?> GetActiveGradeRuleBySectionAsync(int sectionId)
        {
            // Lấy rule theo CourseId của Section
            return await _context.GradeRules
                .Where(r => r.Active
                    && _context.Sections.Any(s => s.SectionId == sectionId && s.CourseId == r.CourseId))
                .OrderByDescending(r => r.RuleId) // nếu có nhiều rule active, lấy rule mới nhất
                .FirstOrDefaultAsync();
        }

        public async Task<List<SectionOptionDto>> GetSectionsForNotificationAsync(int teacherId)
        {
            return await _context.Sections
                .Include(s => s.Course)
                .Include(s => s.SectionStudents)
                .Where(s => s.TeacherId == teacherId && s.Status == "OPEN")
                .OrderByDescending(s => s.SectionId)
                .Select(s => new SectionOptionDto(
                    s.SectionId,
                    s.Course.CourseCode,
                    s.Course.CourseName,
                    s.SectionStudents.Count(ss => ss.Status == "ACTIVE")
                ))
                .ToListAsync();
        }

        public async Task<List<StudentNotificationDto>> GetStudentsWithAlertStatusAsync(int sectionId)
        {
            var students = await (from ss in _context.SectionStudents
                                  join st in _context.Students on ss.StudentId equals st.StudentId
                                  join u in _context.Users on st.StudentId equals u.UserId
                                  where ss.SectionId == sectionId && ss.Status == "ACTIVE"
                                  orderby u.FullName
                                  select new
                                  {
                                      st.StudentId,
                                      st.StudentCode,
                                      u.FullName,
                                      // Lấy cảnh báo mới nhất
                                      LatestAlert = _context.Alerts
                                          .Where(a => a.StudentId == st.StudentId &&
                                                     a.SectionId == sectionId &&
                                                     a.Status != "CLOSED")
                                          .OrderByDescending(a => a.Severity)
                                          .ThenByDescending(a => a.CreatedAt)
                                          .FirstOrDefault()
                                  }).ToListAsync();

            return students.Select(s => new StudentNotificationDto(
                s.StudentId,
                s.StudentCode,
                s.FullName,
                s.LatestAlert?.AlertType,
                s.LatestAlert?.Severity,
                s.LatestAlert?.ActualValue
            )).ToList();
        }

        public async Task<int> SendToSectionAsync(int sectionId, string title, string content)
        {
            // StudentId == UserId
            var studentUserIds = await _context.SectionStudents
                .Where(x => x.SectionId == sectionId)
                .Select(x => x.StudentId)
                .Distinct()
                .ToListAsync();

            if (studentUserIds.Count == 0) return 0;

            var now = DateTime.UtcNow;

            var notis = studentUserIds.Select(uid => new Notification
            {
                UserId = uid,
                Title = title,
                Content = content,
                RelatedAlertId = null,
                IsRead = false,
                CreatedAt = now
            }).ToList();

            _context.Notifications.AddRange(notis);
            await _context.SaveChangesAsync();
            return notis.Count;
        }

        public async Task SendToStudentAsync(int sectionId, int studentId, string title, string content)
        {
            // optional: verify student really belongs to section
            var isInSection = await _context.SectionStudents
                .AnyAsync(x => x.SectionId == sectionId && x.StudentId == studentId);

            if (!isInSection)
                throw new InvalidOperationException("Student không thuộc lớp này.");

            var noti = new Notification
            {
                UserId = studentId, // StudentId == UserId
                Title = title,
                Content = content,
                RelatedAlertId = null,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            _context.Notifications.Add(noti);
            await _context.SaveChangesAsync();
        }

    }
}