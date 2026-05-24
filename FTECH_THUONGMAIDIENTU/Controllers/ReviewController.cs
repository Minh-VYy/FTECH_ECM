using System.Web.Mvc;

namespace FTECH_THUONGMAIDIENTU.Controllers
{
    public class ReviewController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Title = "Đánh giá sản phẩm";
            return View();
        }
    }
}
