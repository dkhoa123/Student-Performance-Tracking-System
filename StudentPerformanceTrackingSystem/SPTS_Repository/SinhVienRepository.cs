using Microsoft.EntityFrameworkCore;
using SPTS_Repository.Entities;
using SPTS_Repository.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPTS_Repository
{
    public class SinhVienRepository : ISinhVienRepository
    {
        private readonly SptsContext _db;
        public SinhVienRepository(SptsContext context)
        {
            _db = context;
        }
        public async Task DangKysv(User user, Student student)
        {
            await _db.Users.AddAsync(user);
            await _db.SaveChangesAsync();          // lúc này user.UserId đã có

            student.StudentId = user.UserId;       // giờ mới gán được
            await _db.Students.AddAsync(student);
            await _db.SaveChangesAsync();
        }
        public async Task<string?> LayMaLonNhat(string prefix)
        {
            return await _db.Students
                .Where(s => s.StudentCode != null && s.StudentCode.StartsWith(prefix))
                .OrderByDescending(s => s.StudentCode)
                .Select(s => s.StudentCode)
                .FirstOrDefaultAsync();
        }

        public async Task<User?> TimEmail(string email)
        {
            return await _db.Users
                .Include(u => u.Student)
                .FirstOrDefaultAsync(u => u.Email == email);
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
                    g.GpaPoint,
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
                               st.StudentCode,
                               u.FullName,
                               u.Email,
                               st.Major,
                          }).SingleAsync();
            return new StudentIdentityDto(x.StudentId, x.StudentCode, x.FullName, x.Email, x.Major);
        }

        public Task<TermGpaDto?> GetTermGpaAsync(int studentId, int termId)
        {
            return _db.TermGpas
                .Where(x => x.StudentId == studentId && x.TermId == termId)
                .Select(x => new TermGpaDto(x.GpaValue, x.CreditsAttempted, x.CreditsEarned))
                .FirstOrDefaultAsync();
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
    }
}
