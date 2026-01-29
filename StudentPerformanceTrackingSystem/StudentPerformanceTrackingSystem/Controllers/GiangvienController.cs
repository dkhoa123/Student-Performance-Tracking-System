using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SPTS_Repository.Entities;
using SPTS_Service;
using SPTS_Service.Interface;
using SPTS_Service.Interface.Auth;
using SPTS_Service.Interface.Giangvien;
using SPTS_Service.ViewModel.AuthVm;
using SPTS_Service.ViewModel.GiangvienVm;
using System.Security.Claims;

namespace StudentPerformanceTrackingSystem.Controllers
{
    [Authorize(Roles = "TEACHER")]
    public class GiangvienController : Controller
    {
        private readonly IDashboardTeacherService _dashSer;
        private readonly IAuthService _auth;
        private readonly ISectionTeacherService _sectionSer;
        private readonly INotificationTeacherService _notiSer;
        private readonly IGradeTeacherService _gradeSer;
        private readonly IProfileTeacherService _profileSer;

        public GiangvienController(
            IDashboardTeacherService dashSer,
            IAuthService auth,
            ISectionTeacherService sectionSer,
            INotificationTeacherService notiSer,
            IGradeTeacherService gradeSer,
            IProfileTeacherService profileSer)
        {
            _dashSer = dashSer;
            _auth = auth;
            _sectionSer = sectionSer;
            _notiSer = notiSer;
            _gradeSer = gradeSer;
            _profileSer = profileSer;
        }

        public async Task<IActionResult> Index(int? termId)
        {
            var teacherId = GetCurrentTeacherId(); // Implement method này theo logic auth của bạn

            var model = await _dashSer.GetDashboardAsync(teacherId, termId);

            return View(model);
        }

        public async Task<IActionResult> Lophoc(int? termId)
        {
            var teacherId = GetCurrentTeacherId(); // Implement method này theo logic auth của bạn

            var model = await _dashSer.GetDashboardAsync(teacherId, termId);

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> ChitietLop(int id, int page = 1, string? search = null)
        {
            const int pageSize = 10;
            var vm = await _sectionSer.GetSectionDetailAsync(id, page, pageSize, search);
            return View(vm);
        }
        // GET: /Giangvien/ThongBao? sectionId=1&page=1
        public async Task<IActionResult> ThongBao(int sectionId, int page = 1)
        {
            var teacherId = GetCurrentTeacherId(); // Implement này theo cách bạn lưu session/claims

            if (sectionId == 0)
            {
                // Lấy lớp đầu tiên nếu chưa chọn
                var sections = await _notiSer.GetSectionsForNotificationAsync(teacherId);
                if (!sections.Any())
                {
                    TempData["Error"] = "Bạn chưa có lớp nào. ";
                    return RedirectToAction("Index");
                }
                sectionId = sections.First().SectionId;
            }

            var model = await _notiSer.GetThongBaoPageAsync(teacherId, sectionId, page);
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

            var sent = await _notiSer.SendToSectionAsync(model.SectionId, model.BroadcastTitle!, model.BroadcastContent!);
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

            await _notiSer.SendToStudentAsync(sectionId, studentId, title, content);
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
            await _gradeSer.SaveGradesAsync(model.SectionId, model.Students);

            TempData["Success"] = "Lưu bảng điểm thành công";
            return RedirectToAction(nameof(ChitietLop), new { id = model.SectionId, page = model.CurrentPage, search = model.Search });
        }

        public async Task<IActionResult> CaNhanGV()
        {
            int teacherId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
            var model = await _profileSer.GetProfileAsync(teacherId);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DoiMatKhau([FromBody] DoiMatKhauVm model)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { success = false, message = "Dữ liệu không hợp lệ." });

            if (model.NewPassword != model.ConfirmPassword)
                return BadRequest(new { success = false, message = "Mật khẩu xác nhận không khớp." });

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
            if (userId == 0)
                return Unauthorized(new { success = false, message = "Chưa đăng nhập." });

            try
            {
                await _auth.DoiMatKhauAsync(userId, model.OldPassword, model.NewPassword);
                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
    }
}
