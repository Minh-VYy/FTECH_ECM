using System;
using System.Web.Mvc;
using FTECH_THUONGMAIDIENTU.Data;
using FTECH_THUONGMAIDIENTU.Infrastructure;

namespace FTECH_THUONGMAIDIENTU.Areas.AffiliateManager.Controllers
{
    /// <summary>
    /// Affiliate Manager - Quản lý các chương trình liên kết
    /// Chức năng: Quản lý affiliate link, Theo dõi click, Xem thông kê hiệu suất
    /// </summary>
    [SessionRoleAuthorize(SessionKey = "AdminRole", AllowedRolesCsv = RoleKeys.SuperAdmin + "," + RoleKeys.AffiliateManager, LoginUrl = "/Admin/Account/Login")]
    public class DashboardController : Controller
    {
        private readonly StatisticsRepository statisticsRepository = new StatisticsRepository();

        // GET: AffiliateManager/Dashboard
        public ActionResult Index()
        {
            ViewBag.Title = "Dashboard - Quản Lý Affiliate";
            var managerName = Session["AdminName"] as string;
            ViewBag.UserName = string.IsNullOrWhiteSpace(managerName) ? "Affiliate Manager" : managerName;
            ViewBag.UserRole = "Affiliate Manager";
            ViewBag.Summary = statisticsRepository.GetSummary();
            return View();
        }
    }
}
