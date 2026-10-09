using System;
using System.Collections.Generic;

namespace CongDichVuSinhVien.Models;

public partial class PhanQuyenNguoiDung
{
    public int MaPhanQuyen { get; set; }

    public int? MaNguoiDung { get; set; }

    public int? MaQuyen { get; set; }

    public DateTime? NgayCap { get; set; }

    public virtual NguoiDung? MaNguoiDungNavigation { get; set; }

    public virtual Quyen? MaQuyenNavigation { get; set; }
}
