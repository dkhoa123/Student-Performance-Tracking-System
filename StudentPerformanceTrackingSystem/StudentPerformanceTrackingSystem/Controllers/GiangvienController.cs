using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SPTS_Service;
using SPTS_Service.Interface;
using System.Security.Claims;

namespace StudentPerformanceTrackingSystem.Controllers
{
    [Authorize(Roles = "TEACHER")]
    public class GiangvienController : Controller
    {
        private readonly IGiangvienService _gvSer;

        public GiangvienController(IGiangvienService giangvienService)
        {
            _gvSer = giangvienService;
        }

        public async Task<IActionResult> Index()
        {
            int teacherId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var model = await _gvSer.GetDashboardAsync(teacherId);

            return View(model);
        }
        
        public async Task<IActionResult> Lophoc()
        {
            int teacherId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var model = await _gvSer.GetDashboardAsync(teacherId);

            return View(model);
        }

        [Authorize(Roles = "TEACHER")]
        public async Task<IActionResult> ChitietLop(int id)
        {
            // id = SectionId
            var vm = await _gvSer.GetSectionDetailAsync(id);
            return View(vm);
        }
        [HttpPost]
        [Authorize(Roles = "TEACHER")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveGrades(SPTS_Service.ViewModel.ChiTietLopVm model)
        {
            await _gvSer.SaveGradesAsync(model.SectionId, model.Students);

            TempData["Success"] = "Lưu bảng điểm thành công";
            return RedirectToAction(nameof(ChitietLop), new { id = model.SectionId });
        }
        public IActionResult DiemSo()
        {
            return View();
        }
    }
}
