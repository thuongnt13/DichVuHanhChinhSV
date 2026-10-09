using System;
using System.Collections.Generic;

namespace CongDichVuSinhVien.Models;

public partial class NguoiDung
{
    public int MaNguoiDung { get; set; }

    public string TenDangNhap { get; set; } = null!;

    public string MatKhau { get; set; } = null!;

    public string HoTen { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string? SoDienThoai { get; set; }

    public bool? TrangThai { get; set; }

    public DateTime? NgayTao { get; set; }

    public virtual CanBo? CanBo { get; set; }

    public virtual ICollection<LichSuXuLy> LichSuXuLy { get; set; } = new List<LichSuXuLy>();

    public virtual ICollection<PhanQuyenNguoiDung> PhanQuyenNguoiDung { get; set; } = new List<PhanQuyenNguoiDung>();

    public virtual SinhVien? SinhVien { get; set; }
}
