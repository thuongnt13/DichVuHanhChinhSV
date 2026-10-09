using System.ComponentModel.DataAnnotations;

namespace CongDichVuSinhVien.Models
{
    public class NopHoSoViewModel
    {
        // Thông tin thủ tục
        public int MaDichVu { get; set; }
        public string TenDichVu { get; set; } = string.Empty;
        public string KyHieuDichVu { get; set; } = string.Empty;
        public string DonViXuLy { get; set; } = string.Empty;
        public decimal? LePhi { get; set; }
        public int? ThoiGianXuLyNgay { get; set; }
        public string? ThanhPhanHoSo { get; set; }

        // Thông tin sinh viên (tự động điền)
        public string MaSinhVien { get; set; } = string.Empty;
        public string HoTen { get; set; } = string.Empty;
        public string LopHoc { get; set; } = string.Empty;
        public string KhoaVien { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? SoDienThoai { get; set; }

        // 1. HÌNH THỨC NHẬN KẾT QUẢ BẢN CỨNG
        [Required(ErrorMessage = "Vui lòng chọn hình thức nhận kết quả")]
        public string HinhThucNhan { get; set; } = "TrucTiep"; // "TrucTiep" hoặc "BuuDien"

        // 2. THÔNG TIN GIAO HÀNG BƯU ĐIỆN (NẾU CHỌN GỬI CHUYỂN PHÁT NHANH)
        public string? HoTenNguoiNhan { get; set; }
        public string? SoDienThoaiNhan { get; set; }
        public string? DiaChiNhan { get; set; }
        public string? GhiChuGiaoHang { get; set; }

        // Nội dung đề nghị & File minh chứng
        [Required(ErrorMessage = "Vui lòng nhập lý do / nội dung đề nghị")]
        public string LyDoXin { get; set; } = string.Empty;

        public IFormFile? FileDinhKem { get; set; }
    }
}