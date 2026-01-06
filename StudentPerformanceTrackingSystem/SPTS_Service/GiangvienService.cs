using SPTS_Repository.Entities;
using SPTS_Repository.Interface;
using SPTS_Service.Interface;
using SPTS_Service.ViewModel;

namespace SPTS_Service
{
    public class GiangvienService : IGiangvienService
    {
        private readonly IGiangvienRepository _repo;

        public GiangvienService(IGiangvienRepository repo)
        {
            _repo = repo;
        }

            public async Task<Giangvien> GetDashboardAsync(int teacherId)
            {
                // Stats
                var totalStudents = await _repo.GetTotalStudentsByTeacherAsync(teacherId);
                var averageScore = await _repo.GetAverageScoreByTeacherAsync(teacherId);
                var atRiskStudents = await _repo.GetAtRiskStudentsCountAsync(teacherId);
                var newStudents = await _repo.GetNewStudentsThisMonthAsync(teacherId);
                var activeSections = await _repo.GetActiveSectionsCountAsync(teacherId); // ✅ MỚI

                // Sections
                var sections = await _repo.GetSectionsByTeacherAsync(teacherId);

                // ✅ Nhóm sections theo học kỳ
                var sectionsByTerm = sections
                    .GroupBy(s => new { s.TermId, s.TermName, s.StartDate })
                    .OrderByDescending(g => g.Key.StartDate) // Học kỳ mới nhất trước
                    .Select(term => new TermSectionsViewModel
                    {
                        TermId = term.Key.TermId,
                        TermName = term.Key.TermName,
                        TermStartDate = term.Key.StartDate,
                        Sections = term.Select(s => new SectionCardViewModel
                        {
                            SectionId = s.SectionId,
                            CourseCode = s.CourseCode,
                            CourseName = s.CourseName,
                            Room = s.Room,
                            StudentCount = s.StudentCount,
                            CompletionRate = s.CompletionRate,
                            ColorClass = s.ColorClass,
                            Credits = s.Credits,
                            Status = s.Status,
                            Schedule = s.Schedule,
                            TimeSlot = s.TimeSlot,
                            AverageScore = s.AverageScore
                        }).ToList()
                    })
                    .ToList();

                var alerts = await _repo.GetRecentAlertsByTeacherAsync(teacherId);
                var chart = await _repo.GetGpaChartDataByTeacherAsync(teacherId);

                return new Giangvien
                {
                    TotalStudents = totalStudents,
                    AverageScore = averageScore,
                    AtRiskStudents = atRiskStudents,
                    NewStudentsThisMonth = newStudents,
                    ActiveSectionsCount = activeSections, // ✅ MỚI

                    SectionsByTerm = sectionsByTerm, // ✅ Nhóm theo term

                    RecentAlerts = alerts.Select(a => new AlertViewModel
                    {
                        StudentName = a.StudentName,
                        AlertType = a.AlertType,
                        Message = a.Message,
                        Severity = a.Severity,
                        IconName = a.IconName,
                        IconColor = a.IconColor,
                        CreatedAt = a.CreatedAt
                    }).ToList(),

                    ChartData = chart.Select(c => new ChartDataViewModel
                    {
                        TermId = c.TermId,
                        TermName = c.TermName,
                        AverageGpa = c.AverageGpa
                    }).ToList()
                };
            }

        public async Task<ChiTietLopVm> GetSectionDetailAsync(int sectionId)
        {
            var dto = await _repo.GetSectionDetailAsync(sectionId);
            var alertCount = await _repo.GetAlertCountBySectionAsync(sectionId);

            var vm = new ChiTietLopVm
            {
                SectionId = dto.SectionId,
                CourseCode = dto.CourseCode,
                CourseName = dto.CourseName,
                TermName = dto.TermName,
                Room = dto.Room,
                ScheduleText = dto.ScheduleText,
                SectionStatus = dto.SectionStatus,
                AlertCount = alertCount,

                Students = dto.Students.Select(s => new StudentGradeRowVm
                {
                    StudentId = s.StudentId,
                    StudentCode = s.StudentCode,
                    FullName = s.FullName,
                    DateOfBirth = s.DateOfBirth,
                    ProcessScore = s.ProcessScore,
                    FinalScore = s.FinalScore,
                    TotalScore = s.TotalScore
                }).ToList()
            };

            vm.StudentCount = vm.Students.Count;

            var graded = vm.Students.Where(x => x.TotalScore.HasValue).ToList();
            vm.AverageScore = graded.Any()
                ? Math.Round(graded.Average(x => x.TotalScore!.Value), 1)
                : (decimal?)null;

            // % qua môn: tính trên những SV đã có TotalScore (cách phổ biến)
            vm.PassRatePercent = graded.Any()
                ? Math.Round((decimal)graded.Count(x => x.TotalScore >= 5m) * 100 / graded.Count, 0)
                : 0;

            return vm;
        }

        public async Task SaveGradesAsync(int sectionId, List<StudentGradeRowVm> students)
        {
            var rule = await _repo.GetActiveGradeRuleBySectionAsync(sectionId);
            if (rule == null)
                throw new Exception("Môn học này chưa cấu hình tỉ trọng điểm (GradeRule).");

            foreach (var s in students)
            {
                // không nhập gì thì bỏ qua
                if (s.ProcessScore == null && s.FinalScore == null)
                    continue;

                decimal? total = null;

                // chỉ tính total khi có đủ 2 cột (tuỳ bạn muốn: thiếu 1 cột thì total null)
                if (s.ProcessScore.HasValue && s.FinalScore.HasValue)
                {
                    var raw = s.ProcessScore.Value * rule.ProcessWeight
                            + s.FinalScore.Value * rule.FinalWeight;

                    total = Math.Round(raw, rule.RoundingScale);
                }

                await _repo.UpsertGradeAsync(sectionId, s.StudentId, s.ProcessScore, s.FinalScore, total);
            }
        }
    }

}
