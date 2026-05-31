using System.Web.Mvc;
using FTECH_THUONGMAIDIENTU.Data;
using FTECH_THUONGMAIDIENTU.Infrastructure;
using FTECH_THUONGMAIDIENTU.Models.Auth;
using FTECH_THUONGMAIDIENTU.Models.UserAccount;

namespace FTECH_THUONGMAIDIENTU.Controllers
{
    public class AccountController : Controller
    {
        private readonly AccountRepository accountRepository = new AccountRepository();
        private readonly MemberRepository memberRepository = new MemberRepository();

        public ActionResult Login()
        {
            ViewBag.Title = "Cổng đăng nhập phân quyền";
            return View();
        }

        [HttpPost]
        public JsonResult Login(AccountAuthRequest request)
        {
            var result = accountRepository.AuthenticatePublicAccount(request);
            if (result.Success)
            {
                Session["CurrentUserName"] = result.DisplayName;
                Session["CurrentUserEmail"] = result.Email;
                Session["CurrentUserRole"] = result.RoleKey;
                result.RedirectUrl = ResolveRedirectUrl(result.RoleKey);
            }

            return Json(ToJson(result));
        }

        public ActionResult Register()
        {
            ViewBag.Title = "Đăng ký tài khoản";
            return View();
        }

        [HttpPost]
        public JsonResult Register(RegisterMemberRequest request)
        {
            if (request == null || !request.TermsAccepted)
            {
                return Json(new
                {
                    success = false,
                    message = "Bạn phải đồng ý điều khoản để tiếp tục."
                });
            }

            if (string.IsNullOrWhiteSpace(request.Password) || request.Password != request.ConfirmPassword)
            {
                return Json(new
                {
                    success = false,
                    message = "Mật khẩu xác nhận không khớp."
                });
            }

            var result = accountRepository.RegisterMember(request);
            if (result.Success)
            {
                result.RedirectUrl = Url.Content("~/login.html");
            }

            return Json(ToJson(result));
        }

        public ActionResult ResetPassword()
        {
            ViewBag.Title = "Đặt lại mật khẩu";
            return View();
        }

        [HttpPost]
        public JsonResult ResetPassword(ResetPasswordRequest request)
        {
            if (request == null || request.NewPassword != request.ConfirmPassword)
            {
                return Json(new
                {
                    success = false,
                    message = "Mật khẩu xác nhận không khớp."
                });
            }

            var result = accountRepository.ResetPassword(request);
            if (result.Success)
            {
                result.RedirectUrl = Url.Content("~/login.html");
            }

            return Json(ToJson(result));
        }

        [SessionRoleAuthorize(SessionKey = "CurrentUserRole", AllowedRolesCsv = RoleKeys.Customer, LoginUrl = "/Account/Login")]
        public ActionResult Profile()
        {
            ViewBag.Title = "Tài khoản cá nhân";
            var email = Session["CurrentUserEmail"] as string;
            var profile = memberRepository.GetByEmail(email);
            ViewBag.UserName = profile.FullName;
            ViewBag.UserRole = "Khách Hàng";
            ViewBag.UserAvatar = profile.AvatarURL;
            return View("~/Areas/UserAccount/Views/Account/Profile.cshtml", profile);
        }

        private string ResolveRedirectUrl(string roleKey)
        {
            switch (roleKey)
            {
                case RoleKeys.SuperAdmin:
                    return Url.Action("Index", "Dashboard", new { area = "Admin" });
                case RoleKeys.ContentManager:
                    return Url.Action("Index", "Post", new { area = "ContentManager" });
                case RoleKeys.AffiliateManager:
                    return Url.Action("Index", "Dashboard", new { area = "AffiliateManager" });
                case RoleKeys.UserAccountManager:
                    return Url.Action("Index", "AdminAccount", new { area = "SuperAdmin" });
                case RoleKeys.Customer:
                default:
                    return Url.Action("Index", "Home");
            }
        }

        private static object ToJson(AuthResult result)
        {
            return new
            {
                success = result.Success,
                message = result.Message,
                redirectUrl = result.RedirectUrl,
                roleKey = result.RoleKey,
                displayName = result.DisplayName,
                email = result.Email
            };
        }
    }
}
