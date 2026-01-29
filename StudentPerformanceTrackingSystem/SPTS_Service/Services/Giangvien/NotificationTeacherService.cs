using SPTS_Repository.DTOs.Giangvien;
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
    public class NotificationTeacherService : INotificationTeacherService
    {
        private readonly INotificationTeacherRepository _repoNoti;
        private readonly ISectionTeacherRepository _repoSection;
        public NotificationTeacherService(
            INotificationTeacherRepository repoNoti,
            ISectionTeacherRepository repoSection)
        {
            _repoNoti = repoNoti;
            _repoSection = repoSection;
        }
       

        public async Task<ThongBaoPageVm> GetThongBaoPageAsync(int teacherId, int sectionId, int page = 1, int pageSize = 10)
        {
            // 1. Lấy danh sách lớp
            var sections = await _repoNoti.GetSectionsForNotificationAsync(teacherId);

            // 2. Lấy thông tin lớp hiện tại
            var currentSection = await _repoSection.GetSectionDetailAsync(sectionId);

            // 3. Lấy sinh viên + alert status
            var allStudents = await _repoNoti.GetStudentsWithAlertStatusAsync(sectionId);

            // 4. Phân trang
            var totalPages = (int)Math.Ceiling(allStudents.Count / (double)pageSize);
            var pagedStudents = allStudents
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return new ThongBaoPageVm
            {
                SectionId = sectionId,
                CourseCode = currentSection.CourseCode,
                CourseName = currentSection.CourseName,
                TermName = currentSection.TermName,
                StudentCount = allStudents.Count,

                AvailableSections = sections.Select(s => new SectionOptionVm
                {
                    SectionId = s.SectionId,
                    CourseCode = s.CourseCode,
                    CourseName = s.CourseName,
                    StudentCount = s.StudentCount,
                    IsSelected = s.SectionId == sectionId
                }).ToList(),

                Students = pagedStudents.Select(s => new StudentRowVm
                {
                    StudentId = s.StudentId,
                    StudentCode = s.StudentCode,
                    FullName = s.FullName,
                    StatusLabel = GetStatusLabel(s.AlertType, s.Severity),
                    StatusBadgeClass = GetStatusBadgeClass(s.Severity),
                    StatusIcon = GetStatusIcon(s.AlertType),
                    StatusDetail = GetStatusDetail(s.AlertType, s.ActualValue),
                    StatusColorClass = GetStatusColor(s.Severity)
                }).ToList(),

                CurrentPage = page,
                TotalPages = totalPages,
                PageSize = pageSize
            };
        }

        // Helper methods
        private static string GetStatusLabel(string? alertType, string? severity)
        {
            if (string.IsNullOrEmpty(alertType)) return "Bình thường";

            return alertType switch
            {
                "LOW_TOTAL" or "LOW_FINAL" or "LOW_GPA" => "Điểm kém",
                "ABSENT" => "Vắng nhiều",
                "MISSING_ASSIGNMENT" => "Thiếu bài tập",
                _ => "Cảnh báo"
            };
        }

        private static string GetStatusBadgeClass(string? severity)
        {
            return severity switch
            {
                "HIGH" => "bg-red-50 dark:bg-red-900/20 text-red-600 dark:text-red-400 border-red-100 dark:border-red-900/30",
                "MEDIUM" => "bg-orange-50 dark:bg-orange-900/20 text-orange-600 dark:text-orange-400 border-orange-100 dark:border-orange-900/30",
                "LOW" => "bg-yellow-50 dark:bg-yellow-900/20 text-yellow-600 dark:text-yellow-400 border-yellow-100 dark: border-yellow-900/30",
                _ => "bg-slate-100 dark:bg-slate-800 text-slate-600 dark: text-slate-400 border-slate-200 dark:border-slate-700"
            };
        }

        private static string GetStatusIcon(string? alertType)
        {
            return alertType switch
            {
                "LOW_TOTAL" or "LOW_FINAL" or "LOW_GPA" => "trending_down",
                "ABSENT" => "event_busy",
                "MISSING_ASSIGNMENT" => "assignment_late",
                _ => "remove"
            };
        }

        private static string GetStatusDetail(string? alertType, decimal? actualValue)
        {
            if (string.IsNullOrEmpty(alertType)) return "";

            return alertType switch
            {
                "LOW_TOTAL" or "LOW_FINAL" or "LOW_GPA" => $"TB:  {actualValue:F1}/10",
                "ABSENT" => $"Vắng:  {actualValue} buổi",
                _ => ""
            };
        }

        private static string GetStatusColor(string? severity)
        {
            return severity switch
            {
                "HIGH" => "bg-red-500",
                "MEDIUM" => "bg-orange-500",
                "LOW" => "bg-yellow-500",
                _ => "bg-emerald-500"
            };
        }

        public Task<int> SendToSectionAsync(int sectionId, string title, string content)
            => _repoNoti.SendToSectionAsync(sectionId, title, content);

        public Task SendToStudentAsync(int sectionId, int studentId, string title, string content)
            => _repoNoti.SendToStudentAsync(sectionId, studentId, title, content);

        public async Task<List<SectionOptionDto>> GetSectionsForNotificationAsync(int teacherId)
        {
            return await _repoNoti.GetSectionsForNotificationAsync(teacherId);
        }
        public async Task<List<StudentNotificationDto>> GetStudentsWithAlertStatusAsync(int sectionId)
        {
            return await _repoNoti.GetStudentsWithAlertStatusAsync(sectionId);
        }
    }
}
