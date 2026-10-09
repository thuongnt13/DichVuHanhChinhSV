using System.Collections.Generic;
using CongDichVuSinhVien.Models;
using Microsoft.EntityFrameworkCore;

namespace CongDichVuSinhVien.Data;

public partial class CongDichVuSinhVienContext : DbContext
{
    public CongDichVuSinhVienContext()
    {
    }

    public CongDichVuSinhVienContext(DbContextOptions<CongDichVuSinhVienContext> options)
        : base(options)
    {
    }

    public virtual DbSet<CanBo> CanBo { get; set; }

    public virtual DbSet<ChucNang> ChucNang { get; set; }

    public virtual DbSet<DichVu> DichVu { get; set; }

    public virtual DbSet<DuLieuHuanLuyenNlp> DuLieuHuanLuyenNlp { get; set; }

    public virtual DbSet<HoSoDichVu> HoSoDichVu { get; set; }

    public virtual DbSet<LichSuXuLy> LichSuXuLy { get; set; }

    public virtual DbSet<LinhVuc> LinhVuc { get; set; }

    public virtual DbSet<NguoiDung> NguoiDung { get; set; }

    public virtual DbSet<PhanQuyenNguoiDung> PhanQuyenNguoiDung { get; set; }

    public virtual DbSet<Quyen> Quyen { get; set; }

    public virtual DbSet<SinhVien> SinhVien { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer("Server=DESKTOP-5H0MUCA\\SQLEXPRESS;Database=CongDichVuSinhVienNLP;Trusted_Connection=True;TrustServerCertificate=True;");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CanBo>(entity =>
        {
            entity.HasKey(e => e.MaCanBo).HasName("PK__CanBo__4003E215E25C53B4");

            entity.HasIndex(e => e.MaNguoiDung, "UQ__CanBo__C539D7638ABC0EFE").IsUnique();

            entity.Property(e => e.MaCanBo)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.ChucVu).HasMaxLength(100);
            entity.Property(e => e.PhongBan).HasMaxLength(150);

            entity.HasOne(d => d.MaNguoiDungNavigation).WithOne(p => p.CanBo)
                .HasForeignKey<CanBo>(d => d.MaNguoiDung)
                .HasConstraintName("FK__CanBo__MaNguoiDu__6383C8BA");
        });

        modelBuilder.Entity<ChucNang>(entity =>
        {
            entity.HasKey(e => e.MaChucNang).HasName("PK__ChucNang__B26DC25704CD92BB");

            entity.HasIndex(e => e.Code, "UQ__ChucNang__A25C5AA7280563BA").IsUnique();

            entity.Property(e => e.ActionName).HasMaxLength(50);
            entity.Property(e => e.Code)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Module).HasMaxLength(50);
        });

        modelBuilder.Entity<DichVu>(entity =>
        {
            entity.HasKey(e => e.MaDichVu).HasName("PK__DichVu__C0E6DE8F03E46B1F");

            entity.HasIndex(e => e.KyHieuDichVu, "UQ__DichVu__AC696727A721E10B").IsUnique();

            entity.Property(e => e.DonViXuLy).HasMaxLength(150);
            entity.Property(e => e.KichHoat).HasDefaultValue(true);
            entity.Property(e => e.KyHieuDichVu)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.LePhi)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(18, 2)");
            entity.Property(e => e.TenDichVu).HasMaxLength(250);
            entity.Property(e => e.ThoiGianXuLyNgay).HasDefaultValue(2);

            entity.HasOne(d => d.MaLinhVucNavigation).WithMany(p => p.DichVu)
                .HasForeignKey(d => d.MaLinhVuc)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__DichVu__MaLinhVu__693CA210");
        });

        modelBuilder.Entity<DuLieuHuanLuyenNlp>(entity =>
        {
            entity.HasKey(e => e.MaMau).HasName("PK__DuLieuHu__3A5BBB7D9E3DF57C");

            entity.ToTable("DuLieuHuanLuyenNLP");

            entity.Property(e => e.CauHoiMau).HasMaxLength(500);
            entity.Property(e => e.KyHieuDichVu)
                .HasMaxLength(30)
                .IsUnicode(false);

            entity.HasOne(d => d.KyHieuDichVuNavigation).WithMany(p => p.DuLieuHuanLuyenNlp)
                .HasPrincipalKey(p => p.KyHieuDichVu)
                .HasForeignKey(d => d.KyHieuDichVu)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__DuLieuHua__KyHie__6EF57B66");
        });

        modelBuilder.Entity<HoSoDichVu>(entity =>
        {
            entity.HasKey(e => e.MaHoSo).HasName("PK__HoSoDich__1666423C68A80CE9");

            entity.HasIndex(e => e.MaTraCuu, "UQ__HoSoDich__2105AFD6A780EA12").IsUnique();

            entity.Property(e => e.MaCanBoXuLy)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.MaSinhVien)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.MaTraCuu)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.NgayHoanThanh).HasColumnType("datetime");
            entity.Property(e => e.NgayNop)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.TepDinhKem).HasMaxLength(500);
            entity.Property(e => e.TepKetQua).HasMaxLength(500);
            entity.Property(e => e.TrangThai)
                .HasMaxLength(50)
                .HasDefaultValue("Chờ tiếp nhận");

            entity.HasOne(d => d.MaCanBoXuLyNavigation).WithMany(p => p.HoSoDichVu)
                .HasForeignKey(d => d.MaCanBoXuLy)
                .HasConstraintName("FK__HoSoDichV__MaCan__74AE54BC");

            entity.HasOne(d => d.MaDichVuNavigation).WithMany(p => p.HoSoDichVu)
                .HasForeignKey(d => d.MaDichVu)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__HoSoDichV__MaDic__72C60C4A");

            entity.HasOne(d => d.MaSinhVienNavigation).WithMany(p => p.HoSoDichVu)
                .HasForeignKey(d => d.MaSinhVien)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__HoSoDichV__MaSin__73BA3083");
        });

        modelBuilder.Entity<LichSuXuLy>(entity =>
        {
            entity.HasKey(e => e.MaLichSu).HasName("PK__LichSuXu__C443222AD287206F");

            entity.Property(e => e.NgayThucHien)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.NoiDung).HasMaxLength(255);
            entity.Property(e => e.TrangThai).HasMaxLength(50);

            entity.HasOne(d => d.MaHoSoNavigation).WithMany(p => p.LichSuXuLy)
                .HasForeignKey(d => d.MaHoSo)
                .HasConstraintName("FK__LichSuXuL__MaHoS__797309D9");

            entity.HasOne(d => d.MaNguoiThucHienNavigation).WithMany(p => p.LichSuXuLy)
                .HasForeignKey(d => d.MaNguoiThucHien)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__LichSuXuL__MaNgu__7A672E12");
        });

        modelBuilder.Entity<LinhVuc>(entity =>
        {
            entity.HasKey(e => e.MaLinhVuc).HasName("PK__LinhVuc__318894A049DB2672");

            entity.Property(e => e.MoTa).HasMaxLength(255);
            entity.Property(e => e.TenLinhVuc).HasMaxLength(150);
        });

        modelBuilder.Entity<NguoiDung>(entity =>
        {
            entity.HasKey(e => e.MaNguoiDung).HasName("PK__NguoiDun__C539D762A420B2A5");

            entity.HasIndex(e => e.TenDangNhap, "UQ__NguoiDun__55F68FC05FFDE8E8").IsUnique();

            entity.Property(e => e.Email)
                .HasMaxLength(150)
                .IsUnicode(false);
            entity.Property(e => e.HoTen).HasMaxLength(150);
            entity.Property(e => e.MatKhau)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.NgayTao)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.SoDienThoai)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.TenDangNhap)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.TrangThai).HasDefaultValue(true);
        });

        modelBuilder.Entity<PhanQuyenNguoiDung>(entity =>
        {
            entity.HasKey(e => e.MaPhanQuyen).HasName("PK__PhanQuye__529AB12BEEB2F2E3");

            entity.Property(e => e.NgayCap)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.MaNguoiDungNavigation).WithMany(p => p.PhanQuyenNguoiDung)
                .HasForeignKey(d => d.MaNguoiDung)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK__PhanQuyen__MaNgu__59063A47");

            entity.HasOne(d => d.MaQuyenNavigation).WithMany(p => p.PhanQuyenNguoiDung)
                .HasForeignKey(d => d.MaQuyen)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK__PhanQuyen__MaQuy__59FA5E80");
        });

        modelBuilder.Entity<Quyen>(entity =>
        {
            entity.HasKey(e => e.MaQuyen).HasName("PK__Quyen__1D4B7ED4D51643B8");

            entity.HasIndex(e => e.TenQuyen, "UQ__Quyen__5637EE7985687E26").IsUnique();

            entity.Property(e => e.MoTa).HasMaxLength(255);
            entity.Property(e => e.TenHienThi).HasMaxLength(100);
            entity.Property(e => e.TenQuyen)
                .HasMaxLength(30)
                .IsUnicode(false);

            entity.HasMany(d => d.MaChucNang).WithMany(p => p.MaQuyen)
                .UsingEntity<Dictionary<string, object>>(
                    "QuyenChucNang",
                    r => r.HasOne<ChucNang>().WithMany()
                        .HasForeignKey("MaChucNang")
                        .HasConstraintName("FK__QuyenChuc__MaChu__02084FDA"),
                    l => l.HasOne<Quyen>().WithMany()
                        .HasForeignKey("MaQuyen")
                        .HasConstraintName("FK__QuyenChuc__MaQuy__01142BA1"),
                    j =>
                    {
                        j.HasKey("MaQuyen", "MaChucNang").HasName("PK__QuyenChu__766DA2F1DD8CDAE1");
                    });
        });

        modelBuilder.Entity<SinhVien>(entity =>
        {
            entity.HasKey(e => e.MaSinhVien).HasName("PK__SinhVien__939AE7757B8316CA");

            entity.HasIndex(e => e.MaNguoiDung, "UQ__SinhVien__C539D7639C1EF19F").IsUnique();

            entity.Property(e => e.MaSinhVien)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.KhoaVien).HasMaxLength(150);
            entity.Property(e => e.LopHoc)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.NienKhoa)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.TinhTrangHocTap)
                .HasMaxLength(50)
                .HasDefaultValue("Đang học");

            entity.HasOne(d => d.MaNguoiDungNavigation).WithOne(p => p.SinhVien)
                .HasForeignKey<SinhVien>(d => d.MaNguoiDung)
                .HasConstraintName("FK__SinhVien__MaNguo__5EBF139D");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
