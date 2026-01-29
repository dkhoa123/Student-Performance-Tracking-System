using Microsoft.EntityFrameworkCore;
using SPTS_Repository.DTOs.Sinhvien;
using SPTS_Repository.Entities;
using SPTS_Repository.Interface.Sinhvien;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPTS_Repository.Repositories.Sinhvien
{
    public class CourseStudentRepository : ICourseStudentRepository
    {
        private readonly SptsContext _db;
        public CourseStudentRepository(SptsContext db)
        {
            _db = db;
        }
        
        public Task<List<CourseProgressDto>> GetCourseProgressAsync(int studentId, int termId)
        {
            return (from g in _db.Grades
                    join s in _db.Sections on g.SectionId equals s.SectionId
                    join c in _db.Courses on s.CourseId equals c.CourseId
                    join t in _db.Teachers on s.TeacherId equals t.TeacherId
                    join tu in _db.Users on t.TeacherId equals tu.UserId

                    from scale in _db.GpaScales
                        .Where(sc => g.TotalScore != null
                                  && g.TotalScore >= sc.MinScore
                                  && g.TotalScore <= sc.MaxScore)
                        .DefaultIfEmpty()

                    where g.StudentId == studentId && s.TermId == termId
                    select new CourseProgressDto(
                        c.CourseCode,
                        c.CourseName,
                        tu.FullName,
                        c.Credits,
                        g.ProcessScore,
                        g.FinalScore,
                        g.TotalScore,

                        // GPA hệ 4 lấy từ scale (đủ TotalScore là có)
                        scale != null ? scale.GpaPoint : null,

                        // Letter cũng từ scale
                        scale.Letter
                    )).ToListAsync();
        }
    }
}
