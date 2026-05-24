using System.Web.Mvc;

namespace FTECH_THUONGMAIDIENTU.Areas.SuperAdmin.Controllers
{
    /// <summary>
    /// Quản lý tài khoản admin - Super Admin quản lý tất cả admin account
    /// </summary>
    public class AdminAccountController : Controller
    {
        // GET: SuperAdmin/AdminAccount
        public ActionResult Index()
        {
            ViewBag.Title = "Quản Lý Tài Khoản Admin";
            return View();
        }

        // GET: SuperAdmin/AdminAccount/Create
        public ActionResult Create()
        {
            ViewBag.Title = "Tạo Tài Khoản Admin Mới";
            return View();
        }

        // POST: SuperAdmin/AdminAccount/Create
        [HttpPost]
        public ActionResult Create(FormCollection collection)
        {
            // Logic tạo admin account
            return RedirectToAction("Index");
        }

        // GET: SuperAdmin/AdminAccount/Edit/5
        public ActionResult Edit(int id)
        {
            ViewBag.Title = "Chỉnh Sửa Tài Khoản Admin";
            return View();
        }

        // POST: SuperAdmin/AdminAccount/Edit/5
        [HttpPost]
        public ActionResult Edit(int id, FormCollection collection)
        {
            // Logic cập nhật admin account
            return RedirectToAction("Index");
        }
    }
}
