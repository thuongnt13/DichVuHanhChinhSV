using System;
using System.Collections.Generic;

namespace CongDichVuSinhVien.Models;

public partial class LichSuXuLy
{
    public int MaLichSu { get; set; }

    public int MaHoSo { get; set; }

    public int MaNguoiThucHien { get; set; }

    public string TrangThai { get; set; } = null!;

    public string NoiDung { get; set; } = null!;

    public DateTime? NgayThucHien { get; set; }

    public virtual HoSoDichVu MaHoSoNavigation { get; set; } = null!;

    public virtual NguoiDung MaNguoiThucHienNavigation { get; set; } = null!;
}
