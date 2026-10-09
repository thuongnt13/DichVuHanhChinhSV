using Microsoft.AspNetCore.Mvc;
using CongDichVuSinhVien.Services;
namespace CongDichVuSinhVien.Controllers
{
    [Route("TroLyNlp")]
    public class TroLyNlpController : Controller
    {
        private readonly INlpService _nlpService;

        public TroLyNlpController(INlpService nlpService)
        {
            _nlpService = nlpService;
        }

        // Endpoint phục vụ tìm kiếm điều hướng
        [HttpGet("TimKiem")]
        public IActionResult TimKiem(string cauHoi)
        {
            if (string.IsNullOrWhiteSpace(cauHoi))
                return RedirectToAction("Index", "Home");

            return RedirectToAction("Index", "Home", new { tuKhoa = cauHoi.Trim() });
        }

        // Endpoint API JSON: phục vụ cho gợi ý tự động (Autocomplete/Chatbot sau này nếu cần)
        public async Task<IActionResult> DuDoanNhuCau(string cauHoi)
        {
            if (string.IsNullOrWhiteSpace(cauHoi))
                return Json(new { success = false, data = new List<object>() });

            var ketQua = await _nlpService.PhanTichNhuCauSinhVienAsync(cauHoi);
            return Json(new { success = true, data = ketQua });
        }
    }
}