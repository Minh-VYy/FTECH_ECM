using System;
using System.Web.Mvc;
using FTECH_THUONGMAIDIENTU.Infrastructure;

namespace FTECH_THUONGMAIDIENTU.Areas.Admin.Controllers
{
    [SessionRoleAuthorize(SessionKey = "AdminRole", AllowedRolesCsv = RoleKeys.SuperAdmin, LoginUrl = "/Admin/Account/Login")]
    public class PartnerController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Title = "Quản Lý Đối Tác";
            return View();
        }
    }
}
