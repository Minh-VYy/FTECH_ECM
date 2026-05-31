using System.Web.Mvc;
using FTECH_THUONGMAIDIENTU.Data;

namespace FTECH_THUONGMAIDIENTU.Areas.AffiliateManager.Controllers
{
    /// <summary>
    /// Affiliate Manager - Quản lý các chương trình liên kết
    /// Chức năng: Quản lý affiliate link, Theo dõi click, Xem thông kê hiệu suất
    /// </summary>
    public class DashboardController : Controller
    {
        private readonly StatisticsRepository statisticsRepository = new StatisticsRepository();

        // GET: AffiliateManager/Dashboard
        public ActionResult Index()
        {
            ViewBag.Title = "Dashboard - Quản Lý Affiliate";
            ViewBag.UserRole = "Affiliate Manager";
            ViewBag.Summary = statisticsRepository.GetSummary();
            return View();
        }
    }
}
