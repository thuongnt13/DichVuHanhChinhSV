using System;
using System.Collections.Generic;

namespace CongDichVuSinhVien.Models;

public partial class DichVu
{
    public int MaDichVu { get; set; }

    public int MaLinhVuc { get; set; }

    public string KyHieuDichVu { get; set; } = null!;

    public string TenDichVu { get; set; } = null!;

    public string? MoTa { get; set; }

    public string? ThanhPhanHoSo { get; set; }

    public int? ThoiGianXuLyNgay { get; set; }

    public decimal? LePhi { get; set; }

    public string DonViXuLy { get; set; } = null!;

    public bool? KichHoat { get; set; }

    public virtual ICollection<DuLieuHuanLuyenNlp> DuLieuHuanLuyenNlp { get; set; } = new List<DuLieuHuanLuyenNlp>();

    public virtual ICollection<HoSoDichVu> HoSoDichVu { get; set; } = new List<HoSoDichVu>();

    public virtual LinhVuc MaLinhVucNavigation { get; set; } = null!;
}
