using System.Web.Mvc;

namespace FTECH_THUONGMAIDIENTU.Areas.SuperAdmin.Controllers
{
    /// <summary>
    /// Quản lý bài viết - Super Admin duyệt bài viết
    /// </summary>
    public class PostController : Controller
    {
        // GET: SuperAdmin/Post
        public ActionResult Index()
        {
            ViewBag.Title = "Duyệt Bài Viết";
            return View();
        }

        // POST: SuperAdmin/Post/Approve
        [HttpPost]
        public ActionResult Approve(int id)
        {
            // Logic duyệt bài viết
            return Json(new { success = true });
        }

        // POST: SuperAdmin/Post/Reject
        [HttpPost]
        public ActionResult Reject(int id, string reason)
        {
            // Logic từ chối bài viết
            return Json(new { success = true });
        }
    }
}
