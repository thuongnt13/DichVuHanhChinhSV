using CongDichVuSinhVien.Data;
using CongDichVuSinhVien.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CongDichVuSinhVien.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "ADMIN")]
    [Route("Admin/QuanLyNguoiDung")]
    public class QuanLyNguoiDungController : Controller
    {
        private readonly CongDichVuSinhVienContext _context;
        public QuanLyNguoiDungController(CongDichVuSinhVienContext context) => _context = context;

        [HttpGet("")]
        [HttpGet("Index")]
        public async Task<IActionResult> Index(int trang = 1, string? tuKhoa = null, int kichThuocTrang = 10)
        {
            var query = _context.NguoiDung
                .Include(u => u.SinhVien)
                .Include(u => u.CanBo)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(tuKhoa))
                query = query.Where(u => u.TenDangNhap.Contains(tuKhoa) || u.HoTen.Contains(tuKhoa) || u.Email.Contains(tuKhoa));

            int total = await query.CountAsync();
            ViewBag.TongSoTrang = (int)Math.Ceiling((double)total / kichThuocTrang);
            ViewBag.TrangHienTai = trang;
            ViewBag.TuKhoa = tuKhoa;

            var list = await query.OrderByDescending(u => u.MaNguoiDung)
                                  .Skip((trang - 1) * kichThuocTrang)
                                  .Take(kichThuocTrang)
                                  .ToListAsync();
            return View(list);
        }

        [HttpGet("GetJson/{id}")]
        public async Task<IActionResult> GetJson(int id)
        {
            var u = await _context.NguoiDung
                .Include(x => x.SinhVien)
                .Include(x => x.CanBo)
                .FirstOrDefaultAsync(x => x.MaNguoiDung == id);
            if (u == null) return NotFound();

            return Json(new
            {
                u.MaNguoiDung,
                u.TenDangNhap,
                u.HoTen,
                u.Email,
                u.SoDienThoai,
                VaiTro = u.CanBo != null ? "CAN_BO" : (u.SinhVien != null ? "SINH_VIEN" : "ADMIN"),
                MaSinhVien = u.SinhVien?.MaSinhVien,
                LopHoc = u.SinhVien?.LopHoc,
                KhoaVien = u.SinhVien?.KhoaVien,
                MaCanBo = u.CanBo?.MaCanBo,
                PhongBan = u.CanBo?.PhongBan
            });
        }

        [HttpPost("SaveAjax")]
        public async Task<IActionResult> SaveAjax([FromBody] UserAjaxDto data)
        {
            if (data == null)
                return Json(new { success = false, message = "Dữ liệu gửi lên không hợp lệ!" });

            NguoiDung? user;

            if (data.MaNguoiDung == 0)
            {
                // 1. Thêm người dùng mới
                user = new NguoiDung
                {
                    TenDangNhap = data.TenDangNhap.Trim(),
                    HoTen = data.HoTen.Trim(),
                    Email = data.Email.Trim(),
                    SoDienThoai = data.SoDienThoai?.Trim(),
                    MatKhau = string.IsNullOrEmpty(data.MatKhau) ? "123456" : data.MatKhau.Trim(),
                    TrangThai = true,
                    NgayTao = DateTime.Now
                };
                _context.NguoiDung.Add(user);
                await _context.SaveChangesAsync();

                // Tự động phân loại bản ghi theo vai trò
                if (data.VaiTro == "SINH_VIEN")
                {
                    _context.SinhVien.Add(new SinhVien
                    {
                        MaSinhVien = string.IsNullOrEmpty(data.MaDinhDanh) ? data.TenDangNhap : data.MaDinhDanh,
                        MaNguoiDung = user.MaNguoiDung,
                        LopHoc = data.LopHoc ?? "D21CQCN01",
                        KhoaVien = data.KhoaVien ?? "Công nghệ Thông tin",
                        TinhTrangHocTap = "Đang học"
                    });
                }
                else if (data.VaiTro == "CAN_BO")
                {
                    _context.CanBo.Add(new CanBo
                    {
                        MaCanBo = string.IsNullOrEmpty(data.MaDinhDanh) ? ("CB_" + data.TenDangNhap) : data.MaDinhDanh,
                        MaNguoiDung = user.MaNguoiDung,
                        PhongBan = data.PhongBan ?? "Phòng Một cửa",
                        ChucVu = "Cán bộ thụ lý"
                    });
                }
            }
            else
            {
                // 2. Chỉnh sửa người dùng hiện có
                user = await _context.NguoiDung.FindAsync(data.MaNguoiDung);
                if (user == null)
                    return Json(new { success = false, message = "Không tìm thấy người dùng!" });

                user.HoTen = data.HoTen.Trim();
                user.Email = data.Email.Trim();
                user.SoDienThoai = data.SoDienThoai?.Trim();

                if (!string.IsNullOrEmpty(data.MatKhau))
                    user.MatKhau = data.MatKhau.Trim();
            }

            await _context.SaveChangesAsync();
            return Json(new { success = true, message = "Lưu thông tin tài khoản thành công!" });
        }

        [HttpPost("ToggleLockAjax/{id}")]
        public async Task<IActionResult> ToggleLockAjax(int id)
        {
            var user = await _context.NguoiDung.FindAsync(id);
            if (user != null)
            {
                user.TrangThai = !(user.TrangThai ?? true);
                await _context.SaveChangesAsync();
                return Json(new { success = true, active = user.TrangThai });
            }
            return Json(new { success = false });
        }

        [HttpPost("DeleteAjax/{id}")]
        public async Task<IActionResult> DeleteAjax(int id)
        {
            var user = await _context.NguoiDung.FindAsync(id);
            if (user != null)
            {
                _context.NguoiDung.Remove(user);
                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            return Json(new { success = false });
        }
        public class UserAjaxDto
        {
            public int MaNguoiDung { get; set; }
            public string TenDangNhap { get; set; } = string.Empty;
            public string? MatKhau { get; set; }
            public string HoTen { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public string? SoDienThoai { get; set; }
            public string VaiTro { get; set; } = "SINH_VIEN";
            public string? MaDinhDanh { get; set; }
            public string? LopHoc { get; set; }
            public string? KhoaVien { get; set; }
            public string? PhongBan { get; set; }
        }
    }
}