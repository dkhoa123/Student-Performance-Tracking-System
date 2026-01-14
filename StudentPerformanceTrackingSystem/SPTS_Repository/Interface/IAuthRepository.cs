using SPTS_Repository.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPTS_Repository.Interface
{
    public interface IAuthRepository 
    {
        Task DangKysv(User user, Student student);
        Task<string?> LayMaLonNhat(string prefix);
        Task<User?> TimEmail(string email);
    }
}
