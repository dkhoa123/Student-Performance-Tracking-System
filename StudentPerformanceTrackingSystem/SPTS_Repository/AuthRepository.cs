using Microsoft.EntityFrameworkCore;
using SPTS_Repository.Entities;
using SPTS_Repository.Interface;

namespace SPTS_Repository
{
    public class AuthRepository : IAuthRepository
    {
        private readonly SptsContext _db;
        public AuthRepository(SptsContext context)
        {
            _db = context;
        }
        public async Task DangKysv(User user, Student student)
        {
            await _db.Users.AddAsync(user);
            await _db.SaveChangesAsync();          // lúc này user.UserId đã có

            student.StudentId = user.UserId;       // giờ mới gán được
            await _db.Students.AddAsync(student);
            await _db.SaveChangesAsync();
        }
        public async Task<string?> LayMaLonNhat(string prefix)
        {
            return await _db.Students
                .Where(s => s.StudentCode != null && s.StudentCode.StartsWith(prefix))
                .OrderByDescending(s => s.StudentCode)
                .Select(s => s.StudentCode)
                .FirstOrDefaultAsync();
        }

        public async Task<User?> TimEmail(string email)
        {
            return await _db.Users
                .Include(u => u.Student)
                .FirstOrDefaultAsync(u => u.Email == email);
        }
    }
}
