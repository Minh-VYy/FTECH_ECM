using System.Web.Mvc;
using FTECH_THUONGMAIDIENTU.Data;
using FTECH_THUONGMAIDIENTU.Models.Auth;

namespace FTECH_THUONGMAIDIENTU.Areas.Admin.Controllers
{
    public class AccountController : Controller
    {
        private readonly AccountRepository accountRepository = new AccountRepository();

        public ActionResult Login()
        {
            ViewBag.Title = "Cổng đăng nhập phân quyền";
            return View("~/Areas/Admin/Views/Auth/Login.cshtml");
        }

        [HttpPost]
        public JsonResult Login(AccountAuthRequest request)
        {
            var result = accountRepository.AuthenticateAdmin(request);
            if (result.Success)
            {
                Session["AdminName"] = result.DisplayName;
                Session["AdminEmail"] = result.Email;
                Session["AdminRole"] = result.RoleKey;
                result.RedirectUrl = Url.Action("Index", "Dashboard", new { area = "Admin" });
            }

            return Json(new
            {
                success = result.Success,
                message = result.Message,
                redirectUrl = result.RedirectUrl,
                roleKey = result.RoleKey,
                displayName = result.DisplayName,
                email = result.Email
            });
        }

        public ActionResult Register()
        {
            ViewBag.Title = "Đăng ký tài khoản";
            return View("~/Areas/Admin/Views/Auth/Register.cshtml");
        }

        public ActionResult ForgotPassword()
        {
            ViewBag.Title = "Quên mật khẩu";
            return View("~/Areas/Admin/Views/Auth/ResetPassword.cshtml");
        }

        public ActionResult Accounts()
        {
            ViewBag.Title = "Quản Lý Tài Khoản Admin";
            return View("Index");
        }
    }
}
