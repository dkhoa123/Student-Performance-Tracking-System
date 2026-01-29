using SPTS_Repository.Interface.Admin;
using SPTS_Repository.Interface.Giangvien;
using SPTS_Service.Interface.Giangvien;
using SPTS_Service.ViewModel.GiangvienVm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPTS_Service.Services.Giangvien
{
    public class DashboardTeacherService : IDashboardTeacherService
    {
        private readonly IDashboardTeacherRepository _repo;
        private readonly IProfileTeacherRepository _profileRepo;
        private readonly ITermTeacherRepository _termRepo;
        private readonly IAlertTeacherRepository _alertRepo;
        private readonly ISectionTeacherRepository _sectionRepo;
        private readonly IChartTeacherRepository _chartRepo;

        public DashboardTeacherService(
            IDashboardTeacherRepository repo, 
            IProfileTeacherRepository profileRepo,
            ITermTeacherRepository termRepo,
            IAlertTeacherRepository alertRepo,
            ISectionTeacherRepository teacherRepo,
            IChartTeacherRepository chartRepo)
        {
            _repo = repo;
            _profileRepo = profileRepo;
            _termRepo = termRepo;
            _alertRepo = alertRepo;
            _sectionRepo = teacherRepo;
            _chartRepo = chartRepo;
        }
        public async Task<GiangVien> GetDashboardAsync(int teacherId, int? termId = null)
        {
            var teacherName = await _profileRepo.GetTeacherUserAsync(teacherId);
            var terms = await _termRepo.GetTermsByTeacherAsync(teacherId);

            // term đang chọn: ưu tiên query param, fallback term mới nhất trong list
            var selectedTermId = termId ?? terms.FirstOrDefault().TermId;

            var totalStudents = await _repo.GetTotalStudentsByTeacherAsync(teacherId); // nếu muốn theo term thì cũng lọc term
            var averageScore = await _repo.GetAverageScoreByTeacherAsync(teacherId, selectedTermId);
            var atRiskStudents = await _alertRepo.GetAtRiskStudentsCountAsync(teacherId);
            var newStudents = await _repo.GetNewStudentsThisMonthAsync(teacherId);
            var activeSections = await _repo.GetActiveSectionsCountAsync(teacherId);
            var newAlertsCount = await _alertRepo.GetNewAlertsCountAsync(teacherId);

            var sections = await _sectionRepo.GetSectionsByTeacherAsync(teacherId);
            var sectionsByTerm = sections
                .GroupBy(s => new { s.TermId, s.TermName, s.StartDate })
                .OrderByDescending(g => g.Key.StartDate)
                .Select(termGroup => new TermSectionsVm
                {
                    TermId = termGroup.Key.TermId,
                    TermName = termGroup.Key.TermName,
                    TermStartDate = termGroup.Key.StartDate,
                    Sections = termGroup.Select(s => new SectionCardViewModel
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

            var alerts = await _alertRepo.GetRecentAlertsByTeacherAsync(teacherId);
            var chart = await _chartRepo.GetGpaChartDataByTeacherAsync(teacherId, selectedTermId); // nếu muốn chart cũng theo term

            return new GiangVien
            {
                TeacherName = teacherName,
                TotalStudents = totalStudents,
                AverageScore = averageScore,
                AtRiskStudents = atRiskStudents,
                NewStudentsThisMonth = newStudents,
                ActiveSectionsCount = activeSections,
                SectionsByTerm = sectionsByTerm,

                AvailableTerms = terms.Select(t => new TermOptionVm
                {
                    TermId = t.TermId,
                    TermName = t.TermName,
                    IsSelected = t.TermId == selectedTermId
                }).ToList(),
                SelectedTermId = selectedTermId,

                RecentAlerts = alerts.Select(a => new AlertViewModel
                {
                    StudentName = a.StudentName,
                    AlertType = a.AlertType,
                    Message = a.Message,
                    Severity = a.Severity,
                    IconName = a.IconName,
                    IconColor = a.IconColor,
                    CreatedAt = a.CreatedAt,
                    NewAlertsCount = newAlertsCount
                }).ToList(),

                ChartData = chart.Select(c => new ChartDataVm
                {
                    TermId = c.TermId,
                    TermName = c.TermName,
                    AverageGpa = c.AverageGpa
                }).ToList()
            };
        }
    }
}
