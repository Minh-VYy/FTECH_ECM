using System;
using System.Web.Mvc;

namespace FTECH_THUONGMAIDIENTU.Areas.Admin.Controllers
{
    public class AccountController : Controller
    {
        public ActionResult Login()
        {
            ViewBag.Title = "Cổng đăng nhập phân quyền";
            return View();
        }

        public ActionResult Register()
        {
            ViewBag.Title = "Đăng ký tài khoản";
            return View("~/Views/Account/Register.cshtml");
        }

        public ActionResult ForgotPassword()
        {
            ViewBag.Title = "Quên mật khẩu";
            return View("~/Views/Account/ResetPassword.cshtml");
        }

        public ActionResult Accounts()
        {
            ViewBag.Title = "Quản Lý Tài Khoản Admin";
            return View("Index");
        }
    }
}
