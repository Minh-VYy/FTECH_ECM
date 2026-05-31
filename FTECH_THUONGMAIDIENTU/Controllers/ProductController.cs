using System.Web.Mvc;
using FTECH_THUONGMAIDIENTU.Data;
using FTECH_THUONGMAIDIENTU.Models.Posts;

namespace FTECH_THUONGMAIDIENTU.Controllers
{
    public class ProductController : Controller
    {
        private readonly PostRepository postRepository = new PostRepository();

        public ActionResult Index()
        {
            ViewBag.Title = "Chi tiết bài review sản phẩm";
            ViewBag.FeaturedPost = postRepository.GetTopPublishedPost();
            return View();
        }
    }
}
