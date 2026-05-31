using System.Web.Mvc;
using FTECH_THUONGMAIDIENTU.Data;

namespace FTECH_THUONGMAIDIENTU.Controllers
{
    public class ReviewController : Controller
    {
        private readonly PostRepository postRepository = new PostRepository();

        public ActionResult Index()
        {
            ViewBag.Title = "Đánh giá sản phẩm";
            ViewBag.ReviewPosts = postRepository.GetPublishedPosts(12);
            return View();
        }
    }
}
