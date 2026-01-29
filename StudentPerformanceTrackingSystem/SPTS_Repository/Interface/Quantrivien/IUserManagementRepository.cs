using SPTS_Repository.DTOs.Quantrivien;
using SPTS_Repository.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPTS_Repository.Interface.Admin
{
    public interface IUserManagementRepository
    {
        // User Management
        Task<(List<User> Users, int TotalCount)> GetUsersAsync(
            string? role,
            string? status,
            string? keyword,
            int page,
            int pageSize);

        Task<User?> GetUserByIdAsync(int userId);
        // Update user 
        Task<UserDetailDto?> GetUserDetailAsync(int userId);
        Task<bool> UpdateUserAsync(UserUpdateDto dto);
        Task<bool> DeleteUserAsync(int userId);
        Task<bool> SetUserStatusAsync(int userId, string newStatus); // ACTIVE/LOCKED...

        Task<List<Department>> GetDepartmentsAsync();
        Task<List<string>> GetMajorsAsync();
    }
}
