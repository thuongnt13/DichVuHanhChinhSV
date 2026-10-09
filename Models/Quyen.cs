using System;
using System.Collections.Generic;

namespace CongDichVuSinhVien.Models;

public partial class Quyen
{
    public int MaQuyen { get; set; }

    public string TenQuyen { get; set; } = null!;

    public string TenHienThi { get; set; } = null!;

    public string? MoTa { get; set; }

    public virtual ICollection<PhanQuyenNguoiDung> PhanQuyenNguoiDung { get; set; } = new List<PhanQuyenNguoiDung>();

    public virtual ICollection<ChucNang> MaChucNang { get; set; } = new List<ChucNang>();
}
