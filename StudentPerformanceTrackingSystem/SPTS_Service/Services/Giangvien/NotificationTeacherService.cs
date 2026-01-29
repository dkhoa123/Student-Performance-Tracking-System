// SPTS-Service/Services/Giangvien/NotificationTeacherService.cs
using SPTS_Repository.DTOs.Giangvien;
using SPTS_Repository.Interface.Giangvien;
using SPTS_Shared.Helpers;                    // Dùng helpers
using SPTS_Service.Interface.Domain;
using SPTS_Service.Interface.Giangvien;
using SPTS_Service.ViewModel.GiangvienVm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SPTS_Service.Services.Giangvien
{
    public class NotificationTeacherService : INotificationTeacherService
    {
        private readonly INotificationTeacherRepository _repoNoti;
        private readonly ISectionTeacherRepository _repoSection;
        private readonly INotificationDomainService _notificationService;  // ✅ Domain Service

        public NotificationTeacherService(
            INotificationTeacherRepository repoNoti,
            ISectionTeacherRepository repoSection,
            INotificationDomainService notificationService)
        {
            _repoNoti = repoNoti;
            _repoSection = repoSection;
            _notificationService = notificationService;
        }

        public async Task<ThongBaoPageVm> GetThongBaoPageAsync(
            int teacherId,
            int sectionId,
            int page = 1,
            int pageSize = 10)
        {
            var sections = await _repoNoti.GetSectionsForNotificationAsync(teacherId);
            var currentSection = await _repoSection.GetSectionDetailAsync(sectionId);
            var allStudents = await _repoNoti.GetStudentsWithAlertStatusAsync(sectionId);

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
                    // ✅ Dùng helpers thay vì helper methods riêng
                    StatusLabel = AlertDisplayHelper.GetStatusLabel(s.AlertType, s.Severity),
                    StatusBadgeClass = AlertDisplayHelper.GetStatusBadgeClass(s.Severity),
                    StatusIcon = AlertDisplayHelper.GetStatusIcon(s.AlertType),
                    StatusDetail = AlertDisplayHelper.GetStatusDetail(s.AlertType, s.ActualValue),
                    StatusColorClass = AlertDisplayHelper.GetStatusColor(s.Severity)
                }).ToList(),

                CurrentPage = page,
                TotalPages = totalPages,
                PageSize = pageSize
            };
        }

        // ✅ Delegate to Domain Service
        public Task<int> SendToSectionAsync(int sectionId, string title, string content)
            => _notificationService.SendToSectionAsync(sectionId, title, content);

        public Task SendToStudentAsync(int sectionId, int studentId, string title, string content)
            => _notificationService.SendToStudentAsync(studentId, title, content);

        public Task<List<SectionOptionDto>> GetSectionsForNotificationAsync(int teacherId)
            => _repoNoti.GetSectionsForNotificationAsync(teacherId);

        public Task<List<StudentNotificationDto>> GetStudentsWithAlertStatusAsync(int sectionId)
            => _repoNoti.GetStudentsWithAlertStatusAsync(sectionId);

        // ❌ XÓA TẤT CẢ helper methods (đã chuyển sang AlertDisplayHelper)
    }
}