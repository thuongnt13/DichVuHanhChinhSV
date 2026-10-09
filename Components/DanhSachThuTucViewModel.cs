using CongDichVuSinhVien.Data;
using CongDichVuSinhVien.Models;
using CongDichVuSinhVien.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CongDichVuSinhVien.Components
{
    public class DanhSachThuTucViewModel
    {
        public List<LinhVuc> LinhVucs { get; set; } = new();
        public List<DichVu> DanhSachDichVu { get; set; } = new();
        public List<NlpSearchResult> KetQuaNlp { get; set; } = new();
        public string? TuKhoa { get; set; }
        public int? MaLinhVucHienTai { get; set; }
        public bool IsNlpSearch => !string.IsNullOrWhiteSpace(TuKhoa);

        // THUỘC TÍNH PHÂN TRANG
        public int TrangHienTai { get; set; } = 1;
        public int TongSoTrang { get; set; } = 1;
        public int TongSoBanGhi { get; set; } = 0;
        public int KichThuocTrang { get; set; } = 6; // 6 thủ tục / trang (2 hàng x 3 cột)
        public bool CoTrangTruoc => TrangHienTai > 1;
        public bool CoTrangSau => TrangHienTai < TongSoTrang;
    }

    public class DanhSachThuTucViewComponent : ViewComponent
    {
        private readonly CongDichVuSinhVienContext _context;
        private readonly INlpService _nlpService;

        public DanhSachThuTucViewComponent(CongDichVuSinhVienContext context, INlpService nlpService)
        {
            _context = context;
            _nlpService = nlpService;
        }

        public async Task<IViewComponentResult> InvokeAsync(string? tuKhoa = null, int? maLinhVuc = null, int trang = 1, int kichThuocTrang = 6)
        {
            if (trang < 1) trang = 1;

            var model = new DanhSachThuTucViewModel
            {
                TuKhoa = tuKhoa,
                MaLinhVucHienTai = maLinhVuc,
                TrangHienTai = trang,
                KichThuocTrang = kichThuocTrang,
                LinhVucs = await _context.LinhVuc.ToListAsync()
            };

            // TRƯỜNG HỢP 1: TÌM KIẾM THEO NGỮ NGHĨA NLP
            if (!string.IsNullOrWhiteSpace(tuKhoa))
            {
                var allNlp = await _nlpService.PhanTichNhuCauSinhVienAsync(tuKhoa.Trim());
                model.TongSoBanGhi = allNlp.Count;
                model.TongSoTrang = (int)Math.Ceiling((double)model.TongSoBanGhi / kichThuocTrang);
                if (model.TongSoTrang == 0) model.TongSoTrang = 1;

                // Phân trang danh sách kết quả NLP
                model.KetQuaNlp = allNlp
                    .Skip((trang - 1) * kichThuocTrang)
                    .Take(kichThuocTrang)
                    .ToList();

                model.DanhSachDichVu = model.KetQuaNlp.Select(k => k.DichVu).ToList();
            }
            // TRƯỜNG HỢP 2: DUYỆT DANH MỤC THỦ TỤC THƯỜNG (CÓ LỌC LĨNH VỰC)
            else
            {
                var query = _context.DichVu.Where(d => d.KichHoat == true).AsQueryable();

                if (maLinhVuc.HasValue && maLinhVuc > 0)
                {
                    query = query.Where(d => d.MaLinhVuc == maLinhVuc.Value);
                }

                model.TongSoBanGhi = await query.CountAsync();
                model.TongSoTrang = (int)Math.Ceiling((double)model.TongSoBanGhi / kichThuocTrang);
                if (model.TongSoTrang == 0) model.TongSoTrang = 1;

                // Phân trang trực tiếp từ SQL Server
                model.DanhSachDichVu = await query
                    .OrderBy(d => d.MaDichVu)
                    .Skip((trang - 1) * kichThuocTrang)
                    .Take(kichThuocTrang)
                    .ToListAsync();
            }

            return View(model);
        }
    }
}