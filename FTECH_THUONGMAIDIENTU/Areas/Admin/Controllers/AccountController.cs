using System;
using System.Web.Mvc;
using FTECH_THUONGMAIDIENTU.Data;
using FTECH_THUONGMAIDIENTU.Infrastructure;
using FTECH_THUONGMAIDIENTU.Models.Auth;

namespace FTECH_THUONGMAIDIENTU.Areas.Admin.Controllers
{
    public class AccountController : Controller
    {
        private readonly AccountRepository accountRepository = new AccountRepository();

        public ActionResult Login()
        {
            ViewBag.Title = "Cổng đăng nhập phân quyền";
            return View("~/Areas/Admin/Views/Auth/Login.cshtml");
        }

        [HttpPost]
        public JsonResult Login(AccountAuthRequest request)
        {
            var result = accountRepository.AuthenticateAdmin(request);
            if (result.Success)
            {
                Session["AdminName"] = result.DisplayName;
                Session["AdminEmail"] = result.Email;
                Session["AdminRole"] = result.RoleKey;

                // Sync with public session keys for unified login experience
                Session["CurrentUserName"] = result.DisplayName;
                Session["CurrentUserEmail"] = result.Email;
                Session["CurrentUserRole"] = result.RoleKey;

                // Keep repository's redirected URL to ensure each role goes to the correct panel, fallback to /Admin/Dashboard
                if (string.IsNullOrWhiteSpace(result.RedirectUrl) || result.RedirectUrl == "/")
                {
                    result.RedirectUrl = Url.Action("Index", "Dashboard", new { area = "Admin" });
                }
            }

            return Json(new
            {
                success = result.Success,
                message = result.Message,
                redirectUrl = result.RedirectUrl,
                roleKey = result.RoleKey,
                displayName = result.DisplayName,
                email = result.Email
            });
        }

        public ActionResult Register()
        {
            ViewBag.Title = "Đăng ký tài khoản";
            return View("~/Areas/Admin/Views/Auth/Register.cshtml");
        }

        public ActionResult ForgotPassword()
        {
            ViewBag.Title = "Quên mật khẩu";
            return View("~/Areas/Admin/Views/Auth/ResetPassword.cshtml");
        }

        public ActionResult Accounts()
        {
            if (!string.Equals(Session["AdminRole"] as string, RoleKeys.SuperAdmin, System.StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToAction("Login", "Account", new { area = "Admin" });
            }

            var adminAccounts = new System.Collections.Generic.List<object>();
            int totalAccounts = 0;
            int activeAccounts = 0;
            int lockedAccounts = 0;
            int newAccountsThisWeek = 0;
            int partnerCount = 0;

            using (var connection = SqlConnectionFactory.CreateConnection())
            {
                connection.Open();

                // Get Admin list
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = @"
SELECT a.AdminID, a.FullName, a.Email, a.Status, a.CreatedAt, r.RoleName
FROM Admins a
INNER JOIN Roles r ON a.RoleID = r.RoleID
ORDER BY a.CreatedAt DESC;";
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var adminId = Convert.ToInt32(reader["AdminID"]);
                            var fullName = reader["FullName"]?.ToString() ?? "";
                            var email = reader["Email"]?.ToString() ?? "";
                            var status = reader["Status"]?.ToString() ?? "";
                            var createdAt = reader["CreatedAt"] != DBNull.Value ? Convert.ToDateTime(reader["CreatedAt"]) : DateTime.Now;
                            var roleName = reader["RoleName"]?.ToString() ?? "";

                            // Determine mapped status for View ("Active" vs other)
                            var mappedStatus = string.Equals(status, "Hoạt động", StringComparison.OrdinalIgnoreCase) ? "Active" : "Locked";

                            // Determine permissions description based on role
                            var permissions = "Truy cập cơ bản";
                            if (roleName.Contains("Super Admin") || roleName.Contains("SuperAdmin"))
                            {
                                permissions = "Toàn quyền hệ thống";
                            }
                            else if (roleName.Contains("Content"))
                            {
                                permissions = "Biên tập & Phê duyệt bài đăng";
                            }
                            else if (roleName.Contains("Affiliate"))
                            {
                                permissions = "Quản lý liên kết & Doanh số";
                            }
                            else if (roleName.Contains("User"))
                            {
                                permissions = "Kiểm duyệt & Xử lý người dùng";
                            }

                            // Mock phone based on adminId
                            var phone = "090-" + (100 + adminId) + "-" + (456 + adminId);

                            adminAccounts.Add(new {
                                Id = adminId,
                                Avatar = "https://i.pravatar.cc/120?img=" + (10 + adminId),
                                FullName = fullName,
                                Email = email,
                                Phone = phone,
                                Role = roleName,
                                Status = mappedStatus,
                                Permissions = permissions,
                                LastUpdated = createdAt
                            });

                            totalAccounts++;
                            if (string.Equals(status, "Hoạt động", StringComparison.OrdinalIgnoreCase))
                            {
                                activeAccounts++;
                            }
                            else
                            {
                                lockedAccounts++;
                            }

                            if ((DateTime.Now - createdAt).TotalDays <= 7)
                            {
                                newAccountsThisWeek++;
                            }
                        }
                    }
                }

                // Get Partner count
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT COUNT(*) FROM AffiliatePartners;";
                    partnerCount = Convert.ToInt32(command.ExecuteScalar());
                }
            }

            ViewBag.AdminAccounts = adminAccounts;
            ViewBag.TotalAccounts = totalAccounts;
            ViewBag.ActiveAccounts = activeAccounts;
            ViewBag.LockedAccounts = lockedAccounts;
            ViewBag.NewAccountsThisWeek = newAccountsThisWeek;
            ViewBag.ActivePercentage = totalAccounts > 0 ? (activeAccounts * 100 / totalAccounts) : 0;
            ViewBag.PartnerCount = partnerCount;
            ViewBag.NewPartners = 1; // Default mock number for UI

            ViewBag.Title = "Quản Lý Tài Khoản Admin";
            return View("Index");
        }
    }
}
