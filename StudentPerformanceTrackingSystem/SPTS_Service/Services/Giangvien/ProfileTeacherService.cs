using SPTS_Repository.Interface.Giangvien;
using SPTS_Service.Interface.Giangvien;
using SPTS_Service.ViewModel.GiangvienVm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPTS_Service.Services.Giangvien
{
    public class ProfileTeacherService : IProfileTeacherService
    {
        private readonly IProfileTeacherRepository _repo;
        public ProfileTeacherService(IProfileTeacherRepository repo)
        {
            _repo = repo;
        }
        public async Task<GiangVienProfileVm> GetProfileAsync(int teacherId)
        {
            var dto = await _repo.GetProfileAsync(teacherId);

            return new GiangVienProfileVm
            {
                TeacherId = dto.TeacherId,
                FullName = dto.TeacherName,
                Email = dto.Email,
                Status = dto.Status,
                TeacherCode = dto.TeacherCode,
                Department = dto.Department,
                Degree = dto.Degree,
                Phone = dto.Phone,
                Birthday = dto.Birthday
            };
        }
    }
}
