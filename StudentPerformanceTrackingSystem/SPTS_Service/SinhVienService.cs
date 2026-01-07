using SPTS_Repository.Entities;
using SPTS_Repository.Interface;
using SPTS_Service.Interface;
using SPTS_Service.ViewModel;

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

            var terms = await _SVre.GetTermsAsync();

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
                StudentId = info.StudentId,
                UserId = info.UserId,
                FullName = info.FullName,
                Email = info.Email,
                StudentCode = info.StudentCode,
                Major = info.Major,
                DateOfBirth = info.DateOfBirth,
                Gender = info.Gender,
                Phone = info.Phone,
                Address = info.Address,
                Status = info.status,

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
                    Credit = x.Credit,
                    ProcessScore = x.ProcessScore,
                    FinalScore = x.FinalScore,
                    TotalScore = x.TotalScore,
                    GpaPoint = x.GpaPoint,
                    Letter = x.Letter
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
                GradeDistribution = dist,

                Terms = terms.Select(t => new TermOptionVm
                {
                    TermId = t.TermId,
                    TermName = t.TermName
                }).ToList(),

                SelectedTermId = termId,
            };


        }

        public async Task CapNhatThongTinSinhVien(SinhVien model)
        {
            var dto = new StudentIdentityDto(
                model.StudentId,
                model.UserId,
                model.StudentCode,
                model.FullName,
                model.Email,
                model.Major,
                model.DateOfBirth,
                model.Gender,
                model.Phone,
                model.Address,
                model.Status
            );

            await _SVre.UpdateStudentAsync(dto);
        }


        public async Task<ThongBaoSinhVienVm> GetNotificationsPageAsync(int studentId, string filter = "all", int page = 1, int pageSize = 10)
        {
            var skip = (page - 1) * pageSize;

            var notifications = await _SVre.GetNotificationsAsync(studentId, filter, skip, pageSize);
            var totalCount = await _SVre.GetNotificationCountAsync(studentId, filter);
            var unreadCount = await _SVre.GetUnreadCountAsync(studentId);

            return new ThongBaoSinhVienVm
            {
                Notifications = notifications.Select(n => new NotificationItemVm
                {
                    NotificationId = n.NotificationId,
                    Title = n.Title,
                    Content = n.Content,
                    IsRead = n.IsRead,
                    CreatedAt = n.CreatedAt,
                    AlertType = n.AlertType,
                    Severity = n.Severity,
                    IconName = GetIconName(n.AlertType),
                    IconColor = GetIconColor(n.Severity),
                    BorderColor = GetBorderColor(n.Severity),
                    BadgeText = GetBadgeText(n.AlertType, n.Severity),
                    BadgeClass = GetBadgeClass(n.Severity),
                    TimeAgo = GetTimeAgo(n.CreatedAt)
                }).ToList(),

                TotalCount = totalCount,
                UnreadCount = unreadCount,
                CurrentFilter = filter,
                CurrentPage = page,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
                PageSize = pageSize
            };
        }

        public Task MarkAsReadAsync(int notificationId, int studentId)
            => _SVre.MarkAsReadAsync(notificationId, studentId);

        public Task MarkAllAsReadAsync(int studentId)
            => _SVre.MarkAllAsReadAsync(studentId);

        // Helper methods
        private static string GetIconName(string? alertType)
        {
            return alertType switch
            {
                "LOW_TOTAL" or "LOW_FINAL" or "LOW_GPA" => "warning",
                "ABSENT" => "school",
                "MISSING_ASSIGNMENT" => "assignment_late",
                _ => "notifications"
            };
        }

        private static string GetIconColor(string? severity)
        {
            return severity switch
            {
                "HIGH" => "bg-red-100 dark:bg-red-900/30 text-red-600 dark:text-red-400",
                "MEDIUM" => "bg-orange-100 dark:bg-orange-900/30 text-orange-600 dark:text-orange-400",
                "LOW" => "bg-yellow-100 dark:bg-yellow-900/30 text-yellow-600 dark:text-yellow-400",
                _ => "bg-blue-50 dark:bg-blue-900/20 text-blue-600 dark:text-blue-400"
            };
        }

        private static string GetBorderColor(string? severity)
        {
            return severity switch
            {
                "HIGH" => "border-l-red-500",
                "MEDIUM" => "border-l-orange-500",
                "LOW" => "border-l-yellow-500",
                _ => ""
            };
        }

        private static string GetBadgeText(string? alertType, string? severity)
        {
            if (string.IsNullOrEmpty(severity)) return "Thông báo chung";

            return severity switch
            {
                "HIGH" => "Nghiêm trọng",
                "MEDIUM" => "Cần chú ý",
                "LOW" => "Lưu ý",
                _ => "Thông báo"
            };
        }

        private static string GetBadgeClass(string? severity)
        {
            return severity switch
            {
                "HIGH" => "bg-red-50 text-red-700 ring-1 ring-inset ring-red-600/10 dark:bg-red-900/30 dark:text-red-400 dark:ring-red-500/20",
                "MEDIUM" => "bg-orange-50 text-orange-700 ring-1 ring-inset ring-orange-600/10 dark: bg-orange-900/30 dark:text-orange-400 dark:ring-orange-500/20",
                "LOW" => "bg-yellow-50 text-yellow-700 ring-1 ring-inset ring-yellow-600/10 dark:bg-yellow-900/30 dark:text-yellow-400 dark:ring-yellow-500/20",
                _ => "bg-[#f0f2f4] text-[#617589] dark:bg-[#374151] dark:text-[#9ca3af]"
            };
        }

        private static string GetTimeAgo(DateTime createdAt)
        {
            var diff = DateTime.UtcNow - createdAt;

            if (diff.TotalMinutes < 1) return "Vừa xong";
            if (diff.TotalMinutes < 60) return $"{(int)diff.TotalMinutes} phút trước";
            if (diff.TotalHours < 24) return $"{(int)diff.TotalHours} giờ trước";
            if (diff.TotalDays < 7) return $"{(int)diff.TotalDays} ngày trước";

            return createdAt.ToString("dd 'Th'MM");
        }
    }
}
