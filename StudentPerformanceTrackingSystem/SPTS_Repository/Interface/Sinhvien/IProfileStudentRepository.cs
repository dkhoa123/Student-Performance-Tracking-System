using SPTS_Repository.DTOs.Sinhvien;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPTS_Repository.Interface.Sinhvien
{
    public interface IProfileStudentRepository
    {
        Task<StudentIdentityDto> GetStudentIdentityAsync(int studentId);
        Task UpdateStudentAsync(StudentIdentityDto dto);
    }
}
