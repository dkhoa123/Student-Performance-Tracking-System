using Microsoft.EntityFrameworkCore;
using SPTS_Repository.DTOs.Sinhvien;
using SPTS_Repository.Entities;
using SPTS_Repository.Interface.Sinhvien;

namespace SPTS_Repository.Repositories.Sinhvien
{
    public class AlertStudentRepository : IAlertStudentRepository
    {
        private readonly SptsContext _db;
        public AlertStudentRepository(SptsContext db)
        {
            _db = db;
        }
        public Task<List<AlertDto>> GetAlertsAsync(int studentId, int termId, int take = 10)
        {
            return (from a in _db.Alerts
                    join sec in _db.Sections on a.SectionId equals sec.SectionId into secj
                    from sec in secj.DefaultIfEmpty()
                    join c in _db.Courses on sec.CourseId equals c.CourseId into cj
                    from c in cj.DefaultIfEmpty()
                    where a.StudentId == studentId
                && (
                a.TermId == termId
                || a.TermId == null && sec.TermId == termId
                )
                    orderby a.CreatedAt descending
                    select new AlertDto(
                        a.AlertId,
                        a.AlertType,
                        a.Severity,
                        c != null ? c.CourseCode : null,
                        a.Reason,
                        a.CreatedAt
                    )).Take(take)
                   .ToListAsync();

        }
    }
}
