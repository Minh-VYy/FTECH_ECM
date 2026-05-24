using System.Web.Mvc;

namespace FTECH_THUONGMAIDIENTU.Areas.SuperAdmin.Controllers
{
    /// <summary>
    /// Super Admin - Quản lý toàn bộ hệ thống
    /// Chức năng: Dashboard, Duyệt bài viết, Duyệt đối tác, Quản lý tài khoản admin
    /// </summary>
    public class DashboardController : Controller
    {
        // GET: SuperAdmin/Dashboard
        public ActionResult Index()
        {
            ViewBag.Title = "Dashboard - Thống Kê Hệ Thống";
            ViewBag.UserRole = "Super Admin";
            return View();
        }
    }
}
