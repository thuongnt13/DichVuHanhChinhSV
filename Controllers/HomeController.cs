using Microsoft.AspNetCore.Mvc;

namespace CongDichVuSinhVien.Controllers
{
    public class HomeController : Controller
    {
        // Trang chủ chỉ đóng vai trò tiếp nhận tham số và điều phối Component
        public IActionResult Index(string? tuKhoa = null, int? maLinhVuc = null, string? maTraCuu = null, int trang = 1)
        {
            ViewBag.TuKhoa = tuKhoa;
            ViewBag.MaLinhVuc = maLinhVuc;
            ViewBag.MaTraCuu = maTraCuu;
            ViewBag.Trang = trang;
            return View();
        }

        public IActionResult HuongDan() => View();
        public IActionResult GioiThieu() => View();
    }
}