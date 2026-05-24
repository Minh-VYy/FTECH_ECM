using System.Web.Mvc;

namespace FTECH_THUONGMAIDIENTU.Controllers
{
    public class ProductController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Title = "Chi tiết bài review sản phẩm";
            return View();
        }
    }
}
