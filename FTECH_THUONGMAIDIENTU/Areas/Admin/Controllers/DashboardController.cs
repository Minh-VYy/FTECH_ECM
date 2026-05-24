using System;
using System.Web.Mvc;

namespace FTECH_THUONGMAIDIENTU.Areas.Admin.Controllers
{
    public class DashboardController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Title = "Dashboard Thống Kê";
            ViewBag.UserName = "Nguyễn Minh Vỹ";
            return View();
        }
    }
}
