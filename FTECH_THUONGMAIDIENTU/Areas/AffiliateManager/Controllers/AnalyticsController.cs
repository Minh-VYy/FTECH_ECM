using System.Web.Mvc;
using FTECH_THUONGMAIDIENTU.Infrastructure;

namespace FTECH_THUONGMAIDIENTU.Areas.AffiliateManager.Controllers
{
    /// <summary>
    /// Theo dõi click và hiệu suất affiliate
    /// </summary>
    [SessionRoleAuthorize(SessionKey = "AdminRole", AllowedRolesCsv = RoleKeys.SuperAdmin + "," + RoleKeys.AffiliateManager, LoginUrl = "/Admin/Account/Login")]
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
