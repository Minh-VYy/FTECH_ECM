using System.Web.Mvc;
using FTECH_THUONGMAIDIENTU.Data;
using FTECH_THUONGMAIDIENTU.Infrastructure;
using FTECH_THUONGMAIDIENTU.Models.UserAccount;

namespace FTECH_THUONGMAIDIENTU.Areas.UserAccount.Controllers
{
    /// <summary>
    /// User Account - Quản lý tài khoản người dùng bình thường
    /// Chức năng: Xem trang cá nhân, Quản lý thông tin cá nhân
    /// </summary>
    [SessionRoleAuthorize(SessionKey = "CurrentUserRole", AllowedRolesCsv = RoleKeys.Customer, LoginUrl = "/Account/Login")]
    public class AccountController : Controller
    {
        private readonly MemberRepository memberRepository = new MemberRepository();

        private MemberProfileSummary LoadCurrentMember()
        {
            var email = Session["CurrentUserEmail"] as string;
            var profile = memberRepository.GetByEmail(email);
            ViewBag.UserName = profile.FullName;
            ViewBag.UserRole = "Khách Hàng";
            ViewBag.UserAvatar = profile.AvatarURL;
            return profile;
        }

        // GET: UserAccount/Account/Dashboard
        public ActionResult Dashboard()
        {
            ViewBag.Title = "Trang Cá Nhân";
            ViewBag.UserRole = "User";
            return View("Dashboard", LoadCurrentMember());
        }

        // GET: UserAccount/Account/Profile
        public ActionResult Profile()
        {
            ViewBag.Title = "Quản Lý Thông Tin Cá Nhân";
            return View(LoadCurrentMember());
        }

        // GET: UserAccount/Account/Edit
        public ActionResult Edit()
        {
            ViewBag.Title = "Chỉnh Sửa Thông Tin";
            return View(LoadCurrentMember());
        }

        // POST: UserAccount/Account/Edit
        [HttpPost]
        public ActionResult Edit(MemberProfileUpdateRequest request)
        {
            var email = request.Email ?? Session["CurrentUserEmail"] as string;
            if (memberRepository.UpdateProfile(email, request))
            {
                TempData["SuccessMessage"] = "Cập nhật thông tin cá nhân thành công.";
            }

            return RedirectToAction("Profile");
        }
    }
}
