using System;
using System.Collections.Generic;

namespace CongDichVuSinhVien.Models;

public partial class CanBo
{
    public string MaCanBo { get; set; } = null!;

    public int MaNguoiDung { get; set; }

    public string PhongBan { get; set; } = null!;

    public string? ChucVu { get; set; }

    public virtual ICollection<HoSoDichVu> HoSoDichVu { get; set; } = new List<HoSoDichVu>();

    public virtual NguoiDung MaNguoiDungNavigation { get; set; } = null!;
}
