using CongDichVuSinhVien.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CongDichVuSinhVien.Controllers
{
    [Route("ThuTuc")]
    public class ThuTucController : Controller
    {
        private readonly CongDichVuSinhVienContext _context;

        public ThuTucController(CongDichVuSinhVienContext context)
        {
            _context = context;
        }

        // GET: /ThuTuc/ChiTiet/5
        [HttpGet("ChiTiet/{id}")]
        public async Task<IActionResult> ChiTiet(int id)
        {
            var dichVu = await _context.DichVu
                .Include(d => d.MaLinhVucNavigation)
                .FirstOrDefaultAsync(d => d.MaDichVu == id && d.KichHoat == true);

            if (dichVu == null)
            {
                return NotFound();
            }

            // Lấy các câu hỏi huấn luyện NLP mẫu của dịch vụ này
            ViewBag.CauHoiNLP = await _context.DuLieuHuanLuyenNlp
                .Where(n => n.KyHieuDichVu == dichVu.KyHieuDichVu)
                .ToListAsync();

            return View(dichVu);
        }
    }
}