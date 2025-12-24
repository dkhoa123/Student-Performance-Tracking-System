using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using SPTS_Service;
using SPTS_Service.Interface;
using StudentPerformanceTrackingSystem.Models;

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

        public IActionResult Index()
        {
            return View();
        }
        
        public IActionResult BangDiemSinhVien()
        {
            return View();
        }

        public IActionResult CaNhan()
        {
            return View();
        }
        public IActionResult Login()
        {
            return View();
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

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
