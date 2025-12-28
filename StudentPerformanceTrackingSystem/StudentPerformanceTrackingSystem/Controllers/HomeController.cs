using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SPTS_Repository.Entities;
using SPTS_Service.Interface;
using SPTS_Service.ViewModel;
using StudentPerformanceTrackingSystem.Models;
using System.Data;
using System.Diagnostics;
using System.Security.Claims;

namespace StudentPerformanceTrackingSystem.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ISinhVienService _svSer;

        public HomeController(ILogger<HomeController> logger, ISinhVienService svSer)
        {
            _logger = logger;
            _svSer = svSer;
        }
        [Authorize]
        public async Task<IActionResult> Index()
        {
            var studentIdClaim = User.FindFirstValue("StudentId");
            if (string.IsNullOrWhiteSpace(studentIdClaim))
                return Forbid();

            int studentId = int.Parse(studentIdClaim);

            var vm = await _svSer.GetDashboardAsync(studentId);
            return View(vm);
        }
        
        public async Task<IActionResult> BangDiemSinhVien()
        {
            var studentIdClaim = User.FindFirstValue("StudentId");
            if (string.IsNullOrWhiteSpace(studentIdClaim))
                return Forbid();

            int studentId = int.Parse(studentIdClaim);

            var vm = await _svSer.GetDashboardAsync(studentId);
            return View(vm);
        }

        public IActionResult CaNhan()
        {
            return View();
        }
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
                var user = await _svSer.DangNhap(model.Email, model.Password);

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

                return RedirectToAction("Index");
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
            await _svSer.DangKysv(model);
            return RedirectToAction("Login");
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
