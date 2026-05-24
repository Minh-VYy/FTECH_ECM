using System;
using System.Web.Mvc;

namespace FTECH_THUONGMAIDIENTU.Areas.ContentManager.Controllers
{
    public class PostController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Title = "Bài Viết Của Tôi";
            ViewBag.UserName = "Trương Thị Kiều Nhi";
            ViewBag.UserRole = "Content Manager";
            ViewBag.UserAvatar = "https://images.unsplash.com/photo-1494790108377-be9c29b29330?auto=format&fit=crop&w=120&q=80";
            ViewBag.DraftCount = 3;
            return View();
        }

        public ActionResult Create()
        {
            ViewBag.Title = "Tạo Bài Viết Mới";
            ViewBag.UserName = "Trương Thị Kiều Nhi";
            ViewBag.UserRole = "Content Manager";
            return View();
        }

        public ActionResult Edit(int id)
        {
            ViewBag.Title = "Chỉnh Sửa Bài Viết";
            ViewBag.UserName = "Trương Thị Kiều Nhi";
            ViewBag.UserRole = "Content Manager";
            return View();
        }
    }
}
