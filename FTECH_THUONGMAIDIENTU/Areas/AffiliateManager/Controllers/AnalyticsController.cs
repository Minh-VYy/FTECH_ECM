using System.Web.Mvc;

namespace FTECH_THUONGMAIDIENTU.Areas.AffiliateManager.Controllers
{
    /// <summary>
    /// Theo dõi click và hiệu suất affiliate
    /// </summary>
    public class AnalyticsController : Controller
    {
        // GET: AffiliateManager/Analytics
        public ActionResult Index()
        {
            ViewBag.Title = "Theo Dõi Click Affiliate";
            return View();
        }

        // GET: AffiliateManager/Analytics/Performance
        public ActionResult Performance()
        {
            ViewBag.Title = "Xem Thông Kê Hiệu Suất";
            return View();
        }
    }
}
