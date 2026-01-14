using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SPTS_Service.Interface;
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
                var terms = GetTermsForDropdown();
                ViewBag.Terms = new SelectList(terms, "Value", "Text", termId);

                var viewModel = await _adService.GetSystemStatistics(termId);
                return View(viewModel);
            }
            catch (Exception ex)
            {
                // Trả về Error view có model => không NullReference nữa
                return View("~/Views/Shared/Error.cshtml", new ErrorViewModel
                {
                    RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
                    Message = ex.Message,
                    Detail = ex.ToString()
                });
            }
        }

        private List<SelectListItem> GetTermsForDropdown()
        {
            return new List<SelectListItem>
            {
                new SelectListItem { Value = "", Text = "-- Tất cả học kỳ --" },
                new SelectListItem { Value = "3", Text = "HK1 2022-2023" },
                new SelectListItem { Value = "4", Text = "HK2 2022-2023" },
                new SelectListItem { Value = "5", Text = "HK1 2023-2024" },
                new SelectListItem { Value = "6", Text = "HK2 2023-2024" },
                new SelectListItem { Value = "7", Text = "HK1 2024-2025" }
            };
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
    }
}