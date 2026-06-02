using System;
using System.Collections.Generic;
using System.Data;
using System.Web.Mvc;
using FTECH_THUONGMAIDIENTU.Infrastructure;

namespace FTECH_THUONGMAIDIENTU.Areas.SuperAdmin.Controllers
{
    [SessionRoleAuthorize(SessionKey = "AdminRole", AllowedRolesCsv = RoleKeys.SuperAdmin + "," + RoleKeys.UserAccountManager, LoginUrl = "/Admin/Account/Login")]
    public class AdminAccountController : Controller
    {
        // GET: SuperAdmin/AdminAccount
        public ActionResult Index()
        {
            ViewBag.Title = "Quản Lý Tài Khoản Admin";

            var adminAccounts = new List<object>();
            int totalAccounts = 0, activeAccounts = 0, lockedAccounts = 0, newAccountsThisWeek = 0, partnerCount = 0;

            using (var connection = SqlConnectionFactory.CreateConnection())
            {
                connection.Open();

                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = @"
SELECT a.AdminID, a.FullName, a.Email, a.Status, a.CreatedAt, r.RoleName
FROM Admins a
INNER JOIN Roles r ON a.RoleID = r.RoleID
ORDER BY a.CreatedAt DESC;";
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var adminId = Convert.ToInt32(reader["AdminID"]);
                            var fullName = reader["FullName"]?.ToString() ?? "";
                            var email = reader["Email"]?.ToString() ?? "";
                            var status = reader["Status"]?.ToString() ?? "";
                            var createdAt = reader["CreatedAt"] != DBNull.Value ? Convert.ToDateTime(reader["CreatedAt"]) : DateTime.Now;
                            var roleName = reader["RoleName"]?.ToString() ?? "";

                            var mappedStatus = string.Equals(status, "Hoạt động", StringComparison.OrdinalIgnoreCase) ? "Active" : "Locked";

                            var permissions = "Truy cập cơ bản";
                            if (roleName.Contains("Super") || roleName.ToLower().Contains("superadmin")) permissions = "Toàn quyền hệ thống";
                            else if (roleName.Contains("Content")) permissions = "Biên tập & Phê duyệt bài đăng";
                            else if (roleName.Contains("Affiliate")) permissions = "Quản lý liên kết & Doanh số";
                            else if (roleName.Contains("User")) permissions = "Kiểm duyệt & Xử lý người dùng";

                            adminAccounts.Add(new
                            {
                                Id = adminId,
                                Avatar = "https://i.pravatar.cc/120?img=" + (10 + adminId),
                                FullName = fullName,
                                Email = email,
                                Role = roleName,
                                Status = mappedStatus,
                                StatusLabel = status,
                                Permissions = permissions,
                                LastUpdated = createdAt
                            });

                            totalAccounts++;
                            if (string.Equals(status, "Hoạt động", StringComparison.OrdinalIgnoreCase)) activeAccounts++;
                            else lockedAccounts++;
                            if ((DateTime.Now - createdAt).TotalDays <= 7) newAccountsThisWeek++;
                        }
                    }
                }

                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = "SELECT COUNT(*) FROM AffiliatePartners WHERE Status = N'Hoạt động';";
                    partnerCount = Convert.ToInt32(cmd.ExecuteScalar());
                }
            }

            ViewBag.AdminAccounts = adminAccounts;
            ViewBag.TotalAccounts = totalAccounts;
            ViewBag.ActiveAccounts = activeAccounts;
            ViewBag.LockedAccounts = lockedAccounts;
            ViewBag.NewAccountsThisWeek = newAccountsThisWeek;
            ViewBag.ActivePercentage = totalAccounts > 0 ? (activeAccounts * 100 / totalAccounts) : 0;
            ViewBag.PartnerCount = partnerCount;

            return View();
        }

        // POST: SuperAdmin/AdminAccount/UpdateRole
        [HttpPost]
        public JsonResult UpdateRole(int id, string newRole, string note)
        {
            try
            {
                using (var connection = SqlConnectionFactory.CreateConnection())
                {
                    connection.Open();
                    using (var cmd = connection.CreateCommand())
                    {
                        cmd.CommandText = @"
DECLARE @RoleId INT;
SELECT @RoleId = RoleID FROM Roles WHERE RoleName = @roleName;
IF @RoleId IS NOT NULL
    UPDATE Admins SET RoleID = @RoleId WHERE AdminID = @id;
SELECT @RoleId;";
                        cmd.Parameters.AddWithValue("@id", id);
                        cmd.Parameters.AddWithValue("@roleName", newRole ?? "");
                        var result = cmd.ExecuteScalar();
                        var success = result != null && result != DBNull.Value;
                        return Json(new { success = success, message = success ? "Đã cập nhật vai trò thành công!" : "Không tìm thấy vai trò hoặc tài khoản." });
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        // POST: SuperAdmin/AdminAccount/ToggleLock
        [HttpPost]
        public JsonResult ToggleLock(int id)
        {
            try
            {
                using (var connection = SqlConnectionFactory.CreateConnection())
                {
                    connection.Open();
                    using (var cmd = connection.CreateCommand())
                    {
                        cmd.CommandText = @"
UPDATE Admins
SET Status = CASE WHEN Status = N'Hoạt động' THEN N'Đã khóa' ELSE N'Hoạt động' END
WHERE AdminID = @id;
SELECT Status FROM Admins WHERE AdminID = @id;";
                        cmd.Parameters.AddWithValue("@id", id);
                        var newStatus = cmd.ExecuteScalar()?.ToString() ?? "";
                        return Json(new { success = true, newStatus = newStatus, message = newStatus == "Đã khóa" ? "Đã khóa tài khoản." : "Đã mở khóa tài khoản." });
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        // GET: SuperAdmin/AdminAccount/Create
        public ActionResult Create()
        {
            ViewBag.Title = "Tạo Tài Khoản Admin Mới";
            return View();
        }
    }
}
