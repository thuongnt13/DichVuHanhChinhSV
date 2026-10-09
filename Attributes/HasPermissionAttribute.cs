using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CongDichVuSinhVien.Attributes
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
    public class HasPermissionAttribute : Attribute, IAuthorizationFilter
    {
        private readonly string _permissionCode;

        public HasPermissionAttribute(string permissionCode)
        {
            _permissionCode = permissionCode;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var user = context.HttpContext.User;

            if (user.Identity == null || !user.Identity.IsAuthenticated)
            {
                context.Result = new RedirectToActionResult("DangNhap", "TaiKhoan", null);
                return;
            }

            // ADMIN có quyền tối cao
            if (user.IsInRole("ADMIN") || user.IsInRole("QuanTri"))
            {
                return;
            }

            // Kiểm tra mã Code chức năng trong Claims
            if (!user.HasClaim(c => c.Type == "Permission" && c.Value.Equals(_permissionCode, StringComparison.OrdinalIgnoreCase)))
            {
                context.Result = new RedirectToActionResult("TuChoiTruyCap", "TaiKhoan", null);
            }
        }
    }
}