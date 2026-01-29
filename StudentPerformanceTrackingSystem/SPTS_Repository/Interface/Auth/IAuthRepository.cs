using SPTS_Repository.Entities;

namespace SPTS_Repository.Interface.Auth
{
    public interface IAuthRepository 
    {
        Task DangKysv(User user, Student student);
        Task<string?> LayMaLonNhat(string prefix);
        Task<User?> TimEmail(string email);

        Task<User?> FindUserByIdAsync(int userId);
        Task SaveChangesAsync();
    }
}
