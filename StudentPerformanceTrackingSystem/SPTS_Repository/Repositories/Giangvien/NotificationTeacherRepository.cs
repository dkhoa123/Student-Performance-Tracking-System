using Microsoft.EntityFrameworkCore;
using SPTS_Repository.DTOs.Giangvien;
using SPTS_Repository.Entities;
using SPTS_Repository.Interface.Giangvien;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPTS_Repository.Repositories.Giangvien
{
    public class NotificationTeacherRepository : INotificationTeacherRepository
    {
        private readonly SptsContext _context;
        public NotificationTeacherRepository(SptsContext context)
        {
            _context = context;
        }
        public async Task<List<SectionOptionDto>> GetSectionsForNotificationAsync(int teacherId)
        {
            return await _context.Sections
                .Include(s => s.Course)
                .Include(s => s.SectionStudents)
                .Where(s => s.TeacherId == teacherId && s.Status == "OPEN")
                .OrderByDescending(s => s.SectionId)
                .Select(s => new SectionOptionDto(
                    s.SectionId,
                    s.Course.CourseCode,
                    s.Course.CourseName,
                    s.SectionStudents.Count(ss => ss.Status == "ACTIVE")
                ))
                .ToListAsync();
        }

        public async Task<List<StudentNotificationDto>> GetStudentsWithAlertStatusAsync(int sectionId)
        {
            var students = await (from ss in _context.SectionStudents
                                  join st in _context.Students on ss.StudentId equals st.StudentId
                                  join u in _context.Users on st.StudentId equals u.UserId
                                  where ss.SectionId == sectionId && ss.Status == "ACTIVE"
                                  orderby u.FullName
                                  select new
                                  {
                                      st.StudentId,
                                      st.StudentCode,
                                      u.FullName,
                                      // Lấy cảnh báo mới nhất
                                      LatestAlert = _context.Alerts
                                          .Where(a => a.StudentId == st.StudentId &&
                                                     a.SectionId == sectionId &&
                                                     a.Status != "CLOSED")
                                          .OrderByDescending(a => a.Severity)
                                          .ThenByDescending(a => a.CreatedAt)
                                          .FirstOrDefault()
                                  }).ToListAsync();

            return students.Select(s => new StudentNotificationDto(
                s.StudentId,
                s.StudentCode,
                s.FullName,
                s.LatestAlert?.AlertType,
                s.LatestAlert?.Severity,
                s.LatestAlert?.ActualValue
            )).ToList();
        }

        public async Task<int> SendToSectionAsync(int sectionId, string title, string content)
        {
            // StudentId == UserId
            var studentUserIds = await _context.SectionStudents
                .Where(x => x.SectionId == sectionId)
                .Select(x => x.StudentId)
                .Distinct()
                .ToListAsync();

            if (studentUserIds.Count == 0) return 0;

            var now = DateTime.UtcNow;

            var notis = studentUserIds.Select(uid => new Notification
            {
                UserId = uid,
                Title = title,
                Content = content,
                RelatedAlertId = null,
                IsRead = false,
                CreatedAt = now
            }).ToList();

            _context.Notifications.AddRange(notis);
            await _context.SaveChangesAsync();
            return notis.Count;
        }

        public async Task SendToStudentAsync(int sectionId, int studentId, string title, string content)
        {
            // optional: verify student really belongs to section
            var isInSection = await _context.SectionStudents
                .AnyAsync(x => x.SectionId == sectionId && x.StudentId == studentId);

            if (!isInSection)
                throw new InvalidOperationException("Student không thuộc lớp này.");

            var noti = new Notification
            {
                UserId = studentId, // StudentId == UserId
                Title = title,
                Content = content,
                RelatedAlertId = null,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            _context.Notifications.Add(noti);
            await _context.SaveChangesAsync();
        }
    }
}
