using SPTS_Repository.Interface.Sinhvien;
using SPTS_Service.Interface.Student;
using SPTS_Service.ViewModel.SinhvienVm;


namespace SPTS_Service.Services.Sinhvien
{
    public class DashboardStudentService : IDashboardStudentService
    {
        private readonly ITermStudentRepository _termRepo;
        private readonly IProfileStudentRepository _profileRepo;
        private readonly IGPAStudentRepository _gpaRepo;
        private readonly ICourseStudentRepository _courseRepo;
        private readonly IAlertStudentRepository _alertRepo;
        public DashboardStudentService(
            ITermStudentRepository termRepo,
            IProfileStudentRepository profileRepo,
            IGPAStudentRepository gpaRepo,
            ICourseStudentRepository courseRepo,
            IAlertStudentRepository alertRepo)
        {
            _termRepo = termRepo;
            _profileRepo = profileRepo;
            _gpaRepo = gpaRepo;
            _courseRepo = courseRepo;
            _alertRepo = alertRepo;
        }

        public async Task<SinhVien> GetDashboardAsync(int studentId, int? termId = null)
        {

            string? termName = null;

            if (termId == null)
            {
                var cur = await _termRepo.GetCurrentTermAsync()
                          ?? throw new Exception("Không tìm thấy term hiện tại.");
                termId = cur.TermId;
                termName = cur.TermName;
            }
            else
            {
                // nếu bạn truyền termId từ ngoài vào, mà vẫn muốn termName
                // thì hoặc query thêm, hoặc bỏ trống
            }

            var terms = await _termRepo.GetTermsAsync();

            termId ??= await _termRepo.GetCurrentTermIdAsync();

            var info = await _profileRepo.GetStudentIdentityAsync(studentId);
            var tg = await _gpaRepo.GetTermGpaAsync(studentId, termId.Value);
            var courses = await _courseRepo.GetCourseProgressAsync(studentId, termId.Value);
            var alerts = await _alertRepo.GetAlertsAsync(studentId, termId.Value, take: 10);
            var cumulative = await _gpaRepo.GetCumulativeGpaAsync(studentId);
            var creditsEarnedCumulative = await _gpaRepo.GetCreditsEarnedCumulativeAsync(studentId);

            var trend = await _gpaRepo.GetTermGpaTrendAsync(studentId, take: 5);

            var termCreditsAttempted = courses.Sum(x => x.Credit);
            var termCreditsEarned = courses.Where(x => x.TotalScore.HasValue && x.TotalScore.Value >= 5m)
                                           .Sum(x => x.Credit);


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
                CreditsAttempted = termCreditsAttempted,
                CreditsEarned = termCreditsEarned,
                CreditsEarnedCumulative = creditsEarnedCumulative,

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

                TermGpaTrend = trend.Select(x => new TermGpaTrendVm
                {
                    TermId = x.TermId,
                    TermName = x.TermName,
                    Gpa = x.GpaValue
                }).ToList(),

            };
        }
    }
}
