using System;
using System.Web.Mvc;
using FTECH_THUONGMAIDIENTU.Data;
using FTECH_THUONGMAIDIENTU.Models.Posts;
using FTECH_THUONGMAIDIENTU.Infrastructure;
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
            var featuredPost = postRepository.GetPost(id, slug) ?? postRepository.GetTopPost();
            ViewBag.FeaturedPost = featuredPost;
            ViewBag.RelatedPosts = postRepository.GetRecentPosts(6).ToList();

            bool hasPurchased = false;
            var email = Session["CurrentUserEmail"] as string;

            if (featuredPost != null)
            {
                ViewBag.Comments = postRepository.GetPostComments(featuredPost.PostID);

                // Check if this member has purchased (clicked affiliate link) this product
                if (!string.IsNullOrWhiteSpace(email))
                {
                    var member = new MemberRepository().GetByEmail(email);
                    if (member != null)
                    {
                        using (var connection = SqlConnectionFactory.CreateConnection())
                        using (var command = connection.CreateCommand())
                        {
                            command.CommandText = @"
SELECT COUNT(*) 
FROM ClickTracking 
WHERE MemberID = @MemberID AND PostID = @PostID;";
                            command.Parameters.AddWithValue("@MemberID", member.MemberID);
                            command.Parameters.AddWithValue("@PostID", featuredPost.PostID);

                            connection.Open();
                            int count = Convert.ToInt32(command.ExecuteScalar());
                            hasPurchased = count > 0;
                        }
                    }
                }
            }
            else
            {
                ViewBag.Comments = new List<FTECH_THUONGMAIDIENTU.Models.Dashboard.CommentSummary>();
            }

            ViewBag.HasPurchased = hasPurchased;
            return View();
        }

        [HttpGet]
        public ActionResult Buy(int postId, int partnerId, string url)
        {
            var email = Session["CurrentUserEmail"] as string;
            int? memberId = null;

            if (!string.IsNullOrWhiteSpace(email))
            {
                var member = new MemberRepository().GetByEmail(email);
                if (member != null)
                {
                    memberId = member.MemberID;
                }
            }

            // Record in ClickTracking matching the user's new schema
            using (var connection = SqlConnectionFactory.CreateConnection())
            {
                connection.Open();

                // 1. Find a link in AffiliateLinks matching this PostID and PartnerID
                Guid? linkId = null;
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = @"
SELECT TOP 1 LinkID 
FROM AffiliateLinks 
WHERE PostID = @PostID AND PartnerID = @PartnerID AND Status = N'Hoạt động';";
                    command.Parameters.AddWithValue("@PostID", postId);
                    command.Parameters.AddWithValue("@PartnerID", partnerId);
                    
                    var result = command.ExecuteScalar();
                    if (result != null && result != DBNull.Value)
                    {
                        linkId = (Guid)result;
                    }
                }

                // 2. Fallback: If no link exists in AffiliateLinks, dynamically insert one to preserve foreign key constraints
                if (!linkId.HasValue)
                {
                    linkId = Guid.NewGuid();
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = @"
INSERT INTO AffiliateLinks (LinkID, LinkName, TargetURL, PartnerID, PostID, Status)
VALUES (@LinkID, @LinkName, @TargetURL, @PartnerID, @PostID, N'Hoạt động');";
                        command.Parameters.AddWithValue("@LinkID", linkId.Value);
                        command.Parameters.AddWithValue("@LinkName", "Affiliate Link for Partner " + partnerId);
                        command.Parameters.AddWithValue("@TargetURL", string.IsNullOrWhiteSpace(url) ? "https://ftech.vn" : url);
                        command.Parameters.AddWithValue("@PartnerID", partnerId);
                        command.Parameters.AddWithValue("@PostID", postId);
                        
                        command.ExecuteNonQuery();
                    }
                }

                // 3. Resolve user IP Address safely
                string ipAddress = Request.UserHostAddress;
                if (string.IsNullOrWhiteSpace(ipAddress))
                {
                    ipAddress = "127.0.0.1";
                }

                // 4. Resolve Referrer URL safely
                string referrerUrl = Request.UrlReferrer?.ToString();
                if (string.IsNullOrWhiteSpace(referrerUrl))
                {
                    referrerUrl = Request.Url?.ToString() ?? "https://ftech.vn";
                }

                // 5. Insert ClickTracking record
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = @"
INSERT INTO ClickTracking (LinkID, PartnerID, PostID, MemberID, ClickTime, IPAddress, ReferrerURL, IsSuspicious)
VALUES (@LinkID, @PartnerID, @PostID, @MemberID, GETDATE(), @IPAddress, @ReferrerURL, 0);";
                    
                    command.Parameters.AddWithValue("@LinkID", linkId.Value);
                    command.Parameters.AddWithValue("@PartnerID", partnerId);
                    command.Parameters.AddWithValue("@PostID", postId);
                    command.Parameters.AddWithValue("@MemberID", memberId.HasValue ? (object)memberId.Value : DBNull.Value);
                    command.Parameters.AddWithValue("@IPAddress", ipAddress);
                    command.Parameters.AddWithValue("@ReferrerURL", referrerUrl);

                    command.ExecuteNonQuery();
                }
            }

            return Redirect(string.IsNullOrWhiteSpace(url) ? "https://ftech.vn" : url);
        }

        [HttpPost]
        public JsonResult AddComment(int postId, string content)
        {
            var email = Session["CurrentUserEmail"] as string;
            if (string.IsNullOrWhiteSpace(email))
            {
                return Json(new { success = false, message = "Bạn phải đăng nhập tài khoản thành viên để bình luận." });
            }

            var member = new MemberRepository().GetByEmail(email);
            if (member == null)
            {
                return Json(new { success = false, message = "Không tìm thấy thông tin tài khoản thành viên hợp lệ." });
            }

            // Verify they have purchased (clicked the buy affiliate link)
            bool hasPurchased = false;
            using (var connection = SqlConnectionFactory.CreateConnection())
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"
SELECT COUNT(*) 
FROM ClickTracking 
WHERE MemberID = @MemberID AND PostID = @PostID;";
                command.Parameters.AddWithValue("@MemberID", member.MemberID);
                command.Parameters.AddWithValue("@PostID", postId);

                connection.Open();
                int count = Convert.ToInt32(command.ExecuteScalar());
                hasPurchased = count > 0;
            }

            if (!hasPurchased)
            {
                return Json(new { success = false, message = "Yêu cầu mua sắm: Bạn chỉ có thể đánh giá, bình luận sau khi đã click xem và mua sản phẩm từ đối tác của chúng tôi!" });
            }

            if (string.IsNullOrWhiteSpace(content))
            {
                return Json(new { success = false, message = "Vui lòng nhập nội dung bình luận đánh giá." });
            }

            // Insert comment into DB
            using (var connection = SqlConnectionFactory.CreateConnection())
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"
INSERT INTO Comments (MemberID, PostID, Content, CreatedAt)
VALUES (@MemberID, @PostID, @Content, GETDATE());";
                command.Parameters.AddWithValue("@MemberID", member.MemberID);
                command.Parameters.AddWithValue("@PostID", postId);
                command.Parameters.AddWithValue("@Content", content.Trim());

                connection.Open();
                command.ExecuteNonQuery();
            }

            return Json(new { success = true, memberName = member.FullName, memberAvatar = member.AvatarURL });
        }
    }
}
