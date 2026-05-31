using System.Web.Mvc;
using FTECH_THUONGMAIDIENTU.Infrastructure;

namespace FTECH_THUONGMAIDIENTU.Areas.AffiliateManager.Controllers
{
    /// <summary>
    /// Quản lý affiliate link
    /// </summary>
    [SessionRoleAuthorize(SessionKey = "AdminRole", AllowedRolesCsv = RoleKeys.SuperAdmin + "," + RoleKeys.AffiliateManager, LoginUrl = "/Admin/Account/Login")]
    public class AffiliateController : Controller
    {
        // GET: AffiliateManager/Affiliate
        public ActionResult Index()
        {
            ViewBag.Title = "Quản Lý Affiliate Link";
            return View();
        }

        // GET: AffiliateManager/Affiliate/Create
        public ActionResult Create()
        {
            ViewBag.Title = "Gắn Affiliate Link Mới";
            return View();
        }

        // POST: AffiliateManager/Affiliate/Create
        [HttpPost]
        public ActionResult Create(FormCollection collection)
        {
            // Logic tạo affiliate link
            return RedirectToAction("Index");
        }

        // GET: AffiliateManager/Affiliate/Edit/5
        public ActionResult Edit(int id)
        {
            ViewBag.Title = "Chỉnh Sửa Affiliate Link";
            return View();
        }

        // POST: AffiliateManager/Affiliate/Edit/5
        [HttpPost]
        public ActionResult Edit(int id, FormCollection collection)
        {
            // Logic cập nhật affiliate link
            return RedirectToAction("Index");
        }
    }
}
