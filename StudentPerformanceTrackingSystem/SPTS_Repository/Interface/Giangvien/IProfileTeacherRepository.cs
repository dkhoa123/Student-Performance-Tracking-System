using SPTS_Repository.DTOs.Giangvien;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPTS_Repository.Interface.Giangvien   
{
    public interface IProfileTeacherRepository
    {
        Task<GiangVienProfileDto> GetProfileAsync(int teacherId);
        Task<string> GetTeacherUserAsync(int teacherId);
    }
}
