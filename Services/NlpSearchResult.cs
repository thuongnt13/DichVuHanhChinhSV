using CongDichVuSinhVien.Models;

namespace CongDichVuSinhVien.Services
{
    // Class này chứa kết quả phân tích NLP
    public class NlpSearchResult
    {
        public DichVu DichVu { get; set; } = null!;
        public double DiemTuongDong { get; set; }
        public string CauHoiKhopNhat { get; set; } = string.Empty;
        public string LyDoGoiY { get; set; } = string.Empty;
    }

    public interface INlpService
    {
        Task<List<NlpSearchResult>> PhanTichNhuCauSinhVienAsync(string cauHoiNguoiDung);
    }
}