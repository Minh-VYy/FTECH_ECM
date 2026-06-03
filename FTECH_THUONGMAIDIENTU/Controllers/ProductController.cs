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
            int? currentMemberId = null;

            if (featuredPost != null)
            {
                ViewBag.Comments = postRepository.GetPostComments(featuredPost.PostID);

                // Check if this member has purchased (clicked affiliate link) this product
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

            ViewBag.CurrentMemberID = currentMemberId;
            ViewBag.HasPurchased = true; // Allow any logged-in user to comment
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
        public JsonResult AddComment(int postId, string content, int? ratingStar)
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

            // All logged-in members can comment - no purchase required
            // (Purchase is tracked via ClickTracking but not enforced as a gate)

            if (string.IsNullOrWhiteSpace(content))
            {
                return Json(new { success = false, message = "Vui lòng nhập nội dung bình luận đánh giá." });
            }

            // Insert/Upsert rating and insert comment into DB
            using (var connection = SqlConnectionFactory.CreateConnection())
            {
                connection.Open();

                // 1. Upsert rating if provided
                if (ratingStar.HasValue && ratingStar.Value >= 1 && ratingStar.Value <= 5)
                {
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = @"
IF EXISTS (SELECT 1 FROM Ratings WHERE MemberID = @MemberID AND PostID = @PostID)
BEGIN
    UPDATE Ratings SET RatingStar = @RatingStar, CreatedAt = GETDATE() WHERE MemberID = @MemberID AND PostID = @PostID;
END
ELSE
BEGIN
    INSERT INTO Ratings (MemberID, PostID, RatingStar, CreatedAt) VALUES (@MemberID, @PostID, @RatingStar, GETDATE());
END";
                        command.Parameters.AddWithValue("@MemberID", member.MemberID);
                        command.Parameters.AddWithValue("@PostID", postId);
                        command.Parameters.AddWithValue("@RatingStar", ratingStar.Value);
                        command.ExecuteNonQuery();
                    }
                }

                // 2. Insert comment into DB
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = @"
INSERT INTO Comments (MemberID, PostID, Content, CreatedAt)
VALUES (@MemberID, @PostID, @Content, GETDATE());";
                    command.Parameters.AddWithValue("@MemberID", member.MemberID);
                    command.Parameters.AddWithValue("@PostID", postId);
                    command.Parameters.AddWithValue("@Content", content.Trim());
                    command.ExecuteNonQuery();
                }
            }

            return Json(new { success = true, memberName = member.FullName, memberAvatar = member.AvatarURL });
        }

        [HttpPost]
        public JsonResult EditComment(int commentId, string content, int? ratingStar)
        {
            var email = Session["CurrentUserEmail"] as string;
            if (string.IsNullOrWhiteSpace(email))
            {
                return Json(new { success = false, message = "Bạn phải đăng nhập để thực hiện chức năng này." });
            }

            var member = new MemberRepository().GetByEmail(email);
            if (member == null)
            {
                return Json(new { success = false, message = "Không tìm thấy thông tin tài khoản thành viên hợp lệ." });
            }

            if (string.IsNullOrWhiteSpace(content))
            {
                return Json(new { success = false, message = "Nội dung bình luận không được trống." });
            }

            using (var connection = SqlConnectionFactory.CreateConnection())
            {
                connection.Open();

                // 1. Fetch existing comment owner, post ID and edit count
                int? dbMemberId = null;
                int? dbPostId = null;
                int editCount = 0;
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT MemberID, PostID, ISNULL(EditCount, 0) AS EditCount FROM Comments WHERE CommentID = @CommentID;";
                    command.Parameters.AddWithValue("@CommentID", commentId);
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            dbMemberId = reader["MemberID"] == DBNull.Value ? (int?)null : Convert.ToInt32(reader["MemberID"]);
                            dbPostId = reader["PostID"] == DBNull.Value ? (int?)null : Convert.ToInt32(reader["PostID"]);
                            editCount = Convert.ToInt32(reader["EditCount"]);
                        }
                        else
                        {
                            return Json(new { success = false, message = "Không tìm thấy bình luận cần chỉnh sửa." });
                        }
                    }
                }

                // 2. Security Check: owner matches currently logged-in member
                if (dbMemberId != member.MemberID)
                {
                    return Json(new { success = false, message = "Bạn không có quyền chỉnh sửa bình luận này." });
                }

                // 3. Edit Limit Check: EditCount < 1
                if (editCount >= 1)
                {
                    return Json(new { success = false, message = "Bạn đã hết lượt chỉnh sửa bình luận này (chỉ được sửa tối đa 1 lần)." });
                }

                // 4. Perform Update
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "UPDATE Comments SET Content = @Content, EditCount = EditCount + 1 WHERE CommentID = @CommentID;";
                    command.Parameters.AddWithValue("@Content", content.Trim());
                    command.Parameters.AddWithValue("@CommentID", commentId);
                    command.ExecuteNonQuery();
                }

                if (ratingStar.HasValue && ratingStar.Value >= 1 && ratingStar.Value <= 5 && dbPostId.HasValue)
                {
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = @"
IF EXISTS (SELECT 1 FROM Ratings WHERE MemberID = @MemberID AND PostID = @PostID)
BEGIN
    UPDATE Ratings SET RatingStar = @RatingStar, CreatedAt = GETDATE() WHERE MemberID = @MemberID AND PostID = @PostID;
END
ELSE
BEGIN
    INSERT INTO Ratings (MemberID, PostID, RatingStar, CreatedAt) VALUES (@MemberID, @PostID, @RatingStar, GETDATE());
END";
                        command.Parameters.AddWithValue("@MemberID", dbMemberId.Value);
                        command.Parameters.AddWithValue("@PostID", dbPostId.Value);
                        command.Parameters.AddWithValue("@RatingStar", ratingStar.Value);
                        command.ExecuteNonQuery();
                    }
                }
            }

            return Json(new { success = true });
        }

        [HttpPost]
        public JsonResult DeleteComment(int commentId)
        {
            var email = Session["CurrentUserEmail"] as string;
            var role = Session["CurrentUserRole"] as string;
            if (string.IsNullOrWhiteSpace(email))
            {
                return Json(new { success = false, message = "Bạn phải đăng nhập để thực hiện chức năng này." });
            }

            var member = new MemberRepository().GetByEmail(email);
            if (member == null)
            {
                return Json(new { success = false, message = "Không tìm thấy thông tin tài khoản thành viên hợp lệ." });
            }

            using (var connection = SqlConnectionFactory.CreateConnection())
            {
                connection.Open();

                // 1. Fetch existing comment owner and post ID
                int? dbMemberId = null;
                int? dbPostId = null;
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT MemberID, PostID FROM Comments WHERE CommentID = @CommentID;";
                    command.Parameters.AddWithValue("@CommentID", commentId);
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            dbMemberId = reader["MemberID"] == DBNull.Value ? (int?)null : Convert.ToInt32(reader["MemberID"]);
                            dbPostId = reader["PostID"] == DBNull.Value ? (int?)null : Convert.ToInt32(reader["PostID"]);
                        }
                        else
                        {
                            return Json(new { success = false, message = "Không tìm thấy bình luận cần xóa." });
                        }
                    }
                }

                // 2. Security Check: owner matches currently logged-in member OR user is Admin
                bool isAdmin = !string.IsNullOrWhiteSpace(role) && role != "customer";
                if (dbMemberId != member.MemberID && !isAdmin)
                {
                    return Json(new { success = false, message = "Bạn không có quyền xóa bình luận này." });
                }

                // 3. Perform Delete of comment and corresponding rating
                if (dbMemberId.HasValue && dbPostId.HasValue)
                {
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = "DELETE FROM Ratings WHERE MemberID = @MemberID AND PostID = @PostID;";
                        command.Parameters.AddWithValue("@MemberID", dbMemberId.Value);
                        command.Parameters.AddWithValue("@PostID", dbPostId.Value);
                        command.ExecuteNonQuery();
                    }
                }
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "DELETE FROM Comments WHERE CommentID = @CommentID;";
                    command.Parameters.AddWithValue("@CommentID", commentId);
                    command.ExecuteNonQuery();
                }
            }

            return Json(new { success = true });
        }
    }
}
