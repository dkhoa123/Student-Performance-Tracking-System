using Microsoft.EntityFrameworkCore;
using SPTS_Repository.Entities;
using SPTS_Repository.Interface;

namespace SPTS_Repository
{
    public class SinhVienRepository : ISinhVienRepository
    {
        private readonly SptsContext _db;
        public SinhVienRepository(SptsContext context)
        {
            _db = context;
        }
        public Task<List<AlertDto>> GetAlertsAsync(int studentId, int termId, int take = 10)
        {
            return (from a in _db.Alerts
                    join sec in _db.Sections on a.SectionId equals sec.SectionId into secj
                    from sec in secj.DefaultIfEmpty()
                    join c in _db.Courses on sec.CourseId equals c.CourseId into cj
                    from c in cj.DefaultIfEmpty()
                    where a.StudentId == studentId
                && (
                a.TermId == termId
                || (a.TermId == null && sec.TermId == termId)
                )
                orderby a.CreatedAt descending
                select new AlertDto(
                    a.AlertId,
                    a.AlertType,
                    a.Severity,
                    c != null ? c.CourseCode : null,
                    a.Reason,
                    a.CreatedAt
                )).Take(take).ToListAsync();

        }

        public Task<List<CourseProgressDto>> GetCourseProgressAsync(int studentId, int termId)
        {
            return (from g in _db.Grades
                    join s in _db.Sections on g.SectionId equals s.SectionId
                    join c in _db.Courses on s.CourseId equals c.CourseId
                    join t in _db.Teachers on s.TeacherId equals t.TeacherId
                    join tu in _db.Users on t.TeacherId equals tu.UserId

                    from scale in _db.GpaScales
                        .Where(sc => g.TotalScore != null
                                  && g.TotalScore >= sc.MinScore
                                  && g.TotalScore <= sc.MaxScore)
                        .DefaultIfEmpty()

                    where g.StudentId == studentId && s.TermId == termId
                    select new CourseProgressDto(
                        c.CourseCode,
                        c.CourseName,
                        tu.FullName,
                        c.Credits,
                        g.ProcessScore,
                        g.FinalScore,
                        g.TotalScore,

                        // GPA hệ 4 lấy từ scale (đủ TotalScore là có)
                        scale != null ? (decimal?)scale.GpaPoint : null,

                        // Letter cũng từ scale
                        scale.Letter
                    )).ToListAsync();
        }

        public async Task<CumulativeGpaDto?> GetCumulativeGpaAsync(int studentId)
        {
            // GPA tích lũy = SUM(gpa_point*credits)/SUM(credits) cho tất cả terms
            var rows = await(from g in _db.Grades
                             join s in _db.Sections on g.SectionId equals s.SectionId
                             join c in _db.Courses on s.CourseId equals c.CourseId
                             where g.StudentId == studentId && g.GpaPoint != null
                             select new { g.GpaPoint, c.Credits })
                             .ToListAsync();

            var credits = rows.Sum(x => x.Credits);
            if (credits <= 0) return new CumulativeGpaDto(null);

            var numerator = rows.Sum(x => x.GpaPoint!.Value * x.Credits);
            var gpa = Math.Round(numerator / credits, 2);

            return new CumulativeGpaDto(gpa);
        }

        public Task<int> GetCurrentTermIdAsync()
        {
            return _db.Terms
                    .OrderByDescending(t => t.StartDate)
                    .Select(t => t.TermId)
                    .FirstOrDefaultAsync();
        }

        public async Task<StudentIdentityDto> GetStudentIdentityAsync(int studentId)
        {
            var x = await (from st in _db.Students
                           join u in _db.Users on st.StudentId equals u.UserId
                           where st.StudentId == studentId
                           select new {
                               st.StudentId,
                               u.UserId,
                               st.StudentCode,
                               u.FullName,
                               u.Email,
                               st.Major,
                               st.DateOfBirth,
                               st.Gender,
                               st.Phone,
                               st.Address,
                               u.Status
                          }).SingleOrDefaultAsync();
            if (x == null)
                throw new Exception($"Không tìm thấy Student/User với StudentId = {studentId}");
            return new StudentIdentityDto(x.StudentId,x.UserId, x.StudentCode,
                x.FullName, x.Email, x.Major, x.DateOfBirth, x.Gender, x.Phone, x.Address, x.Status);
        }

        public async Task<TermGpaDto?> GetTermGpaAsync(int studentId, int termId)
        {
            
            var rows = await (
                from g in _db.Grades
                join s in _db.Sections on g.SectionId equals s.SectionId
                join c in _db.Courses on s.CourseId equals c.CourseId
                from scale in _db.GpaScales
                    .Where(sc => g.TotalScore != null
                              && g.TotalScore >= sc.MinScore
                              && g.TotalScore <= sc.MaxScore)
                    .DefaultIfEmpty()
                where g.StudentId == studentId
                   && s.TermId == termId
                   && scale != null
                select new { GpaPoint = scale!.GpaPoint, Credits = c.Credits, TotalScore = g.TotalScore }
            ).ToListAsync();

            var creditsAttempted = rows.Sum(x => x.Credits);
            if (creditsAttempted <= 0)
                return new TermGpaDto(null, 0, 0);

            var numerator = rows.Sum(x => x.GpaPoint * x.Credits);
            var gpa = Math.Round(numerator / creditsAttempted, 2);

            var creditsEarned = rows.Where(x => x.TotalScore != null && x.TotalScore >= 5m)
                                    .Sum(x => x.Credits);

            return new TermGpaDto(gpa, creditsAttempted, creditsEarned);
        }

        public async Task<int> GetCreditsEarnedCumulativeAsync(int studentId)
        {
            // Tích lũy: tất cả sections của mọi term, tính credits của môn đậu
            // Lưu ý: Grades unique (section_id, student_id) => không double count trong 1 section
            return await(from g in _db.Grades
                         join s in _db.Sections on g.SectionId equals s.SectionId
                         join c in _db.Courses on s.CourseId equals c.CourseId
                         where g.StudentId == studentId
                            && g.TotalScore != null
                            && g.TotalScore >= 5m
                         select c.Credits)
                         .SumAsync();
        }

        public async Task<CurrentTermDto?> GetCurrentTermAsync()
        {
            return await _db.Terms
                .OrderByDescending(t => t.StartDate)
                .Select(t => new CurrentTermDto(t.TermId, t.TermName))
                .FirstOrDefaultAsync();
        }

        public Task<List<TermOptionDto>> GetTermsAsync()
        {
            return _db.Terms
            .OrderByDescending(t => t.StartDate)
            .Select(t => new TermOptionDto(t.TermId, t.TermName))
            .ToListAsync();
        }

        public async Task UpdateStudentAsync(StudentIdentityDto dto)
        {
            var student = await _db.Students.FindAsync(dto.StudentId);
            var user = await _db.Users.FindAsync(dto.UserId);

            if (student == null || user == null)
                throw new Exception("Không tìm thấy sinh viên.");

            user.FullName = dto.FullName;
            student.DateOfBirth = dto.DateOfBirth;
            student.Gender = dto.Gender;
            student.Phone = dto.Phone;
            student.Address = dto.Address;

            await _db.SaveChangesAsync();
        }
        public async Task<List<NotificationDto>> GetNotificationsAsync(int studentId, string filter, int skip, int take)
        {
            var query = _db.Notifications
                .Include(n => n.RelatedAlert)
                .Where(n => n.UserId == studentId);

            // Apply filter
            query = filter switch
            {
                "unread" => query.Where(n => !n.IsRead),
                "alert" => query.Where(n => n.RelatedAlertId != null),
                _ => query // "all"
            };

            return await query
                .OrderByDescending(n => n.CreatedAt)
                .Skip(skip)
                .Take(take)
                .Select(n => new NotificationDto(
                    n.NotificationId,
                    n.Title,
                    n.Content,
                    n.IsRead,
                    n.CreatedAt,
                    n.RelatedAlert != null ? n.RelatedAlert.AlertType : null,
                    n.RelatedAlert != null ? n.RelatedAlert.Severity : null
                ))
                .ToListAsync();
        }

        public async Task<int> GetNotificationCountAsync(int studentId, string filter)
        {
            var query = _db.Notifications.Where(n => n.UserId == studentId);

            query = filter switch
            {
                "unread" => query.Where(n => !n.IsRead),
                "alert" => query.Where(n => n.RelatedAlertId != null),
                _ => query
            };

            return await query.CountAsync();
        }

        public async Task<int> GetUnreadCountAsync(int studentId)
        {
            return await _db.Notifications
                .Where(n => n.UserId == studentId && !n.IsRead)
                .CountAsync();
        }

        public async Task MarkAsReadAsync(int notificationId, int studentId)
        {
            var noti = await _db.Notifications
                .FirstOrDefaultAsync(n => n.NotificationId == notificationId && n.UserId == studentId);

            if (noti != null && !noti.IsRead)
            {
                noti.IsRead = true;
                await _db.SaveChangesAsync();
            }
        }

        public async Task MarkAllAsReadAsync(int studentId)
        {
            await _db.Notifications
                .Where(n => n.UserId == studentId && !n.IsRead)
                .ExecuteUpdateAsync(setters => setters.SetProperty(n => n.IsRead, true));
        }

        public async Task<List<TermGpaTrendRowDto>> GetTermGpaTrendAsync(int studentId, int take = 5)
        {
            var rows = await (
                from g in _db.Grades
                join s in _db.Sections on g.SectionId equals s.SectionId
                join c in _db.Courses on s.CourseId equals c.CourseId
                join t in _db.Terms on s.TermId equals t.TermId

                from scale in _db.GpaScales
                    .Where(sc => g.TotalScore != null
                              && g.TotalScore >= sc.MinScore
                              && g.TotalScore <= sc.MaxScore)
                    .DefaultIfEmpty()

                where g.StudentId == studentId
                      && scale != null
                group new { c, t, scale } by new { t.TermId, t.TermName, t.StartDate } into grp
                select new
                {
                    grp.Key.TermId,
                    grp.Key.TermName,
                    grp.Key.StartDate,
                    Credits = grp.Sum(x => x.c.Credits),
                    Numerator = grp.Sum(x => x.scale!.GpaPoint * x.c.Credits)
                }
            )
            .OrderByDescending(x => x.StartDate)
            .Take(take)
            .ToListAsync();

            rows.Reverse();

            return rows.Select(x =>
                new TermGpaTrendRowDto(
                    x.TermId,
                    x.TermName,
                    x.StartDate.HasValue ? x.StartDate.Value.ToDateTime(TimeOnly.MinValue) : (DateTime?)null,
                    x.Credits > 0 ? Math.Round(x.Numerator / x.Credits, 2) : (decimal?)null
                )
            ).ToList();
        }
    }
}
