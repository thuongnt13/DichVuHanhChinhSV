using CongDichVuSinhVien.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CongDichVuSinhVien.Components
{
    public class ThongKeViewModel
    {
        public int TongDichVu { get; set; }
        public int TongLinhVuc { get; set; }
        public int TongHoSoNop { get; set; }
        public int TongHoSoXong { get; set; }
        public bool IsSinhVien { get; set; }
    }

    public class ThongKeHeThongViewComponent : ViewComponent
    {
        private readonly CongDichVuSinhVienContext _context;

        public ThongKeHeThongViewComponent(CongDichVuSinhVienContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var user = HttpContext.User;

            // 1. KIỂM TRA QUYỀN CHÍNH XÁC: Cán bộ hay Sinh viên
            bool isCanBoOrAdmin = user.IsInRole("ADMIN") || user.IsInRole("QuanTri")
                               || user.IsInRole("CAN_BO") || user.IsInRole("CanBo");

            // Nếu đã đăng nhập và không phải Cán bộ/Admin -> Bắt buộc là Sinh viên
            bool isSinhVien = user.Identity != null && user.Identity.IsAuthenticated && !isCanBoOrAdmin;

            int hoSoNop = 0;
            int hoSoXong = 0;

            // 2. NẾU LÀ SINH VIÊN: CHỈ ĐẾM ĐÚNG HỒ SƠ CỦA SINH VIÊN ĐANG ĐĂNG NHẬP
            if (isSinhVien)
            {
                // Tìm MSSV qua Claim hoặc qua CSDL
                string? mssv = user.FindFirst("MaDinhDanh")?.Value;

                var userIdStr = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (int.TryParse(userIdStr, out int maNguoiDung))
                {
                    var sv = await _context.SinhVien.FirstOrDefaultAsync(s => s.MaNguoiDung == maNguoiDung);
                    if (sv != null)
                    {
                        mssv = sv.MaSinhVien;
                    }
                }

                // Nếu có MSSV hợp lệ thì đếm hồ sơ của sinh viên này
                if (!string.IsNullOrEmpty(mssv))
                {
                    hoSoNop = await _context.HoSoDichVu
                        .CountAsync(h => h.MaSinhVien == mssv);

                    hoSoXong = await _context.HoSoDichVu
                        .CountAsync(h => h.MaSinhVien == mssv && (h.TrangThai == "Hoàn thành" || h.TrangThai == "Đã duyệt"));
                }
                else
                {
                    hoSoNop = 0;
                    hoSoXong = 0;
                }
            }
            // 3. NẾU LÀ CÁN BỘ / QUẢN TRỊ VIÊN: MỚI ĐƯỢC HIỂN THỊ TỔNG TOÀN TRƯỜNG
            else
            {
                hoSoNop = await _context.HoSoDichVu.CountAsync();
                hoSoXong = await _context.HoSoDichVu
                    .CountAsync(h => h.TrangThai == "Hoàn thành" || h.TrangThai == "Đã duyệt");
            }

            var model = new ThongKeViewModel
            {
                TongDichVu = await _context.DichVu.CountAsync(d => d.KichHoat == true),
                TongLinhVuc = await _context.LinhVuc.CountAsync(),
                TongHoSoNop = hoSoNop,
                TongHoSoXong = hoSoXong,
                IsSinhVien = isSinhVien
            };

            return View(model);
        }
    }
}