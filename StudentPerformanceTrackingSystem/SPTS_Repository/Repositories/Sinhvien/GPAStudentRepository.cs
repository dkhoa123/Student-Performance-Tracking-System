using Microsoft.EntityFrameworkCore;
using SPTS_Repository.DTOs.Sinhvien;
using SPTS_Repository.Entities;
using SPTS_Repository.Interface.Sinhvien;


namespace SPTS_Repository.Repositories.Sinhvien
{
    public class GPAStudentRepository : IGPAStudentRepository
    {
        private readonly SptsContext _db;
        public GPAStudentRepository(SptsContext db)
        {
            _db = db;
        }
        public async Task<int> GetCreditsEarnedCumulativeAsync(int studentId)
        {
            // Tích lũy: tất cả sections của mọi term, tính credits của môn đậu
            // Lưu ý: Grades unique (section_id, student_id) => không double count trong 1 section
            return await (from g in _db.Grades
                          join s in _db.Sections on g.SectionId equals s.SectionId
                          join c in _db.Courses on s.CourseId equals c.CourseId
                          where g.StudentId == studentId
                             && g.TotalScore != null
                             && g.TotalScore >= 5m
                          select c.Credits)
                         .SumAsync();
        }

        public async Task<CumulativeGpaDto?> GetCumulativeGpaAsync(int studentId)
        {
            // GPA tích lũy = SUM(gpa_point*credits)/SUM(credits) cho tất cả terms
            var rows = await (from g in _db.Grades
                              join s in _db.Sections on g.SectionId equals s.SectionId
                              join c in _db.Courses on s.CourseId equals c.CourseId
                              where g.StudentId == studentId && g.GpaPoint != null
                              select new { g.GpaPoint, c.Credits })
                             .ToListAsync();

            var credits = rows.Sum(x => x.Credits);
            if (credits <= 0) return new CumulativeGpaDto(null);

            var numerator = rows.Sum(x => x.GpaPoint!.Value * x.Credits);
            var gpa = Math.Round(numerator / credits, 2);

            return new CumulativeGpaDto(gpa);
        }

        public async Task<TermGpaDto?> GetTermGpaAsync(int studentId, int termId)
        {

            var rows = await (
                from g in _db.Grades
                join s in _db.Sections on g.SectionId equals s.SectionId
                join c in _db.Courses on s.CourseId equals c.CourseId
                from scale in _db.GpaScales
                    .Where(sc => g.TotalScore != null
                              && g.TotalScore >= sc.MinScore
                              && g.TotalScore <= sc.MaxScore)
                    .DefaultIfEmpty()
                where g.StudentId == studentId
                   && s.TermId == termId
                   && scale != null
                select new { scale!.GpaPoint, c.Credits, g.TotalScore }
            ).ToListAsync();

            var creditsAttempted = rows.Sum(x => x.Credits);
            if (creditsAttempted <= 0)
                return new TermGpaDto(null, 0, 0);

            var numerator = rows.Sum(x => x.GpaPoint * x.Credits);
            var gpa = Math.Round(numerator / creditsAttempted, 2);

            var creditsEarned = rows.Where(x => x.TotalScore != null && x.TotalScore >= 5m)
                                    .Sum(x => x.Credits);

            return new TermGpaDto(gpa, creditsAttempted, creditsEarned);
        }

        public async Task<List<TermGpaTrendRowDto>> GetTermGpaTrendAsync(int studentId, int take = 5)
        {
            var rows = await (
                from g in _db.Grades
                join s in _db.Sections on g.SectionId equals s.SectionId
                join c in _db.Courses on s.CourseId equals c.CourseId
                join t in _db.Terms on s.TermId equals t.TermId

                from scale in _db.GpaScales
                    .Where(sc => g.TotalScore != null
                              && g.TotalScore >= sc.MinScore
                              && g.TotalScore <= sc.MaxScore)
                    .DefaultIfEmpty()

                where g.StudentId == studentId
                      && scale != null
                // THAY ĐỔI: Thêm điều kiện g.TotalScore >= 5m trong trend
                //where g.StudentId == studentId
                //      && scale != null
                //      && g.TotalScore != null
                //      && g.TotalScore >= 5m  // CHỈ LẤY MÔN ĐẠT
                group new { c, t, scale } by new { t.TermId, t.TermName, t.StartDate } into grp
                select new
                {
                    grp.Key.TermId,
                    grp.Key.TermName,
                    grp.Key.StartDate,
                    Credits = grp.Sum(x => x.c.Credits),
                    Numerator = grp.Sum(x => x.scale!.GpaPoint * x.c.Credits)
                }
            )
            .OrderByDescending(x => x.StartDate)
            .Take(take)
            .ToListAsync();

            rows.Reverse();

            return rows.Select(x =>
                new TermGpaTrendRowDto(
                    x.TermId,
                    x.TermName,
                    x.StartDate.HasValue ? x.StartDate.Value.ToDateTime(TimeOnly.MinValue) : null,
                    x.Credits > 0 ? Math.Round(x.Numerator / x.Credits, 2) : null
                )
            ).ToList();
        }
    }
}
