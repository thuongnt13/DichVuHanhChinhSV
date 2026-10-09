using CongDichVuSinhVien.Attributes;
using CongDichVuSinhVien.Data;
using CongDichVuSinhVien.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CongDichVuSinhVien.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "ADMIN,CAN_BO")]
    [Route("Admin/QuanLyHoSo")]
    public class QuanLyHoSoController : Controller
    {
        private readonly CongDichVuSinhVienContext _context;
        private readonly IWebHostEnvironment _env;

        public QuanLyHoSoController(CongDichVuSinhVienContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // Danh sách hồ sơ
        [HttpGet("")]
        [HttpGet("Index")]
        [HasPermission("Applications.View")]
        public async Task<IActionResult> Index(string? trangThai = null, string? tuKhoa = null, int trang = 1, int kichThuocTrang = 10)
        {
            var query = _context.HoSoDichVu
                .Include(h => h.MaDichVuNavigation)
                .Include(h => h.MaSinhVienNavigation)
                    .ThenInclude(s => s.MaNguoiDungNavigation)
                .Include(h => h.MaCanBoXuLyNavigation)
                    .ThenInclude(c => c!.MaNguoiDungNavigation)
                .AsQueryable();

            if (!string.IsNullOrEmpty(trangThai))
                query = query.Where(h => h.TrangThai == trangThai);

            if (!string.IsNullOrWhiteSpace(tuKhoa))
                query = query.Where(h => h.MaTraCuu.Contains(tuKhoa.Trim()) ||
                                         h.MaSinhVien.Contains(tuKhoa.Trim()) ||
                                         h.MaDichVuNavigation.TenDichVu.Contains(tuKhoa.Trim()));

            ViewBag.CountChoTiepNhan = await _context.HoSoDichVu.CountAsync(h => h.TrangThai == "Chờ tiếp nhận");
            ViewBag.CountDangXuLy = await _context.HoSoDichVu.CountAsync(h => h.TrangThai == "Đang xử lý");
            ViewBag.CountHoanThanh = await _context.HoSoDichVu.CountAsync(h => h.TrangThai == "Hoàn thành" || h.TrangThai == "Đã duyệt");
            ViewBag.CountTuChoi = await _context.HoSoDichVu.CountAsync(h => h.TrangThai == "Từ chối");

            // PHÂN TRANG
            int tongSoBanGhi = await query.CountAsync();
            int tongSoTrang = (int)Math.Ceiling((double)tongSoBanGhi / kichThuocTrang);
            if (tongSoTrang < 1) tongSoTrang = 1;

            ViewBag.TrangHienTai = trang;
            ViewBag.TongSoTrang = tongSoTrang;
            ViewBag.TongSoBanGhi = tongSoBanGhi;
            ViewBag.TrangThaiHienTai = trangThai;
            ViewBag.TuKhoa = tuKhoa;

            var list = await query.OrderByDescending(h => h.NgayNop)
                                  .Skip((trang - 1) * kichThuocTrang)
                                  .Take(kichThuocTrang)
                                  .ToListAsync();

            return View(list);
        }

        // Cán bộ nhận xử lý
        [HttpPost("TiepNhan/{id}")]
        [ValidateAntiForgeryToken]
        [HasPermission("Applications.Edit")]
        public async Task<IActionResult> TiepNhan(int id)
        {
            var hoSo = await _context.HoSoDichVu.FindAsync(id);
            if (hoSo == null) return NotFound();

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var maCanBo = User.FindFirst("MaDinhDanh")?.Value;

            hoSo.MaCanBoXuLy = maCanBo;
            hoSo.TrangThai = "Đang xử lý";

            _context.LichSuXuLy.Add(new LichSuXuLy
            {
                MaHoSo = hoSo.MaHoSo,
                MaNguoiThucHien = userId,
                TrangThai = "Đang xử lý",
                NoiDung = "Cán bộ đã tiếp nhận và đang tiến hành thẩm định hồ sơ.",
                NgayThucHien = DateTime.Now
            });

            await _context.SaveChangesAsync();
            TempData["Success"] = $"Đã tiếp nhận hồ sơ {hoSo.MaTraCuu} thành công!";
            return RedirectToAction("Index");
        }

        // Phê duyệt và trả kết quả
        [HttpPost("PheDuyet")]
        [ValidateAntiForgeryToken]
        [HasPermission("Applications.Approve")]
        public async Task<IActionResult> PheDuyet(
            int maHoSo,
            string? maVanDon,
            string? donViVanChuyen,
            DateTime? ngayHenTra,
            string? ghiChu,
            IFormFile? fileKetQua = null)
        {
            var hoSo = await _context.HoSoDichVu.FindAsync(maHoSo);
            if (hoSo == null) return NotFound();

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            // 1. Xử lý lưu file kết quả ký số (nếu có tải lên)
            if (fileKetQua != null && fileKetQua.Length > 0)
            {
                string folder = Path.Combine(_env.WebRootPath, "uploads", "ketqua");
                if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

                string fileName = $"KQ_{hoSo.MaTraCuu}_{DateTime.Now:yyyyMMddHHmmss}{Path.GetExtension(fileKetQua.FileName)}";
                using (var stream = new FileStream(Path.Combine(folder, fileName), FileMode.Create))
                {
                    await fileKetQua.CopyToAsync(stream);
                }
                hoSo.TepKetQua = $"/uploads/ketqua/{fileName}";
            }

            // 2. Định dạng thông báo gửi sinh viên
            string thoiGianHen = ngayHenTra.HasValue
                ? ngayHenTra.Value.ToString("HH:mm - dd/MM/yyyy")
                : DateTime.Now.AddDays(1).ToString("08:30 - dd/MM/yyyy");

            string thongTinKetQua;
            if (!string.IsNullOrEmpty(maVanDon))
            {
                // Trường hợp gửi bưu điện
                thongTinKetQua = $"[GỬI BƯU ĐIỆN]\n- Mã vận đơn: {maVanDon}\n- Đơn vị: {donViVanChuyen ?? "Bưu điện Việt Nam"}\n- Dự kiến phát: {thoiGianHen}\n- Ghi chú: {ghiChu ?? "Đã bàn giao bưu cục phát."}";
            }
            else
            {
                // Trường hợp nhận tại quầy một cửa
                thongTinKetQua = $"[LỊCH HẸN TRẢ: {thoiGianHen}]\n{ghiChu ?? "Mời sinh viên mang theo Thẻ sinh viên đến phòng Một cửa nhận kết quả."}";
            }

            // 3. Cập nhật trạng thái hoàn thành
            hoSo.TrangThai = "Hoàn thành";
            hoSo.NgayHoanThanh = DateTime.Now;
            hoSo.GhiChuCanBo = thongTinKetQua;

            // 4. Ghi lịch sử xử lý
            _context.LichSuXuLy.Add(new LichSuXuLy
            {
                MaHoSo = hoSo.MaHoSo,
                MaNguoiThucHien = userId,
                TrangThai = "Hoàn thành",
                NoiDung = thongTinKetQua,
                NgayThucHien = DateTime.Now
            });

            await _context.SaveChangesAsync();
            TempData["Success"] = $"Đã phê duyệt hoàn thành hồ sơ {hoSo.MaTraCuu}!";
            return RedirectToAction("Index");
        }

        // Từ chối
        [HttpPost("TuChoi")]
        [ValidateAntiForgeryToken]
        [HasPermission("Applications.Reject")]
        public async Task<IActionResult> TuChoi(int maHoSo, string lyDoTuChoi)
        {
            var hoSo = await _context.HoSoDichVu.FindAsync(maHoSo);
            if (hoSo == null) return NotFound();

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            hoSo.TrangThai = "Từ chối";
            hoSo.GhiChuCanBo = lyDoTuChoi;

            _context.LichSuXuLy.Add(new LichSuXuLy
            {
                MaHoSo = hoSo.MaHoSo,
                MaNguoiThucHien = userId,
                TrangThai = "Từ chối",
                NoiDung = "Từ chối với lý do: " + lyDoTuChoi,
                NgayThucHien = DateTime.Now
            });

            await _context.SaveChangesAsync();
            TempData["Warning"] = $"Đã từ chối giải quyết hồ sơ {hoSo.MaTraCuu}!";
            return RedirectToAction("Index");
        }
    }
}