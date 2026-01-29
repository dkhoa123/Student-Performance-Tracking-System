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
    public class ProfileTeacherRepository : IProfileTeacherRepository
    {
        private readonly SptsContext _context;
        public ProfileTeacherRepository(SptsContext context)
        {
            _context = context;
        }
        public async Task<GiangVienProfileDto> GetProfileAsync(int teacherId)
        {
            var row = await _context.Users
                .Where(u => u.UserId == teacherId)
                .Select(u => new
                {
                    u.UserId,
                    u.FullName,
                    u.Email,
                    u.Status,

                    u.Teacher.TeacherCode,
                    u.Teacher.Degree,
                    Department = u.Teacher.DemparmentName,
                    u.Teacher.Phone,
                    Birthday = u.Teacher.DateOfBirth
                })
                .FirstOrDefaultAsync();

            if (row == null) throw new Exception("Không tìm thấy giảng viên.");

            return new GiangVienProfileDto
            (
                row.UserId,
                row.FullName,
                row.Email,
                row.Status == "ACTIVE" ? "Tài khoản đang hoạt động" : "Tài khoản bị khóa",
                "Giảng viên",
                row.TeacherCode,
                row.Department ?? "",
                row.Degree ?? "",
                row.Phone ?? "",
                row.Birthday.HasValue
                    ? row.Birthday.Value.ToDateTime(TimeOnly.MinValue)
                    : DateTime.MinValue
            );
        }

        public async Task<string> GetTeacherUserAsync(int teacherId)
        {
            var user = await _context.Users
        .FirstOrDefaultAsync(u => u.UserId == teacherId);

            return user?.FullName ?? "Giảng viên";

        }
    }
}
