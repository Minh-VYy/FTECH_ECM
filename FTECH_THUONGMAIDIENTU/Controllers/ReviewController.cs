using System.Linq;
using System.Web.Mvc;
using FTECH_THUONGMAIDIENTU.Data;
using FTECH_THUONGMAIDIENTU.Models.Posts;

namespace FTECH_THUONGMAIDIENTU.Controllers
{
    public class ReviewController : Controller
    {
        private readonly PostRepository postRepository = new PostRepository();

        public ActionResult Index()
        {
            ViewBag.Title = "Đánh giá sản phẩm";
            ViewBag.ReviewPosts = postRepository.GetRecentPosts(12);
            return View();
        }

        [HttpGet]
        public JsonResult Feed()
        {
            var posts = postRepository.GetRecentPosts(12);
            return Json(new
            {
                reviews = posts.Select(MapReviewCard)
            }, JsonRequestBehavior.AllowGet);
        }

        private static object MapReviewCard(PostSummary post)
        {
            return new
            {
                post.PostID,
                post.Title,
                post.Slug,
                post.ThumbnailURL,
                post.Content,
                post.CategoryName,
                post.ViewCount,
                post.RatingCount,
                post.AverageRating,
                post.CreatedAt,
                Url = string.IsNullOrWhiteSpace(post.Slug)
                    ? $"/Product/Index/{post.PostID}"
                    : $"/product/{post.Slug}.html"
            };
        }
    }
}
