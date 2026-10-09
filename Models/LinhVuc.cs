using System;
using System.Collections.Generic;

namespace CongDichVuSinhVien.Models;

public partial class LinhVuc
{
    public int MaLinhVuc { get; set; }

    public string TenLinhVuc { get; set; } = null!;

    public string? MoTa { get; set; }

    public virtual ICollection<DichVu> DichVu { get; set; } = new List<DichVu>();
}
