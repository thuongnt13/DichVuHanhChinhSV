
namespace CongDichVuSinhVien.Models
{
    public class HomeIndexViewModel
    {
        // Dữ liệu danh mục
        public List<LinhVuc> DanhSachLinhVuc { get; set; } = new();
        public List<DichVu> DanhSachDichVu { get; set; } = new();

        // SỬA: Đổi NLP thành Nlp đúng theo file DuLieuHuanLuyenNlp.cs
        public List<DuLieuHuanLuyenNlp> CauHoiMauNLP { get; set; } = new();

        // Thống kê hệ thống
        public int TongSoDichVu { get; set; }
        public int TongSoLinhVuc { get; set; }
        public int TongHoSoDaTiepNhan { get; set; }
        public int TongHoSoDaHoanThanh { get; set; }

        // Kết quả tra cứu nhanh
        public HoSoDichVu? KetQuaTraCuu { get; set; }
        public string? MaTraCuuTimKiem { get; set; }
        public string? ThongBaoTraCuu { get; set; }
    }
}