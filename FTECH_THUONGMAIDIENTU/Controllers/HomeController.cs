using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FTECH_THUONGMAIDIENTU.Data;
using FTECH_THUONGMAIDIENTU.Infrastructure;
using FTECH_THUONGMAIDIENTU.Models.Dashboard;
using FTECH_THUONGMAIDIENTU.Models.Posts;
using System.Net.Mail;

namespace FTECH_THUONGMAIDIENTU.Controllers
{
    public class HomeController : Controller
    {
        private readonly StatisticsRepository statisticsRepository = new StatisticsRepository();
        private readonly PostRepository postRepository = new PostRepository();
        private readonly AffiliatePartnerRepository partnerRepository = new AffiliatePartnerRepository();

        public ActionResult Index()
        {
            ViewBag.Title = "FTECH – Công Nghệ Đỉnh Cao";
            ViewBag.Summary = statisticsRepository.GetSummary();
            ViewBag.FeaturedPosts = postRepository.GetRecentPosts(6);
            ViewBag.TopPost = postRepository.GetTopPost();
            ViewBag.Categories = postRepository.GetCategoriesWithCounts();
            ViewBag.RecentComments = postRepository.GetRecentComments(4);
            return View("Trangchu");
        }

        [HttpGet]
        public JsonResult Feed()
        {
            var featuredPosts = postRepository.GetRecentPosts(12).ToList();
            var categories = postRepository.GetCategoriesWithCounts().ToList();
            var comments = postRepository.GetRecentComments(6).ToList();
            var partners = partnerRepository.GetActivePartners(12).ToList();

            return Json(new
            {
                summary = statisticsRepository.GetSummary(),
                featuredPosts = featuredPosts.Select(MapPostCard),
                topPosts = featuredPosts.Take(8).Select(MapPostCard),
                categories,
                recentComments = comments.Select(MapCommentCard),
                partnerNames = partners.Select(MapPartnerCard)
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult About()
        {
            ViewBag.Message = "Tìm hiểu thêm về F-TECH và các dịch vụ thương mại điện tử của chúng tôi.";
            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Liên hệ với chúng tôi để được tư vấn và hỗ trợ.";
            return View();
        }

        [HttpPost]
        public ActionResult SendContact(string fullName, string email, string phone, string subject, string message)
        {
            // TODO: Implement contact form submission logic
            // Validate inputs, send email notification, save to database, etc.

            if (ModelState.IsValid)
            {
                try
                {
                    // Example: Send email notification
                    // SendEmailNotification(email, subject, message);

                    ViewBag.SuccessMessage = "Cảm ơn bạn! Tin nhắn của bạn đã được gửi thành công. Chúng tôi sẽ liên hệ lại trong thời gian sớm nhất.";
                    return RedirectToAction("Contact");
                }
                catch (Exception)
                {
                    ViewBag.ErrorMessage = "Có lỗi xảy ra khi gửi tin nhắn. Vui lòng thử lại.";
                }
            }

            return View("Contact");
        }

        [HttpPost]
        public JsonResult SubscribeNewsletter(string email)
        {
            var normalizedEmail = NormalizeEmail(email);
            if (string.IsNullOrWhiteSpace(normalizedEmail))
            {
                return Json(new { success = false, message = "Vui lòng nhập email hợp lệ." });
            }

            using (var connection = SqlConnectionFactory.CreateConnection())
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"
IF EXISTS (SELECT 1 FROM dbo.NewsletterSubscribers WHERE Email = @Email)
BEGIN
    SELECT 1;
END
ELSE
BEGIN
    INSERT INTO dbo.NewsletterSubscribers (Email, Status)
    VALUES (@Email, N'Active');
    SELECT 0;
END;";
                command.Parameters.AddWithValue("@Email", normalizedEmail);

                connection.Open();
                var alreadySubscribed = Convert.ToInt32(command.ExecuteScalar()) == 1;

                return Json(new
                {
                    success = true,
                    alreadySubscribed,
                    message = alreadySubscribed
                        ? "Email này đã được đăng ký trước đó."
                        : "Đăng ký newsletter thành công."
                });
            }
        }

        private static object MapPostCard(PostSummary post)
        {
            return new
            {
                post.PostID,
                post.Title,
                post.Slug,
                post.ThumbnailURL,
                post.CategoryName,
                post.ViewCount,
                post.RatingCount,
                post.AverageRating,
                post.CreatedAt,
                Content = post.Content,
                Url = string.IsNullOrWhiteSpace(post.Slug)
                    ? $"/Product/Index/{post.PostID}"
                    : $"/product/{HttpUtility.UrlPathEncode(post.Slug)}.html"
            };
        }

        private static object MapPartnerCard(AffiliatePartnerSummary partner)
        {
            return new
            {
                partner.PartnerID,
                partner.PartnerName,
                partner.WebsiteUrl,
                partner.Status
            };
        }

        private static object MapCommentCard(FTECH_THUONGMAIDIENTU.Models.Dashboard.CommentSummary comment)
        {
            return new
            {
                comment.CommentID,
                comment.PostID,
                comment.PostSlug,
                comment.PostTitle,
                comment.MemberName,
                comment.MemberAvatarURL,
                comment.Content,
                comment.CreatedAt
            };
        }

        private static string NormalizeEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return null;
            }

            try
            {
                return new MailAddress(email.Trim()).Address;
            }
            catch (FormatException)
            {
                return null;
            }
        }
    }
}