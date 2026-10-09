using CongDichVuSinhVien.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CongDichVuSinhVien.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "ADMIN,CAN_BO")]
    public class DashboardController : Controller
    {
        private readonly CongDichVuSinhVienContext _context;

        public DashboardController(CongDichVuSinhVienContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.TongHoSo = await _context.HoSoDichVu.CountAsync();
            ViewBag.ChoTiepNhan = await _context.HoSoDichVu.CountAsync(h => h.TrangThai == "Chờ tiếp nhận");
            ViewBag.DangXuLy = await _context.HoSoDichVu.CountAsync(h => h.TrangThai == "Đang xử lý");
            ViewBag.HoanThanh = await _context.HoSoDichVu.CountAsync(h => h.TrangThai == "Hoàn thành" || h.TrangThai == "Đã duyệt");
            ViewBag.TuChoi = await _context.HoSoDichVu.CountAsync(h => h.TrangThai == "Từ chối");

            ViewBag.TongSinhVien = await _context.SinhVien.CountAsync();
            ViewBag.TongDichVu = await _context.DichVu.CountAsync(d => d.KichHoat == true);
            ViewBag.TongMauNLP = await _context.DuLieuHuanLuyenNlp.CountAsync();

            var hoSoMoiNhat = await _context.HoSoDichVu
                .Include(h => h.MaDichVuNavigation)
                .Include(h => h.MaSinhVienNavigation)
                    .ThenInclude(s => s.MaNguoiDungNavigation)
                .OrderByDescending(h => h.NgayNop)
                .Take(6)
                .ToListAsync();

            return View(hoSoMoiNhat);
        }
    }
}