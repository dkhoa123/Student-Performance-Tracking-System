using SPTS_Repository.Entities;
using SPTS_Repository.Interface;
using SPTS_Service.Interface;
using StudentPerformanceTrackingSystem.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPTS_Service
{
    public class SinhVienService : ISinhVienService
    {
        private readonly ISinhVienRepository _SVre;
        public SinhVienService(ISinhVienRepository SVre)
        {
            _SVre = SVre;
        }
        public async Task DangKysv(DangKySinhVien model)
        {
            if (model.Password != model.ConfirmPassword)
                throw new Exception("Mật khẩu xác nhận không khớp.");
            try
            {
                var user = new User
                {
                    FullName = model.FullName,
                    Email = model.Email,
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
    }
}
