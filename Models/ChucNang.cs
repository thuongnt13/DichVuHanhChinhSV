using System;
using System.Collections.Generic;

namespace CongDichVuSinhVien.Models;

public partial class ChucNang
{
    public int MaChucNang { get; set; }

    public string Module { get; set; } = null!;

    public string ActionName { get; set; } = null!;

    public string Code { get; set; } = null!;

    public virtual ICollection<Quyen> MaQuyen { get; set; } = new List<Quyen>();
}
