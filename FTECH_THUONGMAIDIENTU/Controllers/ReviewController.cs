using System;
using System.Linq;
using System.Web.Mvc;
using FTECH_THUONGMAIDIENTU.Data;
using FTECH_THUONGMAIDIENTU.Infrastructure;
using FTECH_THUONGMAIDIENTU.Models.Posts;

namespace FTECH_THUONGMAIDIENTU.Controllers
{
    public class ReviewController : Controller
    {
        private readonly PostRepository postRepository = new PostRepository();

        public ActionResult Index(int? id, string q, string brand, string sort)
        {
            ViewBag.Title = string.IsNullOrWhiteSpace(q)
                ? "Đánh giá sản phẩm"
                : $"Kết quả tìm kiếm: {q.Trim()}";
            ViewBag.SearchQuery = q;
            ViewBag.SearchBrand = brand;
            ViewBag.SearchSort = sort;
            ViewBag.ReviewPosts = postRepository.GetRecentPosts(1000, q, null, brand, sort);

            PostSummary product = null;
            bool hasPurchased = false;
            var email = Session["CurrentUserEmail"] as string;
            int? currentMemberId = null;

            if (id.HasValue)
            {
                product = postRepository.GetPost(id.Value, null);
                if (product != null)
                {
                    if (!string.IsNullOrWhiteSpace(email))
                    {
                        var member = new MemberRepository().GetByEmail(email);
                        if (member != null)
                        {
                            currentMemberId = member.MemberID;
                            using (var connection = SqlConnectionFactory.CreateConnection())
                            using (var command = connection.CreateCommand())
                            {
                                command.CommandText = @"
SELECT COUNT(*) 
FROM ClickTracking 
WHERE MemberID = @MemberID AND PostID = @PostID;";
                                command.Parameters.AddWithValue("@MemberID", member.MemberID);
                                command.Parameters.AddWithValue("@PostID", product.PostID);

                                connection.Open();
                                int count = Convert.ToInt32(command.ExecuteScalar());
                                hasPurchased = count > 0;
                            }
                        }
                    }
                }
            }

            ViewBag.Product = product;
            ViewBag.HasPurchased = hasPurchased;
            ViewBag.CurrentMemberID = currentMemberId;

            return View();
        }

        [HttpGet]
        public JsonResult Feed(int? id, string q, string brand, string sort)
        {
            if (id.HasValue)
            {
                var comments = postRepository.GetPostComments(id.Value);
                return Json(new
                {
                    reviews = comments.Select(MapCommentToReviewCard)
                }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var posts = postRepository.GetRecentPosts(1000, q, null, brand, sort);
                return Json(new
                {
                    reviews = posts.Select(MapReviewCard)
                }, JsonRequestBehavior.AllowGet);
            }
        }

        private static object MapCommentToReviewCard(FTECH_THUONGMAIDIENTU.Models.Dashboard.CommentSummary comment)
        {
            return new
            {
                PostID = comment.PostID,
                Title = comment.MemberName,
                Slug = comment.PostSlug,
                ThumbnailURL = comment.MemberAvatarURL,
                Content = comment.Content,
                CategoryName = "Đánh giá thành viên",
                ViewCount = 0,
                RatingCount = 0,
                AverageRating = comment.RatingStar,
                CreatedAt = comment.CreatedAt,
                Url = "#",
                IsRealReview = true,
                CommentID = comment.CommentID,
                MemberID = comment.MemberID,
                EditCount = comment.EditCount
            };
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
