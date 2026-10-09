using CongDichVuSinhVien.Data;
using CongDichVuSinhVien.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CongDichVuSinhVien.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "ADMIN,CAN_BO")]
    [Route("Admin/QuanLyThuTuc")]
    public class QuanLyThuTucController : Controller
    {
        private readonly CongDichVuSinhVienContext _context;
        public QuanLyThuTucController(CongDichVuSinhVienContext context) => _context = context;

        [HttpGet("")]
        [HttpGet("Index")]
        public async Task<IActionResult> Index(int trang = 1, string? tuKhoa = null, int kichThuocTrang = 8)
        {
            var query = _context.DichVu.Include(d => d.MaLinhVucNavigation).AsQueryable();
            if (!string.IsNullOrWhiteSpace(tuKhoa))
                query = query.Where(d => d.TenDichVu.Contains(tuKhoa) || d.KyHieuDichVu.Contains(tuKhoa));

            int total = await query.CountAsync();
            ViewBag.TongSoTrang = (int)Math.Ceiling((double)total / kichThuocTrang);
            ViewBag.TrangHienTai = trang;
            ViewBag.TuKhoa = tuKhoa;
            ViewBag.LinhVucs = await _context.LinhVuc.ToListAsync();

            var list = await query.OrderBy(d => d.MaDichVu)
                                  .Skip((trang - 1) * kichThuocTrang)
                                  .Take(kichThuocTrang)
                                  .ToListAsync();
            return View(list);
        }

        // AJAX: Lấy chi tiết thủ tục
        [HttpGet("GetJson/{id}")]
        public async Task<IActionResult> GetJson(int id)
        {
            var item = await _context.DichVu.FindAsync(id);
            if (item == null) return NotFound();
            return Json(item);
        }

        // AJAX: Lưu (Thêm mới hoặc Cập nhật)
        [HttpPost("SaveAjax")]
        public async Task<IActionResult> SaveAjax([FromBody] DichVu model)
        {
            if (model.MaDichVu == 0)
            {
                model.KichHoat = true;
                _context.DichVu.Add(model);
            }
            else
            {
                var existing = await _context.DichVu.FindAsync(model.MaDichVu);
                if (existing == null) return Json(new { success = false, message = "Không tìm thấy thủ tục!" });

                existing.KyHieuDichVu = model.KyHieuDichVu;
                existing.TenDichVu = model.TenDichVu;
                existing.MaLinhVuc = model.MaLinhVuc;
                existing.DonViXuLy = model.DonViXuLy;
                existing.ThoiGianXuLyNgay = model.ThoiGianXuLyNgay;
                existing.LePhi = model.LePhi;
                existing.MoTa = model.MoTa;
            }
            await _context.SaveChangesAsync();
            return Json(new { success = true, message = "Lưu dữ liệu thành công!" });
        }

        // AJAX: Xóa thủ tục
        [HttpPost("DeleteAjax/{id}")]
        public async Task<IActionResult> DeleteAjax(int id)
        {
            var item = await _context.DichVu.FindAsync(id);
            if (item == null) return Json(new { success = false, message = "Không tìm thấy!" });

            _context.DichVu.Remove(item);
            await _context.SaveChangesAsync();
            return Json(new { success = true, message = "Đã xóa thủ tục!" });
        }

        // AJAX: Đổi trạng thái Bật/Tắt
        [HttpPost("ToggleActiveAjax/{id}")]
        public async Task<IActionResult> ToggleActiveAjax(int id)
        {
            var item = await _context.DichVu.FindAsync(id);
            if (item != null)
            {
                item.KichHoat = !(item.KichHoat ?? true);
                await _context.SaveChangesAsync();
                return Json(new { success = true, active = item.KichHoat });
            }
            return Json(new { success = false });
        }
    }
}