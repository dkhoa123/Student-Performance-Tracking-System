using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPTS_Repository.DTOs.Giangvien
{
    public record ChartDataViewModelDto(
        int TermId, 
        string TermName, 
        decimal AverageGpa);
}
