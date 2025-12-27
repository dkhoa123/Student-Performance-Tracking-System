using SPTS_Repository.Entities;
using SPTS_Repository.Interface;
using SPTS_Service.Interface;
using SPTS_Service.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPTS_Service
{
    public class SinhVienService : ISinhVienService
    {
        private readonly ISinhVienRepository _SVre;
        public SinhVienService(ISinhVienRepository SVre)
        {
            _SVre = SVre;
        }
        public async Task DangKysv(DangKySinhVien model)
        {
            if (model.Password != model.ConfirmPassword)
                throw new Exception("Mật khẩu xác nhận không khớp.");
            try
            {
                var email = model.Email.Trim().ToLower();
                var user = new User
                {
                    FullName = model.FullName,
                    Email = email,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password),
                    Role = "STUDENT",
                    Status = "ACTIVE",
                    CreatedAt = DateTime.UtcNow
                };

                var student = new Student
                {
                    StudentCode = await CreateStuCode(model.CohortYear),
                    Major = model.Major,
                    CohortYear = model.CohortYear,
                };
                await _SVre.DangKysv(user, student);
            }
            catch (Exception ex)
            {
                throw new Exception("Đăng ký sinh viên thất bại: " + ex.Message);
            }
        }
        //tạo mã sinh viên tự động
        public async Task<string?> CreateStuCode(int? CohortYear)
        {
            if (CohortYear < 0 || CohortYear > 99)
                throw new Exception("CohortYear phải nằm trong khoảng 0..99 (ví dụ: 23).");

            var year = DateTime.Now.Year.ToString(); // "2025"
            var prefix = $"{CohortYear}{year}";         // "232025"

            var maxCode = await _SVre.LayMaLonNhat(prefix);

            int nextSeq = 1;
            if (!string.IsNullOrWhiteSpace(maxCode))
            {
                // 5 số cuối: "00001"
                var last5 = maxCode.Substring(maxCode.Length - 5);
                if (int.TryParse(last5, out var lastSeq))
                    nextSeq = lastSeq + 1;
            }

            return $"{prefix}{nextSeq:D5}"; // "23202500001"
        }

        public async Task<User> DangNhap(string emailSv, string matKhau)
        {
            var user = await _SVre.TimEmail(emailSv);

            if (user == null)
                throw new Exception("Email hoặc mật khẩu không đúng.");

            if (user.Status != "ACTIVE")
                throw new Exception("Tài khoản đang bị khóa hoặc chưa kích hoạt.");

            if (!BCrypt.Net.BCrypt.Verify(matKhau, user.PasswordHash))
                throw new Exception("Email hoặc mật khẩu không đúng.");

            return user;
        }

        public async Task<SinhVien> GetDashboardAsync(int studentId, int? termId = null)
        {

            string? termName = null;

            if (termId == null)
            {
                var cur = await _SVre.GetCurrentTermAsync()
                          ?? throw new Exception("Không tìm thấy term hiện tại.");
                termId = cur.TermId;
                termName = cur.TermName;
            }
            else
            {
                // nếu bạn truyền termId từ ngoài vào, mà vẫn muốn termName
                // thì hoặc query thêm, hoặc bỏ trống
            }

            termId ??= await _SVre.GetCurrentTermIdAsync();

            var info = await _SVre.GetStudentIdentityAsync(studentId);
            var tg = await _SVre.GetTermGpaAsync(studentId, termId.Value);
            var courses = await _SVre.GetCourseProgressAsync(studentId, termId.Value);
            var alerts = await _SVre.GetAlertsAsync(studentId, termId.Value, take: 10);
            var cumulative = await _SVre.GetCumulativeGpaAsync(studentId);
            var creditsEarnedCumulative = await _SVre.GetCreditsEarnedCumulativeAsync(studentId);

            var dist = new GradeDistributionVm
            {
                A = courses.Count(x => x.GpaPoint == 4),
                B = courses.Count(x => x.GpaPoint == 3),
                C = courses.Count(x => x.GpaPoint == 2),
                DF = courses.Count(x => x.GpaPoint != null && x.GpaPoint <= 1)
            };

            return new SinhVien
            {
                UserId = info.StudentId,
                FullName = info.FullName,
                Email = info.Email,
                StudentCode = info.StudentCode,
                
                TermGpa = tg?.GpaValue,
                CreditsAttempted = tg?.CreditsAttempted ?? 0,
                CreditsEarned = creditsEarnedCumulative,

                CumulativeGpa = cumulative?.GpaValue,
                CurrentTermName = termName,

                CurrentCourses = courses.Select(x => new CourseProgressVm
                {
                    CourseCode = x.CourseCode,
                    CourseName = x.CourseName,
                    TeacherName = x.TeacherName,
                    TotalScore = x.TotalScore,
                    GpaPoint = x.GpaPoint
                }).ToList(),

                Alerts = alerts.Select(x => new AlertVm
                {
                    AlertId = x.AlertId,
                    AlertType = x.AlertType,
                    Severity = x.Severity,
                    CourseCode = x.CourseCode,
                    Reason = x.Reason,
                    CreatedAt = x.CreatedAt
                }).ToList(),

                AcademicAlertCount = alerts.Count,
                GradeDistribution = dist
            };
        }
    }
}
