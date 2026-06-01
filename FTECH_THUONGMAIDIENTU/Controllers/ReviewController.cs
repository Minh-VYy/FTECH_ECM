using System.Linq;
using System.Web.Mvc;
using FTECH_THUONGMAIDIENTU.Data;
using FTECH_THUONGMAIDIENTU.Models.Posts;

namespace FTECH_THUONGMAIDIENTU.Controllers
{
    public class ReviewController : Controller
    {
        private readonly PostRepository postRepository = new PostRepository();

        public ActionResult Index(string q)
        {
            ViewBag.Title = string.IsNullOrWhiteSpace(q)
                ? "Đánh giá sản phẩm"
                : $"Kết quả tìm kiếm: {q.Trim()}";
            ViewBag.SearchQuery = q;
            ViewBag.ReviewPosts = postRepository.GetRecentPosts(12, q);
            return View();
        }

        [HttpGet]
        public JsonResult Feed(string q)
        {
            var posts = postRepository.GetRecentPosts(12, q);
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
