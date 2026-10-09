using CongDichVuSinhVien.Data;
using CongDichVuSinhVien.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CongDichVuSinhVien.Controllers
{
    [Authorize] // Bắt buộc đăng nhập
    [Route("HoSo")]
    public class HoSoController : Controller
    {
        private readonly CongDichVuSinhVienContext _context;
        private readonly IWebHostEnvironment _env;

        public HoSoController(CongDichVuSinhVienContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // GET: /HoSo/NopHoSo?maDichVu=1
        [HttpGet("NopHoSo")]
        public async Task<IActionResult> NopHoSo(int maDichVu)
        {
            // 1. Lấy mã người dùng từ Cookie
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int maNguoiDung))
            {
                return RedirectToAction("DangNhap", "TaiKhoan", new { returnUrl = $"/HoSo/NopHoSo?maDichVu={maDichVu}" });
            }

            // 2. Tìm thông tin sinh viên theo MaNguoiDung
            var sinhVien = await _context.SinhVien
                .Include(s => s.MaNguoiDungNavigation)
                .FirstOrDefaultAsync(s => s.MaNguoiDung == maNguoiDung);

            // TỰ ĐỘNG KHẮC PHỤC: Nếu tài khoản chưa có bản ghi trong bảng SinhVien -> Tự động khởi tạo để không bị đá về Home
            if (sinhVien == null)
            {
                var userNd = await _context.NguoiDung.FindAsync(maNguoiDung);
                if (userNd != null)
                {
                    sinhVien = await _context.SinhVien.FirstOrDefaultAsync(s => s.MaSinhVien == userNd.TenDangNhap);
                    if (sinhVien == null)
                    {
                        sinhVien = new SinhVien
                        {
                            MaSinhVien = userNd.TenDangNhap,
                            MaNguoiDung = userNd.MaNguoiDung,
                            LopHoc = "D21CQCN01",
                            KhoaVien = "Công nghệ Thông tin",
                            TinhTrangHocTap = "Đang học"
                        };
                        _context.SinhVien.Add(sinhVien);
                        await _context.SaveChangesAsync();
                        sinhVien.MaNguoiDungNavigation = userNd;
                    }
                }
            }

            // 3. Lấy thông tin thủ tục dịch vụ
            var dichVu = await _context.DichVu.FirstOrDefaultAsync(d => d.MaDichVu == maDichVu && d.KichHoat == true);
            if (dichVu == null)
            {
                return NotFound();
            }

            // 4. Tạo ViewModel điền sẵn dữ liệu
            var viewModel = new NopHoSoViewModel
            {
                MaDichVu = dichVu.MaDichVu,
                TenDichVu = dichVu.TenDichVu,
                KyHieuDichVu = dichVu.KyHieuDichVu,
                DonViXuLy = dichVu.DonViXuLy,
                LePhi = dichVu.LePhi,
                ThoiGianXuLyNgay = dichVu.ThoiGianXuLyNgay,
                ThanhPhanHoSo = dichVu.ThanhPhanHoSo,

                MaSinhVien = sinhVien != null ? sinhVien.MaSinhVien : User.Identity?.Name ?? "",
                HoTen = sinhVien?.MaNguoiDungNavigation?.HoTen ?? User.Identity?.Name ?? "",
                LopHoc = sinhVien?.LopHoc ?? "Chưa cập nhật",
                KhoaVien = sinhVien?.KhoaVien ?? "Chưa phân khoa",
                Email = sinhVien?.MaNguoiDungNavigation?.Email ?? "",
                SoDienThoai = sinhVien?.MaNguoiDungNavigation?.SoDienThoai ?? ""
            };

            return View(viewModel);
        }

        // POST: /HoSo/NopHoSo
        [HttpPost("NopHoSo")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> NopHoSo(NopHoSoViewModel model)
        {
            if (model.HinhThucNhan == "BuuDien" && string.IsNullOrWhiteSpace(model.DiaChiNhan))
            {
                ModelState.AddModelError("DiaChiNhan", "Vui lòng nhập địa chỉ nhận thư khi chọn gửi bưu điện!");
            }

            if (!ModelState.IsValid) return View(model);

            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            int.TryParse(userIdStr, out int maNguoiDung);

            var sinhVien = await _context.SinhVien.FirstOrDefaultAsync(s => s.MaNguoiDung == maNguoiDung);
            if (sinhVien == null) return RedirectToAction("TuChoiTruyCap", "TaiKhoan");

            string? duongDanFile = null;
            if (model.FileDinhKem != null && model.FileDinhKem.Length > 0)
            {
                string uploadFolder = Path.Combine(_env.WebRootPath, "uploads", "hoso");
                if (!Directory.Exists(uploadFolder)) Directory.CreateDirectory(uploadFolder);

                string fileName = $"{DateTime.Now:yyyyMMdd_HHmmss}_{Path.GetFileName(model.FileDinhKem.FileName)}";
                using (var stream = new FileStream(Path.Combine(uploadFolder, fileName), FileMode.Create))
                {
                    await model.FileDinhKem.CopyToAsync(stream);
                }
                duongDanFile = $"/uploads/hoso/{fileName}";
            }

            string maTraCuu = $"HS-{DateTime.Now:yyyyMMdd}-{new Random().Next(1000, 9999)}";

            string noiDungDon = model.HinhThucNhan == "BuuDien"
                ? $"[HÌNH THỨC NHẬN: CHUYỂN PHÁT NHANH BƯU ĐIỆN]\n- Người nhận: {model.HoTenNguoiNhan} | SĐT: {model.SoDienThoaiNhan}\n- Địa chỉ: {model.DiaChiNhan}\n- Ghi chú giao: {model.GhiChuGiaoHang ?? "Không"}\n\n[NỘI DUNG ĐỀ NGHỊ]:\n{model.LyDoXin.Trim()}"
                : $"[HÌNH THỨC NHẬN: TRỰC TIẾP TẠI BỘ PHẬN MỘT CỬA]\n\n[NỘI DUNG ĐỀ NGHỊ]:\n{model.LyDoXin.Trim()}";

            var hoSoMoi = new HoSoDichVu
            {
                MaTraCuu = maTraCuu,
                MaDichVu = model.MaDichVu,
                MaSinhVien = sinhVien.MaSinhVien,
                LyDoXin = noiDungDon,
                TepDinhKem = duongDanFile,
                TrangThai = "Chờ tiếp nhận",
                NgayNop = DateTime.Now
            };

            _context.HoSoDichVu.Add(hoSoMoi);
            await _context.SaveChangesAsync();

            _context.LichSuXuLy.Add(new LichSuXuLy
            {
                MaHoSo = hoSoMoi.MaHoSo,
                MaNguoiThucHien = maNguoiDung,
                TrangThai = "Chờ tiếp nhận",
                NoiDung = "Sinh viên đã nộp hồ sơ trực tuyến.",
                NgayThucHien = DateTime.Now
            });
            await _context.SaveChangesAsync();

            return RedirectToAction("ThanhCong", new { id = hoSoMoi.MaHoSo });
        }

        // GET: /HoSo/ThanhCong/5
        [HttpGet("ThanhCong/{id}")]
        public async Task<IActionResult> ThanhCong(int id)
        {
            var hoSo = await _context.HoSoDichVu
                .Include(h => h.MaDichVuNavigation)
                .FirstOrDefaultAsync(h => h.MaHoSo == id);

            if (hoSo == null) return NotFound();
            return View(hoSo);
        }

        // GET: /HoSo/DanhSach (ĐÃ SỬA CHUẨN BẢO MẬT 100%)
        [HttpGet("DanhSach")]
        public async Task<IActionResult> DanhSach(string? loai = "tatca", string? tuKhoa = null)
        {
            // 1. KIỂM TRA XEM CÓ THỰC SỰ LÀ CÁN BỘ HOẶC ADMIN KHÔNG
            bool isCanBoOrAdmin = User.IsInRole("ADMIN") || User.IsInRole("QuanTri")
                               || User.IsInRole("CAN_BO") || User.IsInRole("CanBo");

            // 2. TÌM MSSV CỦA NGƯỜI ĐANG ĐĂNG NHẬP QUA NHIỀU NGUỒN (KHÔNG ĐỂ BỊ LỖI NULL)
            string? mssv = User.FindFirst("MaDinhDanh")?.Value;

            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(userIdStr, out int maNguoiDung))
            {
                var sv = await _context.SinhVien.FirstOrDefaultAsync(s => s.MaNguoiDung == maNguoiDung);
                if (sv != null)
                {
                    mssv = sv.MaSinhVien;
                }
            }

            // Nếu vẫn chưa có MSSV, thử tìm qua Tên đăng nhập
            if (string.IsNullOrEmpty(mssv))
            {
                var tenDangNhap = User.Identity?.Name ?? User.FindFirst("TenDangNhap")?.Value;
                var sv = await _context.SinhVien.FirstOrDefaultAsync(s => s.MaSinhVien == tenDangNhap || s.MaNguoiDungNavigation.TenDangNhap == tenDangNhap);
                if (sv != null)
                {
                    mssv = sv.MaSinhVien;
                }
            }

            var query = _context.HoSoDichVu
                .Include(h => h.MaDichVuNavigation)
                .Include(h => h.MaSinhVienNavigation)
                    .ThenInclude(s => s.MaNguoiDungNavigation)
                .Include(h => h.MaCanBoXuLyNavigation)
                .AsQueryable();

            // 3. QUY TẮC BẢO MẬT: NẾU KHÔNG PHẢI LÀ CÁN BỘ/ADMIN -> BẮT BUỘC CHỈ XEM HỒ SƠ CỦA CHÍNH MÌNH
            if (!isCanBoOrAdmin)
            {
                if (!string.IsNullOrEmpty(mssv))
                {
                    // Lọc đúng MSSV của sinh viên này
                    query = query.Where(h => h.MaSinhVien == mssv);
                }
                else
                {
                    // Tài khoản sinh viên này chưa có MSSV hợp lệ trong DB -> Hiển thị rỗng, không được xem của người khác
                    query = query.Where(h => false);
                }
                ViewBag.IsSinhVien = true;
            }
            else
            {
                // Cán bộ / Admin mới được xem toàn trường
                ViewBag.IsSinhVien = false;
            }

            // 4. Lọc trạng thái
            if (loai == "hoanthanh")
            {
                query = query.Where(h => h.TrangThai == "Hoàn thành" || h.TrangThai == "Đã duyệt");
            }

            // 5. Tìm kiếm từ khóa
            if (!string.IsNullOrWhiteSpace(tuKhoa))
            {
                query = query.Where(h => h.MaTraCuu.Contains(tuKhoa.Trim()) || h.MaDichVuNavigation.TenDichVu.Contains(tuKhoa.Trim()));
            }

            var danhSachHoSo = await query
                .OrderByDescending(h => h.NgayNop)
                .ToListAsync();

            ViewBag.Loai = loai;
            ViewBag.TuKhoa = tuKhoa;

            return View(danhSachHoSo);
        }

        // GET: /HoSo/ChiTiet/5 (ĐÃ SỬA CHẶN XEM TRỘM)
        [HttpGet("ChiTiet/{id}")]
        public async Task<IActionResult> ChiTiet(int id)
        {
            var hoSo = await _context.HoSoDichVu
                .Include(h => h.MaDichVuNavigation)
                .Include(h => h.MaSinhVienNavigation)
                    .ThenInclude(s => s.MaNguoiDungNavigation)
                .Include(h => h.MaCanBoXuLyNavigation)
                    .ThenInclude(c => c!.MaNguoiDungNavigation)
                .Include(h => h.LichSuXuLy)
                    .ThenInclude(l => l.MaNguoiThucHienNavigation)
                .FirstOrDefaultAsync(h => h.MaHoSo == id);

            if (hoSo == null) return NotFound();

            bool isCanBoOrAdmin = User.IsInRole("ADMIN") || User.IsInRole("QuanTri")
                               || User.IsInRole("CAN_BO") || User.IsInRole("CanBo");

            // NẾU LÀ SINH VIÊN -> KIỂM TRA MSSV CÓ KHỚP VỚI HỒ SƠ KHÔNG
            if (!isCanBoOrAdmin)
            {
                string? mssv = User.FindFirst("MaDinhDanh")?.Value;
                var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (int.TryParse(userIdStr, out int maNguoiDung))
                {
                    var sv = await _context.SinhVien.FirstOrDefaultAsync(s => s.MaNguoiDung == maNguoiDung);
                    if (sv != null) mssv = sv.MaSinhVien;
                }

                // Nếu hồ sơ này của người khác -> CHẶN NGAY LẬP TỨC
                if (string.IsNullOrEmpty(mssv) || hoSo.MaSinhVien != mssv)
                {
                    return RedirectToAction("TuChoiTruyCap", "TaiKhoan");
                }
            }

            return View(hoSo);
        }
    }
}