using SPTS_Service.ViewModel.QuantrivienVm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPTS_Service.Interface.Admin
{
    public interface IUserManagementService
    {
        Task<AdminUsersVM> GetUsersPageAsync(
        string? role,
        string? status,
        string? keyword,
        int page,
        int pageSize);

        Task<UserDetailVM?> GetUserDetailAsync(int userId);
        Task<bool> UpdateUserAsync(UserUpdateVM vm);
        Task<bool> DeleteUserAsync(int userId);

        Task<bool> LockUserAsync(int userId);
        Task<bool> UnlockUserAsync(int userId);

        Task<List<MajorOptionVM>> GetMajorsAsync();
        Task<List<DepartmentOptionVM>> GetDepartmentsAsync();
    }
}
