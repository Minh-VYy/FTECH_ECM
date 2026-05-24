using System.Web.Mvc;

namespace FTECH_THUONGMAIDIENTU.Controllers
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
            return View();
        }

        public ActionResult ResetPassword()
        {
            ViewBag.Title = "Đặt lại mật khẩu";
            return View();
        }

        public ActionResult Profile()
        {
            ViewBag.Title = "Tài khoản cá nhân";
            return View();
        }
    }
}
