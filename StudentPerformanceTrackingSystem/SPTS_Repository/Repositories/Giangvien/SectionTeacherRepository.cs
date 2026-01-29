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
    public class SectionTeacherRepository : ISectionTeacherRepository
    {
        private readonly SptsContext _context;
        public SectionTeacherRepository(SptsContext context)
        {
            _context = context;
        }

        public Task<int> GetAlertCountBySectionAsync(int sectionId)
        {
            return _context.Alerts
                .Where(a => a.SectionId == sectionId && a.Status != "CLOSED")
                .Select(a => a.StudentId)
                .Distinct()
                .CountAsync();
        }

        public async Task<ChiTietLopDto> GetSectionDetailAsync(int sectionId)
        {
            var sec = await _context.Sections
                .Include(x => x.Course)
                .Include(x => x.Term)
                .Include(x => x.SectionSchedules)
                .SingleOrDefaultAsync(x => x.SectionId == sectionId);

            if (sec == null) throw new Exception("Không tìm thấy lớp.");

            // room + schedule (lấy cái đầu tiên, hoặc join string như bạn làm ở dashboard)
            var room = sec.SectionSchedules
                .OrderBy(ss => GetDayOrder(ss.DayOfWeek))
                .ThenBy(ss => ss.StartPeriod)
                .FirstOrDefault()?.Room ?? "TBA";

            var scheduleText = sec.SectionSchedules.Any()
                ? string.Join("; ", sec.SectionSchedules
                    .OrderBy(ss => GetDayOrder(ss.DayOfWeek))
                    .ThenBy(ss => ss.StartPeriod)
                    .Select(ss => $"{GetDayNameVietnamese(ss.DayOfWeek)}, Tiết {ss.StartPeriod}-{ss.EndPeriod}"))
                : "Chưa có lịch";

            var students = await (from ss in _context.SectionStudents
                                  join st in _context.Students on ss.StudentId equals st.StudentId
                                  join u in _context.Users on st.StudentId equals u.UserId
                                  join g in _context.Grades.Where(g => g.SectionId == sectionId)
                                       on st.StudentId equals g.StudentId into gj
                                  from g in gj.DefaultIfEmpty()
                                  where ss.SectionId == sectionId && ss.Status == "ACTIVE"
                                  orderby u.FullName
                                  select new StudentGradeRowDto(
                                      st.StudentId,
                                      st.StudentCode,
                                      u.FullName,
                                      st.DateOfBirth,
                                      g.ProcessScore,
                                      g.FinalScore,
                                      g.TotalScore
                                  )).ToListAsync();

            return new ChiTietLopDto(
                sec.SectionId,
                sec.Course.CourseCode,
                sec.Course.CourseName,
                sec.Term.TermName,
                room,
                scheduleText,
                sec.Status,
                students
            );
        }

        public async Task<List<SectionCardViewModelDto>> GetSectionsByTeacherAsync(int teacherId)
        {
            // Bước 1: Query data về memory
            var sections = await _context.Sections
                .Include(sec => sec.SectionSchedules)
                .Include(sec => sec.Course)
                .Include(sec => sec.Term)
                .Include(sec => sec.SectionStudents)
                .Include(sec => sec.Grades)
                .Where(sec => sec.TeacherId == teacherId)
                .ToListAsync();

            // Bước 2: Map sang DTO trong memory
            return sections.Select(sec => new SectionCardViewModelDto(
                sec.SectionId,
                sec.Course.CourseCode,
                sec.Course.CourseName,

                // Room
                sec.SectionSchedules
                    .OrderBy(ss => GetDayOrder(ss.DayOfWeek))
                    .ThenBy(ss => ss.StartPeriod)
                    .FirstOrDefault()?.Room ?? "TBA",

                // Student count
                sec.SectionStudents.Count(ss => ss.Status == "ACTIVE"),

                // Completion rate
                sec.SectionStudents.Any()
                    ? (decimal)sec.Grades.Count(g => g.TotalScore != null) * 100 / sec.SectionStudents.Count()
                    : 0,

                GetRandomColor(),
                sec.Course.Credits,
                sec.Status,

                // Schedule:  "Thứ 2, Tiết 1-3; Thứ 5, Tiết 4-6"
                string.Join("; ", sec.SectionSchedules
                    .OrderBy(ss => GetDayOrder(ss.DayOfWeek))
                    .ThenBy(ss => ss.StartPeriod)
                    .Select(ss =>
                        GetDayNameVietnamese(ss.DayOfWeek) +
                        ", Tiết " + ss.StartPeriod + "-" + ss.EndPeriod
                    )),

                // Time slot:  "07:00 - 09:30"
                // ✅ FIX: Sử dụng string interpolation thay vì ToString với format
                sec.SectionSchedules
                    .OrderBy(ss => GetDayOrder(ss.DayOfWeek))
                    .ThenBy(ss => ss.StartPeriod)
                    .Select(ss => $"{ss.StartTime:hh\\:mm} - {ss.EndTime:hh\\:mm}")
                    .FirstOrDefault() ?? "",

                // Average score
                sec.Grades.Any(g => g.TotalScore.HasValue)
                    ? sec.Grades.Where(g => g.TotalScore.HasValue)
                                .Average(g => g.TotalScore!.Value)
                    : null,

                sec.TermId,
                sec.Term.TermName,
                // ✅ FIX: Convert DateOnly to DateTime
                sec.Term.StartDate.HasValue
                    ? sec.Term.StartDate.Value.ToDateTime(TimeOnly.MinValue)
                    : null
            ))
            .OrderByDescending(s => s.StartDate)
            .ToList();
        }

        // ✅ Helper methods
        private static int GetDayOrder(string dayOfWeek)
        {
            return dayOfWeek switch
            {
                "MONDAY" => 1,
                "TUESDAY" => 2,
                "WEDNESDAY" => 3,
                "THURSDAY" => 4,
                "FRIDAY" => 5,
                "SATURDAY" => 6,
                "SUNDAY" => 7,
                _ => 8
            };
        }

        private static string GetDayNameVietnamese(string dayOfWeek)
        {
            return dayOfWeek switch
            {
                "MONDAY" => "Thứ 2",
                "TUESDAY" => "Thứ 3",
                "WEDNESDAY" => "Thứ 4",
                "THURSDAY" => "Thứ 5",
                "FRIDAY" => "Thứ 6",
                "SATURDAY" => "Thứ 7",
                "SUNDAY" => "CN",
                _ => ""
            };
        }

        // ✅ FIX: Xóa duplicate method, chỉ giữ 1 instance method
        private string GetRandomColor()
        {
            var colors = new[] { "purple", "orange", "blue", "green", "pink" };
            return colors[new Random().Next(colors.Length)];
        }

        // THÊM method đếm số lớp đang hoạt động

    }
}
