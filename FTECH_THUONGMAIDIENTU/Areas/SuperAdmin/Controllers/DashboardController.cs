using System;
using System.Web.Mvc;
using FTECH_THUONGMAIDIENTU.Data;
using FTECH_THUONGMAIDIENTU.Infrastructure;

namespace FTECH_THUONGMAIDIENTU.Areas.SuperAdmin.Controllers
{
    /// <summary>
    /// Super Admin - Quản lý toàn bộ hệ thống
    /// Chức năng: Dashboard, Duyệt bài viết, Duyệt đối tác, Quản lý tài khoản admin
    /// </summary>
    [SessionRoleAuthorize(SessionKey = "AdminRole", AllowedRolesCsv = RoleKeys.SuperAdmin, LoginUrl = "/Admin/Account/Login")]
    public class DashboardController : Controller
    {
        private readonly StatisticsRepository statisticsRepository = new StatisticsRepository();

        // GET: SuperAdmin/Dashboard
        public ActionResult Index()
        {
            ViewBag.Title = "Dashboard - Thống Kê Hệ Thống";
            var adminName = Session["AdminName"] as string;
            ViewBag.AdminName = string.IsNullOrWhiteSpace(adminName) ? "Super Admin" : adminName;
            ViewBag.UserName = ViewBag.AdminName;
            ViewBag.UserRole = "Super Admin";
            ViewBag.Summary = statisticsRepository.GetSummary();
            return View();
        }
    }
}
