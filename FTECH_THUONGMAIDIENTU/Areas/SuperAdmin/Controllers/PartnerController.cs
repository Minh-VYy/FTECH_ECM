using System.Web.Mvc;
using FTECH_THUONGMAIDIENTU.Infrastructure;

namespace FTECH_THUONGMAIDIENTU.Areas.SuperAdmin.Controllers
{
    /// <summary>
    /// Quản lý đối tác - Super Admin duyệt đối tác mới
    /// </summary>
    [SessionRoleAuthorize(SessionKey = "AdminRole", AllowedRolesCsv = RoleKeys.SuperAdmin, LoginUrl = "/Admin/Account/Login")]
    public class PartnerController : Controller
    {
        // GET: SuperAdmin/Partner
        public ActionResult Index()
        {
            ViewBag.Title = "Duyệt Đối Tác Mới";
            return View();
        }

        // POST: SuperAdmin/Partner/Approve
        [HttpPost]
        public ActionResult Approve(int id)
        {
            // Logic duyệt đối tác
            return Json(new { success = true });
        }

        // POST: SuperAdmin/Partner/Reject
        [HttpPost]
        public ActionResult Reject(int id, string reason)
        {
            // Logic từ chối đối tác
            return Json(new { success = true });
        }
    }
}
