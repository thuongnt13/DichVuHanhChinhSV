using System;
using System.Collections.Generic;

namespace CongDichVuSinhVien.Models;

public partial class SinhVien
{
    public string MaSinhVien { get; set; } = null!;

    public int MaNguoiDung { get; set; }

    public string LopHoc { get; set; } = null!;

    public string KhoaVien { get; set; } = null!;

    public string? NienKhoa { get; set; }

    public string? TinhTrangHocTap { get; set; }

    public virtual ICollection<HoSoDichVu> HoSoDichVu { get; set; } = new List<HoSoDichVu>();

    public virtual NguoiDung MaNguoiDungNavigation { get; set; } = null!;
}
