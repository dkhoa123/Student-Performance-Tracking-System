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
       
        public IActionResult ChitietLop()
        {
            return View();
        }
      
        public IActionResult DiemSo()
        {
            return View();
        }
    }
}
