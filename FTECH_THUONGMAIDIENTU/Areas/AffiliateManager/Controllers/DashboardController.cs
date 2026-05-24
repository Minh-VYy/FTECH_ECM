using System.Web.Mvc;

namespace FTECH_THUONGMAIDIENTU.Areas.AffiliateManager.Controllers
{
    /// <summary>
    /// Affiliate Manager - Quản lý các chương trình liên kết
    /// Chức năng: Quản lý affiliate link, Theo dõi click, Xem thông kê hiệu suất
    /// </summary>
    public class DashboardController : Controller
    {
        // GET: AffiliateManager/Dashboard
        public ActionResult Index()
        {
            ViewBag.Title = "Dashboard - Quản Lý Affiliate";
            ViewBag.UserRole = "Affiliate Manager";
            return View();
        }
    }
}
