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
        }

}
