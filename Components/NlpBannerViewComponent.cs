using CongDichVuSinhVien.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CongDichVuSinhVien.Components
{
    public class NlpBannerViewComponent : ViewComponent
    {
        private readonly CongDichVuSinhVienContext _context;

        public NlpBannerViewComponent(CongDichVuSinhVienContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync(string? tuKhoa = null)
        {
            ViewBag.TuKhoaHienTai = tuKhoa;
            // Lấy câu hỏi huấn luyện mẫu từ CSDL
            var cauHoiMau = await _context.DuLieuHuanLuyenNlp.Take(6).ToListAsync();
            return View(cauHoiMau);
        }
    }
}