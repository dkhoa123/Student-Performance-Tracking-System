using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SPTS_Repository.Interface;
using SPTS_Service.Interface;
using SPTS_Service.ViewModels;
using StudentPerformanceTrackingSystem.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace StudentPerformanceTrackingSystem.Controllers
{
    public class AdminController : Controller
    {
        private readonly IAdminService _adService;

        public AdminController(IAdminService adService)
        {
            _adService = adService;
        }

        [HttpGet]
        public async Task<IActionResult> DashboardAdmin(int? termId = null)
        {
            try
            {
                // ✅ Lấy terms từ database
                var termsVM = await _adService.GetTermsForDropdownAsync();

                // ✅ Xử lý termId
                // Nếu termId = 0 hoặc null → hiển thị tất cả
                // Nếu termId > 0 → hiển thị theo kỳ cụ thể
                int? selectedTermId = null;

                if (termId.HasValue && termId.Value > 0)
                {
                    selectedTermId = termId.Value;
                }
                // Nếu không truyền gì (lần đầu vào) → mặc định là "Tất cả"
                // selectedTermId sẽ là null

                // Convert sang SelectList
                ViewBag.Terms = new SelectList(
                    termsVM,
                    nameof(TermOptionVM.TermId),
                    nameof(TermOptionVM.TermName),
                    termId ?? 0  // Selected value: 0 = "Tất cả"
                );

                // ✅ Truyền null hoặc termId vào service
                var viewModel = await _adService.GetSystemStatistics(selectedTermId);

                // ✅ Truyền thông tin về term đang chọn vào ViewBag
                ViewBag.SelectedTermName = selectedTermId.HasValue
                    ? termsVM.FirstOrDefault(t => t.TermId == selectedTermId.Value)?.TermName
                    : "Tất cả học kỳ";

                return View(viewModel);
            }
            catch (Exception ex)
            {
                return View("~/Views/Shared/Error.cshtml", new ErrorViewModel
                {
                    RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
                    Message = ex.Message,
                    Detail = ex.ToString()
                });
            }
        }

        [HttpGet]
        public async Task<IActionResult> quanlyMonGV(int? termId = null, int page = 1, int pageSize = 10)
        {
            var vm = await _adService.GetCourseTeacherPageAsync(termId, page, pageSize);
            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> quanlyUser(string? role = null, string? status = null, string? keyword = null, int page = 1, int pageSize = 10)
        {
            var vm = await _adService.GetUsersPageAsync(role, status, keyword, page, pageSize);
            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> LockUser(int id)
        {
            await _adService.LockUserAsync(id);
            return RedirectToAction(nameof(quanlyUser));
        }

        [HttpPost]
        public async Task<IActionResult> UnlockUser(int id)
        {
            await _adService.UnlockUserAsync(id);
            return RedirectToAction(nameof(quanlyUser));
        }

        [HttpGet]
        public async Task<IActionResult> GetUserDetail(int id)
        {
            try
            {
                var user = await _adService.GetUserDetailAsync(id);
                if (user == null)
                    return NotFound();

                return Json(user);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateUser([FromBody] UserUpdateVM dto, string? returnRole = null, int page = 1)
        {
            try { 

                // Validate trước khi gọi service
                if (dto.Role == "STUDENT" && string.IsNullOrWhiteSpace(dto.StudentCode))
                {
                    return BadRequest(new { error = "Mã sinh viên không được để trống" });
                }

                if (dto.Role == "TEACHER" && string.IsNullOrWhiteSpace(dto.TeacherCode))
                {
                    return BadRequest(new { error = "Mã giảng viên không được để trống" });
                }

                var success = await _adService.UpdateUserAsync(dto);
                if (!success)
                    return BadRequest(new { error = "Cập nhật thất bại" });

                return Json(new { success = true, message = "Cập nhật thành công!" });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ERROR: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"STACK: {ex.StackTrace}");
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteUser(int id, string? returnRole = null, int page = 1)
        {
            try
            {
                // Prevent self-deletion
                var currentUserId = HttpContext.Session.GetInt32("UserId");
                if (currentUserId == id)
                {
                    TempData["Error"] = "Không thể xóa chính mình!";
                    return RedirectToAction(nameof(quanlyUser), new { role = returnRole, page });
                }

                var success = await _adService.DeleteUserAsync(id);
                if (!success)
                {
                    TempData["Error"] = "Không tìm thấy người dùng!";
                }
                else
                {
                    TempData["Success"] = "Xóa người dùng thành công!";
                }

                return RedirectToAction(nameof(quanlyUser), new { role = returnRole, page });
            }
            catch (Exception ex)
            {
                // Hiển thị lỗi chi tiết
                TempData["Error"] = ex.Message;
                return RedirectToAction(nameof(quanlyUser), new { role = returnRole, page });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetMajors()
        {
            try
            {
                var majors = await _adService.GetMajorsAsync();
                return Json(majors);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetDepartments()
        {
            try
            {
                var departments = await _adService.GetDepartmentsAsync();
                return Json(departments);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}