using System.Web.Mvc;
using FTECH_THUONGMAIDIENTU.Data;

namespace FTECH_THUONGMAIDIENTU.Areas.Admin.Controllers
{
    public class DashboardController : Controller
    {
        private readonly StatisticsRepository statisticsRepository = new StatisticsRepository();

        public ActionResult Index()
        {
            ViewBag.Title = "Dashboard Thống Kê";
            ViewBag.UserName = "Nguyễn Minh Vỹ";
            ViewBag.Summary = statisticsRepository.GetSummary();
            return View();
        }
    }
}
