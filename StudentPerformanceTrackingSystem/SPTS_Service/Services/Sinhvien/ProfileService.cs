using SPTS_Repository.DTOs.Sinhvien;
using SPTS_Repository.Interface.Sinhvien;
using SPTS_Service.Interface.Student;
using SPTS_Service.ViewModel.SinhvienVm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPTS_Service.Services.Sinhvien
{
    public class ProfileService : IProfileService
    {
        private readonly IProfileStudentRepository _profileRepo;
        public ProfileService(IProfileStudentRepository profileRepo)
        {
            _profileRepo = profileRepo;
        }
        public async Task CapNhatThongTinSinhVien(SinhVien model)
        {
            var dto = new StudentIdentityDto(
                model.StudentId,
                model.UserId,
                model.StudentCode,
                model.FullName,
                model.Email,
                model.Major,
                model.DateOfBirth,
                model.Gender,
                model.Phone,
                model.Address,
                model.Status
            );

            await _profileRepo.UpdateStudentAsync(dto);
        }
    }
}
