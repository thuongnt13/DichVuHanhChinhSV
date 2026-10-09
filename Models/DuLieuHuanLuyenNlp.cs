using System;
using System.Collections.Generic;

namespace CongDichVuSinhVien.Models;

public partial class DuLieuHuanLuyenNlp
{
    public int MaMau { get; set; }

    public string KyHieuDichVu { get; set; } = null!;

    public string CauHoiMau { get; set; } = null!;

    public virtual DichVu KyHieuDichVuNavigation { get; set; } = null!;
}
