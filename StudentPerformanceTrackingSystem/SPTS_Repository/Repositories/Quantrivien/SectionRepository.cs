using Microsoft.EntityFrameworkCore;
using SPTS_Repository.Entities;
using SPTS_Repository.Interface.Admin;
using SPTS_Shared.Constants;

namespace SPTS_Repository.Repositories.Admin
{
    public class SectionRepository : ISectionRepository
    {
        private readonly SptsContext _context;

        public SectionRepository(SptsContext context)
        {
            _context = context;
        }
        public async Task<bool> AssignTeacherToSectionAsync(int sectionId, int teacherId)
        {
            try
            {
                var section = await _context.Sections.FirstOrDefaultAsync(s => s.SectionId == sectionId);
                if (section == null) return false;

                // Kiểm tra teacher có tồn tại không
                var teacher = await _context.Teachers.FirstOrDefaultAsync(t => t.TeacherId == teacherId);
                if (teacher == null)
                    throw new Exception("Giảng viên không tồn tại");

                section.TeacherId = teacherId;
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                throw;
            }
        }

        public async Task<List<Teacher>> GetAvailableTeachersAsync(int? termId = null)
        {
            // ✅ Lấy TẤT CẢ giảng viên active
            var allTeachers = await _context.Teachers
                .AsNoTracking()
                .Include(t => t.TeacherNavigation)
                .Where(t => t.TeacherNavigation.Status == UserStatus.Active)
                .ToListAsync();

            return allTeachers;
        }

        // ✅ THÊM: Method mới để lấy số môn đang dạy của giảng viên
        public async Task<Dictionary<int, int>> GetTeacherSectionCountsAsync(int? termId = null)
        {
            var query = _context.Sections.AsNoTracking().AsQueryable();

            if (termId.HasValue)
                query = query.Where(s => s.TermId == termId.Value);

            var counts = await query
                .Where(s => s.TeacherId != null)
                .GroupBy(s => s.TeacherId)
                .Select(g => new { TeacherId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.TeacherId, x => x.Count);

            return counts;
        }

        public async Task<bool> UnassignTeacherFromSectionAsync(int sectionId)
        {
            try
            {
                var section = await _context.Sections.FirstOrDefaultAsync(s => s.SectionId == sectionId);
                if (section == null) return false;

                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                throw;
            }
        }
    }
}
