using CongDichVuSinhVien.Data;
using CongDichVuSinhVien.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 1. Kết nối CSDL SQL Server
builder.Services.AddDbContext<CongDichVuSinhVienContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2. ĐĂNG KÝ NLP SERVICE TẠI ĐÂY (HẾT LỖI RESOLVE SERVICE)
builder.Services.AddScoped<INlpService, NlpService>();

// 3. Cấu hình Cookie Authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/TaiKhoan/DangNhap";           // URL chuyển hướng khi chưa đăng nhập
        options.LogoutPath = "/TaiKhoan/DangXuat";
        options.AccessDeniedPath = "/TaiKhoan/TuChoiTruyCap"; // URL khi không đủ quyền
        options.ExpireTimeSpan = TimeSpan.FromHours(8);       // Thời gian lưu đăng nhập (8 tiếng)
        options.SlidingExpiration = true;
    });

builder.Services.AddControllersWithViews();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Bắt buộc: UseAuthentication PHẢI đứng trước UseAuthorization
app.UseAuthentication();
app.UseAuthorization();
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=TaiKhoan}/{action=DangNhap}/{id?}");

app.Run();