using SPTS_Repository.DTOs.Giangvien;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPTS_Repository.Interface.Giangvien
{
    public interface IChartTeacherRepository
    {
        Task<List<ChartDataViewModelDto>> GetGpaChartDataByTeacherAsync(
        int teacherId, int? termId = null);

    }
}
