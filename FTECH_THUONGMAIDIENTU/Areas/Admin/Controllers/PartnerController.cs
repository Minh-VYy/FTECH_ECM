using System;
using System.Web.Mvc;

namespace FTECH_THUONGMAIDIENTU.Areas.Admin.Controllers
{
    public class PartnerController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Title = "Quản Lý Đối Tác";
            return View();
        }
    }
}
