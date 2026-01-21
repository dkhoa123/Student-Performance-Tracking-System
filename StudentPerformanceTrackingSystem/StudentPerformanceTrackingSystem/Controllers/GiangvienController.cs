using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SPTS_Service;
using SPTS_Service.Interface;
using SPTS_Service.ViewModel;
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

        public async Task<IActionResult> Index(int? termId)
        {
            int teacherId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var model = await _gvSer.GetDashboardAsync(teacherId, termId);
            return View(model);
        }

        public async Task<IActionResult> Lophoc()
        {
            int teacherId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var model = await _gvSer.GetDashboardAsync(teacherId);

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> ChitietLop(int id, int page = 1, string? search = null)
        {
            const int pageSize = 10;
            var vm = await _gvSer.GetSectionDetailAsync(id, page, pageSize, search);
            return View(vm);
        }
        // GET: /Giangvien/ThongBao? sectionId=1&page=1
        public async Task<IActionResult> ThongBao(int sectionId, int page = 1)
        {
            var teacherId = GetCurrentTeacherId(); // Implement này theo cách bạn lưu session/claims

            if (sectionId == 0)
            {
                // Lấy lớp đầu tiên nếu chưa chọn
                var sections = await _gvSer.GetSectionsForNotificationAsync(teacherId);
                if (!sections.Any())
                {
                    TempData["Error"] = "Bạn chưa có lớp nào. ";
                    return RedirectToAction("Index");
                }
                sectionId = sections.First().SectionId;
            }

            var model = await _gvSer.GetThongBaoPageAsync(teacherId, sectionId, page);
            return View(model);
        }

        // POST:  Gửi broadcast
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendBroadcast(ThongBaoPageVm model)
        {
            if (string.IsNullOrWhiteSpace(model.BroadcastTitle) || string.IsNullOrWhiteSpace(model.BroadcastContent))
            {
                TempData["Error"] = "Vui lòng nhập tiêu đề và nội dung. ";
                return RedirectToAction(nameof(ThongBao), new { sectionId = model.SectionId });
            }

            var sent = await _gvSer.SendToSectionAsync(model.SectionId, model.BroadcastTitle!, model.BroadcastContent!);
            TempData["Success"] = $"✅ Đã gửi {sent} thông báo thành công! ";

            return RedirectToAction(nameof(ThongBao), new { sectionId = model.SectionId });
        }

        // POST:  Gửi riêng
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendPrivate(int sectionId, int studentId, string title, string content)
        {
            if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(content))
            {
                TempData["Error"] = "Vui lòng nhập tiêu đề và nội dung.";
                return RedirectToAction(nameof(ThongBao), new { sectionId });
            }

            await _gvSer.SendToStudentAsync(sectionId, studentId, title, content);
            TempData["Success"] = "✅ Đã gửi thông báo riêng! ";

            return RedirectToAction(nameof(ThongBao), new { sectionId });
        }

        private int GetCurrentTeacherId()
        {
            return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
        }

        [HttpPost]
        [Authorize(Roles = "TEACHER")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveGrades(ChiTietLopVm model)
        {
            await _gvSer.SaveGradesAsync(model.SectionId, model.Students);

            TempData["Success"] = "Lưu bảng điểm thành công";
            return RedirectToAction(nameof(ChitietLop), new { id = model.SectionId, page = model.CurrentPage, search = model.Search });
        }
    }
}
