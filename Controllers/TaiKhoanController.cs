using System.Security.Claims;
using CongDichVuSinhVien.Data;
using CongDichVuSinhVien.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CongDichVuSinhVien.Controllers
{
    public class TaiKhoanController : Controller
    {
        private readonly CongDichVuSinhVienContext _context;

        public TaiKhoanController(CongDichVuSinhVienContext context)
        {
            _context = context;
        }

        // GET: /TaiKhoan/DangNhap
        [HttpGet]
        public IActionResult DangNhap(string? returnUrl = null)
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return ChuyenHuongTheoVaiTro();
            }

            ViewData["ReturnUrl"] = returnUrl;
            return View(new DangNhapViewModel { ReturnUrl = returnUrl });
        }

        // POST: /TaiKhoan/DangNhap
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DangNhap(DangNhapViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // 1. Kiểm tra tài khoản
            var user = await _context.NguoiDung
                .FirstOrDefaultAsync(u => u.TenDangNhap == model.TenDangNhap.Trim() && u.MatKhau == model.MatKhau.Trim());

            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Tên đăng nhập hoặc mật khẩu không chính xác.");
                return View(model);
            }

            if (user.TrangThai == false)
            {
                ModelState.AddModelError(string.Empty, "Tài khoản này hiện đang bị khóa.");
                return View(model);
            }

            // 2. Xác định vai trò
            string roleName = "SINH_VIEN";
            string maDinhDanh = user.TenDangNhap;

            var canBo = await _context.CanBo.FirstOrDefaultAsync(c => c.MaNguoiDung == user.MaNguoiDung);
            var sinhVien = await _context.SinhVien.FirstOrDefaultAsync(s => s.MaNguoiDung == user.MaNguoiDung);

            var maQuyens = await _context.PhanQuyenNguoiDung
                .Where(p => p.MaNguoiDung == user.MaNguoiDung && p.MaQuyen != null)
                .Select(p => p.MaQuyen!.Value)
                .ToListAsync();

            if (canBo != null)
            {
                roleName = "CAN_BO";
                maDinhDanh = canBo.MaCanBo;
                if (!maQuyens.Contains(2)) maQuyens.Add(2); // Mã 2 là CAN_BO
            }
            else if (sinhVien != null)
            {
                roleName = "SINH_VIEN";
                maDinhDanh = sinhVien.MaSinhVien;
                if (!maQuyens.Contains(3)) maQuyens.Add(3); // Mã 3 là SINH_VIEN
            }

            if (maQuyens.Contains(1)) // Mã 1 là ADMIN
            {
                roleName = "ADMIN";
            }

            // 3. ĐỌC TOÀN BỘ CODE CHỨC NĂNG TỪ CSDL (DÙNG SQL TRỰC TIẾP, AN TOÀN 100% KHÔNG BỊ LỖI NAVIGATION)
            var listPermissions = new List<string>();
            if (maQuyens.Any())
            {
                var conn = _context.Database.GetDbConnection();
                await _context.Database.OpenConnectionAsync();
                using var cmd = conn.CreateCommand();
                string inClause = string.Join(",", maQuyens);
                cmd.CommandText = $"SELECT DISTINCT cn.Code FROM ChucNang cn INNER JOIN QuyenChucNang qc ON cn.MaChucNang = qc.MaChucNang WHERE qc.MaQuyen IN ({inClause})";

                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    listPermissions.Add(reader.GetString(0));
                }
            }

            // 4. Tạo Claims
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.MaNguoiDung.ToString()),
                new Claim(ClaimTypes.Name, user.HoTen ?? user.TenDangNhap),
                new Claim("TenDangNhap", user.TenDangNhap),
                new Claim(ClaimTypes.Email, user.Email ?? ""),
                new Claim(ClaimTypes.Role, roleName),
                new Claim("MaDinhDanh", maDinhDanh)
            };

            // Nạp từng quyền hạn (HasPermission) vào Claim
            foreach (var code in listPermissions)
            {
                claims.Add(new Claim("Permission", code));
            }

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = model.GhiNho,
                ExpiresUtc = model.GhiNho ? DateTimeOffset.UtcNow.AddDays(7) : DateTimeOffset.UtcNow.AddHours(8)
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            // 5. Chuyển hướng
            if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
            {
                return Redirect(model.ReturnUrl);
            }

            return ChuyenHuongTheoVaiTro(roleName);
        }

        // DUY NHẤT 1 HÀM NÀY (KHÔNG BỊ TRÙNG LẶP)
        private IActionResult ChuyenHuongTheoVaiTro(string? role = null)
        {
            role ??= User.FindFirst(ClaimTypes.Role)?.Value;

            if (role == "ADMIN" || role == "CAN_BO")
            {
                // Cán bộ & Admin chuyển thẳng vào Dashboard của Area Admin
                return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
            }

            // Sinh viên chuyển về trang chủ Portal
            return RedirectToAction("Index", "Home");
        }

        // GET: /TaiKhoan/DangXuat
        [HttpGet]
        public async Task<IActionResult> DangXuat()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("DangNhap", "TaiKhoan");
        }

        // GET: /TaiKhoan/TuChoiTruyCap
        [HttpGet]
        public IActionResult TuChoiTruyCap()
        {
            return View();
        }
    }
}