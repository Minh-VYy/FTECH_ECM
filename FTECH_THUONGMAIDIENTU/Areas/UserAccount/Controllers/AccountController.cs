using System.Web.Mvc;

namespace FTECH_THUONGMAIDIENTU.Areas.UserAccount.Controllers
{
    /// <summary>
    /// User Account - Quản lý tài khoản người dùng bình thường
    /// Chức năng: Xem trang cá nhân, Quản lý thông tin cá nhân
    /// </summary>
    public class AccountController : Controller
    {
        // GET: UserAccount/Account/Dashboard
        public ActionResult Dashboard()
        {
            ViewBag.Title = "Trang Cá Nhân";
            ViewBag.UserRole = "User";
            return View("Profile");
        }

        // GET: UserAccount/Account/Profile
        public ActionResult Profile()
        {
            ViewBag.Title = "Quản Lý Thông Tin Cá Nhân";
            return View();
        }

        // GET: UserAccount/Account/Edit
        public ActionResult Edit()
        {
            ViewBag.Title = "Chỉnh Sửa Thông Tin";
            return View();
        }

        // POST: UserAccount/Account/Edit
        [HttpPost]
        public ActionResult Edit(FormCollection collection)
        {
            // Logic cập nhật thông tin user
            return RedirectToAction("Profile");
        }
    }
}
