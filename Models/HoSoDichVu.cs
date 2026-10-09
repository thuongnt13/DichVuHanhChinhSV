using System;
using System.Collections.Generic;

namespace CongDichVuSinhVien.Models;

public partial class HoSoDichVu
{
    public int MaHoSo { get; set; }

    public string MaTraCuu { get; set; } = null!;

    public int MaDichVu { get; set; }

    public string MaSinhVien { get; set; } = null!;

    public string? MaCanBoXuLy { get; set; }

    public string LyDoXin { get; set; } = null!;

    public string? TepDinhKem { get; set; }

    public string? TrangThai { get; set; }

    public string? TepKetQua { get; set; }

    public string? GhiChuCanBo { get; set; }

    public DateTime? NgayNop { get; set; }

    public DateTime? NgayHoanThanh { get; set; }

    public virtual ICollection<LichSuXuLy> LichSuXuLy { get; set; } = new List<LichSuXuLy>();

    public virtual CanBo? MaCanBoXuLyNavigation { get; set; }

    public virtual DichVu MaDichVuNavigation { get; set; } = null!;

    public virtual SinhVien MaSinhVienNavigation { get; set; } = null!;
}
