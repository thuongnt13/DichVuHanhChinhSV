using CongDichVuSinhVien.Data;
using CongDichVuSinhVien.Models;
using CongDichVuSinhVien.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CongDichVuSinhVien.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "ADMIN,CAN_BO")]
    [Route("Admin/QuanLyNlp")]
    public class QuanLyNlpController : Controller
    {
        private readonly CongDichVuSinhVienContext _context;
        private readonly INlpService _nlpService;

        public QuanLyNlpController(CongDichVuSinhVienContext context, INlpService nlpService)
        {
            _context = context;
            _nlpService = nlpService;
        }

        [HttpGet("")]
        [HttpGet("Index")]
        public async Task<IActionResult> Index(int trang = 1, string? tuKhoa = null, int kichThuocTrang = 10)
        {
            var query = _context.DuLieuHuanLuyenNlp.AsQueryable();
            if (!string.IsNullOrWhiteSpace(tuKhoa))
                query = query.Where(n => n.CauHoiMau.Contains(tuKhoa) || n.KyHieuDichVu.Contains(tuKhoa));

            int total = await query.CountAsync();
            ViewBag.TongSoTrang = (int)Math.Ceiling((double)total / kichThuocTrang);
            ViewBag.TrangHienTai = trang;
            ViewBag.TuKhoa = tuKhoa;
            ViewBag.DichVus = await _context.DichVu.Where(d => d.KichHoat == true).ToListAsync();

            var list = await query.OrderByDescending(n => n.MaMau)
                                  .Skip((trang - 1) * kichThuocTrang)
                                  .Take(kichThuocTrang)
                                  .ToListAsync();
            return View(list);
        }

        [HttpGet("GetJson/{id}")]
        public async Task<IActionResult> GetJson(int id)
        {
            var item = await _context.DuLieuHuanLuyenNlp.FindAsync(id);
            if (item == null) return NotFound();
            return Json(item);
        }

        [HttpPost("SaveAjax")]
        public async Task<IActionResult> SaveAjax([FromBody] DuLieuHuanLuyenNlp model)
        {
            if (model.MaMau == 0)
            {
                _context.DuLieuHuanLuyenNlp.Add(model);
            }
            else
            {
                var existing = await _context.DuLieuHuanLuyenNlp.FindAsync(model.MaMau);
                if (existing == null) return Json(new { success = false });
                existing.KyHieuDichVu = model.KyHieuDichVu;
                existing.CauHoiMau = model.CauHoiMau;
            }
            await _context.SaveChangesAsync();
            return Json(new { success = true, message = "Lưu câu hỏi huấn luyện thành công!" });
        }

        [HttpPost("DeleteAjax/{id}")]
        public async Task<IActionResult> DeleteAjax(int id)
        {
            var item = await _context.DuLieuHuanLuyenNlp.FindAsync(id);
            if (item != null)
            {
                _context.DuLieuHuanLuyenNlp.Remove(item);
                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            return Json(new { success = false });
        }

        // AJAX TEST TRỰC TIẾP
        [HttpPost("TestAjax")]
        public async Task<IActionResult> TestAjax([FromBody] string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return Json(new List<object>());
            var result = await _nlpService.PhanTichNhuCauSinhVienAsync(text);
            return Json(result);
        }
    }
}