using SPTS_Repository.DTOs.Quantrivien;
using SPTS_Repository.Interface.Admin;
using SPTS_Service.Interface.Admin;
using SPTS_Service.ViewModel.QuantrivienVm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPTS_Service.Services.Quantrivien
{
    public class UserManagementService : IUserManagementService
    {
        private readonly IUserManagementRepository _usermanaRepo;
        public UserManagementService(IUserManagementRepository usermanaRepo)
        {
            _usermanaRepo = usermanaRepo;
        }
        public Task<bool> DeleteUserAsync(int userId)
        {
            return _usermanaRepo.DeleteUserAsync(userId);
        }

        public async Task<List<DepartmentOptionVM>> GetDepartmentsAsync()
        {
            var departments = await _usermanaRepo.GetDepartmentsAsync();
            return departments.Select(d => new DepartmentOptionVM
            {
                DepartmentId = d.DepartmentId,
                DepartmentName = d.DepartmentName ?? "",
                DepartmentCode = d.DepartmentCode ?? ""
            }).ToList();
        }

        public async Task<List<MajorOptionVM>> GetMajorsAsync()
        {
            var majors = await _usermanaRepo.GetMajorsAsync();
            return majors.Select(m => new MajorOptionVM
            {
                MajorCode = m,
                MajorName = m
            }).ToList();
        }

        public async Task<UserDetailVM?> GetUserDetailAsync(int userId)
        {
            var dto = await _usermanaRepo.GetUserDetailAsync(userId);
            if (dto == null) return null;

            return new UserDetailVM
            {
                UserId = dto.UserId,
                FullName = dto.FullName,
                Email = dto.Email,
                Role = dto.Role,
                Status = dto.Status,
                StudentCode = dto.StudentCode,
                TeacherCode = dto.TeacherCode
            };
        }

        public async Task<AdminUsersVM> GetUsersPageAsync(string? role, string? status, string? keyword, int page, int pageSize)
        {
            var (users, total) = await _usermanaRepo.GetUsersAsync(role, status, keyword, page, pageSize);

            return new AdminUsersVM
            {
                Role = role,
                Status = status,
                Keyword = keyword,
                Page = page <= 0 ? 1 : page,
                PageSize = pageSize <= 0 ? 10 : pageSize,
                TotalCount = total,
                Users = users.Select(u => new UserRowVM
                {
                    UserId = u.UserId,
                    FullName = u.FullName ?? "",
                    Email = u.Email ?? "",
                    Role = u.Role ?? "",
                    Status = u.Status ?? "",
                    Initials = BuildInitials(u.FullName)
                }).ToList()
            };
        }
        private static string BuildInitials(string? fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName)) return "?";
            var parts = fullName.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 1) return parts[0].Substring(0, 1).ToUpper();
            return (parts[0].Substring(0, 1) + parts[^1].Substring(0, 1)).ToUpper();
        }

        public Task<bool> LockUserAsync(int userId)
            => _usermanaRepo.SetUserStatusAsync(userId, "LOCKED");

        public Task<bool> UnlockUserAsync(int userId)
            => _usermanaRepo.SetUserStatusAsync(userId, "ACTIVE");

        public Task<bool> UpdateUserAsync(UserUpdateVM vm)
        {
            return _usermanaRepo.UpdateUserAsync(new UserUpdateDto
            {
                UserId = vm.UserId,
                FullName = vm.FullName,
                Email = vm.Email,
                Role = vm.Role,
                Status = vm.Status,
                StudentCode = vm.StudentCode,
                Major = vm.Major,                  // ✅ THÊM
                CohortYear = vm.CohortYear,        // ✅ THÊM
                DepartmentId = vm.DepartmentId,    // ✅ THÊM
                TeacherCode = vm.TeacherCode,
                Degree = vm.Degree,                // ✅ THÊM
                DepartmentName = vm.DepartmentName // ✅ THÊM
            });
        }
    }
}
