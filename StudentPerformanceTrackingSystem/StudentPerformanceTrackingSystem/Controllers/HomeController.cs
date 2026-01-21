using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SPTS_Service.Interface;
using SPTS_Service.ViewModel;
using StudentPerformanceTrackingSystem.Models;
using System.Diagnostics;
using System.Security.Claims;

namespace StudentPerformanceTrackingSystem.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IAuthService _svAuth;
        private readonly ISinhVienService _svSer;

        public HomeController(ILogger<HomeController> logger, ISinhVienService svSer, IAuthService svAuth)
        {
            _logger = logger;
            _svSer = svSer;
            _svAuth = svAuth;
        }
        [Authorize(Roles = "STUDENT")]
        public async Task<IActionResult> Index()
        {
            var studentIdClaim = User.FindFirstValue("StudentId");
            if (string.IsNullOrWhiteSpace(studentIdClaim))
                return Forbid();

            int studentId = int.Parse(studentIdClaim);

            var vm = await _svSer.GetDashboardAsync(studentId);
            return View(vm);
        }
        [Authorize(Roles = "STUDENT")]
        public async Task<IActionResult> BangDiemSinhVien(int? termId)
        {
            var studentIdClaim = User.FindFirstValue("StudentId");
            if (string.IsNullOrWhiteSpace(studentIdClaim))
                return Forbid();

            int studentId = int.Parse(studentIdClaim);

            var vm = await _svSer.GetDashboardAsync(studentId, termId);
            return View(vm);
        }
        [HttpGet]
        [Authorize(Roles = "STUDENT")]
        public async Task<IActionResult> CaNhan()
        {
            var studentIdClaim = User.FindFirstValue("StudentId");
            if (string.IsNullOrWhiteSpace(studentIdClaim))
                return Forbid();

            int studentId = int.Parse(studentIdClaim);

            var vm = await _svSer.GetDashboardAsync(studentId);
            return View(vm);
        }


        // POST: lưu chỉnh sửa
        [HttpPost]
        [Authorize(Roles = "STUDENT")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CaNhan(SinhVien model)
        {
            if (!ModelState.IsValid)
                return View(model);

            await _svSer.CapNhatThongTinSinhVien(model);

            TempData["Success"] = "Cập nhật thông tin thành công";
            return RedirectToAction(nameof(CaNhan));
        }


        [AllowAnonymous]
        public IActionResult Login()
        {
            return View(new DangNhapModel());
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(DangNhapModel model)
        {
            if (!ModelState.IsValid) return View(model);

            try
            {
                var user = await _svAuth.DangNhap(model.Email, model.Password);

                var claims = new List<Claim>
                {
                 new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                 new Claim(ClaimTypes.Email, user.Email),
                 new Claim(ClaimTypes.Name, user.FullName ?? user.Email),
                 new Claim(ClaimTypes.Role, user.Role ?? "")
                };

                // nếu là sinh viên
                if (user.Student != null)
                {
                    claims.Add(new Claim("StudentId", user.Student.StudentId.ToString()));
                    claims.Add(new Claim("StudentCode", user.Student.StudentCode));
                }

                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    principal,
                    new AuthenticationProperties
                    {
                        IsPersistent = model.RememberMe,
                        ExpiresUtc = model.RememberMe
                            ? DateTimeOffset.UtcNow.AddDays(7)
                            : DateTimeOffset.UtcNow.AddHours(2)
                    });

                return user.Role?.ToUpper() switch
                {
                    "STUDENT" => RedirectToAction("Index", "Home"),
                    "TEACHER" => RedirectToAction("Index", "Giangvien"),
                    "ADMIN" => RedirectToAction("Index", "Admin"),
                    _ => RedirectToAction("Index", "Home") // Mặc định
                };
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(model);
            }
        }

        public IActionResult SignUp()
        {
            return View(new DangKySinhVien());
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SignUp(DangKySinhVien model)
        {
            if(!ModelState.IsValid) return View();
            await _svAuth.DangKysv(model);
            return RedirectToAction("Login");
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            // Xóa cookie COMPASS thủ công (phòng trường hợp SignOutAsync không xóa)
            Response.Cookies.Delete("COMPASS");

            return RedirectToAction("Login");
        }

        // GET: /Sinhvien/ThongBao? filter=all&page=1
        public async Task<IActionResult> CanhBao(string filter = "all", int page = 1)
        {
            var studentId = GetCurrentStudentId();
            var model = await _svSer.GetNotificationsPageAsync(studentId, filter, page);
            return View(model);
        }

        // POST: Mark as read
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var studentId = GetCurrentStudentId();
            await _svSer.MarkAsReadAsync(id, studentId);
            return Ok();
        }

        // POST: Mark all as read
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAllAsRead()
        {
            var studentId = GetCurrentStudentId();
            await _svSer.MarkAllAsReadAsync(studentId);
            TempData["Success"] = "Đã đánh dấu tất cả thông báo là đã đọc. ";
            return RedirectToAction(nameof(CanhBao));
        }

        private int GetCurrentStudentId()
        {
            // ưu tiên claim "StudentId"
            var studentIdClaim = User.FindFirstValue("StudentId");
            if (!string.IsNullOrWhiteSpace(studentIdClaim))
                return int.Parse(studentIdClaim);

            // fallback (nếu hệ bạn StudentId == UserId)
            return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
        }

        [HttpPost]
        [Authorize(Roles = "STUDENT")]
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
                await _svAuth.DoiMatKhauAsync(userId, model.OldPassword, model.NewPassword);
                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
