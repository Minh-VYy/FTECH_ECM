using System.Web.Mvc;
using FTECH_THUONGMAIDIENTU.Data;
using FTECH_THUONGMAIDIENTU.Models.Posts;
using System.Collections.Generic;
using System.Linq;

namespace FTECH_THUONGMAIDIENTU.Controllers
{
    public class ProductController : Controller
    {
        private readonly PostRepository postRepository = new PostRepository();

        public ActionResult Index(int? id, string slug)
        {
            ViewBag.Title = "Chi tiết bài review sản phẩm";
            ViewBag.FeaturedPost = postRepository.GetPost(id, slug) ?? postRepository.GetTopPost();
            ViewBag.RelatedPosts = postRepository.GetRecentPosts(6).ToList();
            return View();
        }
    }
}
