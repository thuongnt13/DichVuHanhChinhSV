using System.Security.Claims;

namespace CongDichVuSinhVien.Helpers
{
    public static class ClaimsPrincipalExtensions
    {
        public static bool HasPermission(this ClaimsPrincipal user, string permissionCode)
        {

            if (user?.Identity == null || !user.Identity.IsAuthenticated) return false;

            // Admin hệ thống mặc định có tất cả quyền
            if (user.IsInRole("ADMIN") || user.IsInRole("QuanTri")) return true;

            // Kiểm tra claim Permission được cấp
            return user.HasClaim("Permission", permissionCode);
        }
    }
}