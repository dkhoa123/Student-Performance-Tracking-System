using SPTS_Repository.Entities;
using SPTS_Repository.Interface.Auth;
using SPTS_Service.Interface.Auth;
using SPTS_Service.ViewModel.AuthVm;

namespace SPTS_Service.Services.Auth
{
    public class AuthService : IAuthService
    {
        private readonly IAuthRepository _SVre;
        public AuthService(IAuthRepository SVre)
        {
            _SVre = SVre;
        }
        public async Task DangKysv(DangKySinhVien model)
        {
            if (model.Password != model.ConfirmPassword)
                throw new Exception("Mật khẩu xác nhận không khớp.");
            try
            {
                var email = model.Email.Trim().ToLower();
                var user = new User
                {
                    FullName = model.FullName,
                    Email = email,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password),
                    Role = "STUDENT",
                    Status = "ACTIVE",
                    CreatedAt = DateTime.UtcNow
                };

                var student = new Student
                {
                    StudentCode = await CreateStuCode(model.CohortYear),
                    Major = model.Major,
                    CohortYear = model.CohortYear,
                };
                await _SVre.DangKysv(user, student);
            }
            catch (Exception ex)
            {
                throw new Exception("Đăng ký sinh viên thất bại: " + ex.Message);
            }
        }
        //tạo mã sinh viên tự động
        public async Task<string?> CreateStuCode(int? CohortYear)
        {
            if (CohortYear < 0 || CohortYear > 99)
                throw new Exception("CohortYear phải nằm trong khoảng 0..99 (ví dụ: 23).");

            var year = DateTime.Now.Year.ToString(); // "2025"
            var prefix = $"{CohortYear}{year}";         // "232025"

            var maxCode = await _SVre.LayMaLonNhat(prefix);

            int nextSeq = 1;
            if (!string.IsNullOrWhiteSpace(maxCode))
            {
                // 5 số cuối: "00001"
                var last5 = maxCode.Substring(maxCode.Length - 5);
                if (int.TryParse(last5, out var lastSeq))
                    nextSeq = lastSeq + 1;
            }

            return $"{prefix}{nextSeq:D5}"; // "23202500001"
        }

        public async Task<User> DangNhap(string emailSv, string matKhau)
        {
            var user = await _SVre.TimEmail(emailSv);

            if (user == null)
                throw new Exception("Email hoặc mật khẩu không đúng.");

            if (user.Status != "ACTIVE")
                throw new Exception("Tài khoản đang bị khóa hoặc chưa kích hoạt.");

            if (!BCrypt.Net.BCrypt.Verify(matKhau, user.PasswordHash))
                throw new Exception("Email hoặc mật khẩu không đúng.");

            return user;
        }

        public async Task DoiMatKhauAsync(int userId, string oldPassword, string newPassword)
        {
            var user = await _SVre.FindUserByIdAsync(userId);
            if (user == null)
                throw new Exception("Không tìm thấy tài khoản.");

            if (user.Status != "ACTIVE")
                throw new Exception("Tài khoản đang bị khóa hoặc chưa kích hoạt.");

            if (!BCrypt.Net.BCrypt.Verify(oldPassword, user.PasswordHash))
                throw new Exception("Mật khẩu cũ không đúng.");

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            await _SVre.SaveChangesAsync();
        }
    }
}
