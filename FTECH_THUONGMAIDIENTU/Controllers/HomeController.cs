using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FTECH_THUONGMAIDIENTU.Data;

namespace FTECH_THUONGMAIDIENTU.Controllers
{
    public class HomeController : Controller
    {
        private readonly StatisticsRepository statisticsRepository = new StatisticsRepository();
        private readonly PostRepository postRepository = new PostRepository();

        public ActionResult Index()
        {
            ViewBag.Title = "FTECH – Công Nghệ Đỉnh Cao";
            ViewBag.Summary = statisticsRepository.GetSummary();
            ViewBag.FeaturedPosts = postRepository.GetPublishedPosts(3);
            ViewBag.TopPost = postRepository.GetTopPublishedPost();
            return View("Trangchu");
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
    }
}