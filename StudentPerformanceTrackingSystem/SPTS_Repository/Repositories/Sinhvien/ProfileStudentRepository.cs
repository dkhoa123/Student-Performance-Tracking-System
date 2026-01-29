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
    public class ProfileStudentRepository : IProfileStudentRepository
    {
        private readonly SptsContext _db;
        public ProfileStudentRepository(SptsContext db)
        {
            _db = db;
        }
        public async Task<StudentIdentityDto> GetStudentIdentityAsync(int studentId)
        {
            var x = await (from st in _db.Students
                           join u in _db.Users on st.StudentId equals u.UserId
                           where st.StudentId == studentId
                           select new
                           {
                               st.StudentId,
                               u.UserId,
                               st.StudentCode,
                               u.FullName,
                               u.Email,
                               st.Major,
                               st.DateOfBirth,
                               st.Gender,
                               st.Phone,
                               st.Address,
                               u.Status
                           }).SingleOrDefaultAsync();
            if (x == null)
                throw new Exception($"Không tìm thấy Student/User với StudentId = {studentId}");
            return new StudentIdentityDto(x.StudentId, x.UserId, x.StudentCode,
                x.FullName, x.Email, x.Major, x.DateOfBirth, x.Gender, x.Phone, x.Address, x.Status);
        }

        public async Task UpdateStudentAsync(StudentIdentityDto dto)
        {
            var student = await _db.Students.FindAsync(dto.StudentId);
            var user = await _db.Users.FindAsync(dto.UserId);

            if (student == null || user == null)
                throw new Exception("Không tìm thấy sinh viên.");

            user.FullName = dto.FullName;
            student.DateOfBirth = dto.DateOfBirth;
            student.Gender = dto.Gender;
            student.Phone = dto.Phone;
            student.Address = dto.Address;

            await _db.SaveChangesAsync();
        }
    }
}
