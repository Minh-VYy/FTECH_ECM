using System;
using System.Web.Mvc;
using FTECH_THUONGMAIDIENTU.Data;
using FTECH_THUONGMAIDIENTU.Infrastructure;

namespace FTECH_THUONGMAIDIENTU.Areas.Admin.Controllers
{
    [SessionRoleAuthorize(SessionKey = "AdminRole", AllowedRolesCsv = RoleKeys.SuperAdmin, LoginUrl = "/Admin/Account/Login")]
    public class DashboardController : Controller
    {
        private readonly StatisticsRepository statisticsRepository = new StatisticsRepository();

        public ActionResult Index()
        {
            ViewBag.Title = "Dashboard Thống Kê";
            var adminName = Session["AdminName"] as string;
            ViewBag.AdminName = string.IsNullOrWhiteSpace(adminName) ? "Super Admin" : adminName;
            ViewBag.UserName = ViewBag.AdminName;
            ViewBag.Summary = statisticsRepository.GetSummary();
            return View();
        }
    }
}
