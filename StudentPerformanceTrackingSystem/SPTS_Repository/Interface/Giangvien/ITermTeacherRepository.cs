using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPTS_Repository.Interface.Giangvien
{
    public interface ITermTeacherRepository
    {
        Task<List<(int TermId, string TermName)>> GetTermsByTeacherAsync(int teacherId);
        Task<int> GetTermIdBySectionAsync(int sectionId);
    }
}
