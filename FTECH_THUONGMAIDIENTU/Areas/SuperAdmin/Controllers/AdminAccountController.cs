using System;
using System.Collections.Generic;
using System.Data;
using System.Web.Mvc;
using FTECH_THUONGMAIDIENTU.Infrastructure;
using FTECH_THUONGMAIDIENTU.Models.Admin;

namespace FTECH_THUONGMAIDIENTU.Areas.SuperAdmin.Controllers
{
    [SessionRoleAuthorize(SessionKey = "AdminRole", AllowedRolesCsv = RoleKeys.SuperAdmin + "," + RoleKeys.UserAccountManager, LoginUrl = "/Admin/Account/Login")]
    public class AdminAccountController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Title = "Quản Lý Tài Khoản Admin";

            var adminAccounts = new List<AdminAccountViewModel>();
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
                            int adminId    = Convert.ToInt32(reader["AdminID"]);
                            string name    = reader["FullName"] as string ?? "";
                            string email   = reader["Email"] as string ?? "";
                            string status  = reader["Status"] as string ?? "";
                            var createdAt  = reader["CreatedAt"] == DBNull.Value ? DateTime.Now : Convert.ToDateTime(reader["CreatedAt"]);
                            string role    = reader["RoleName"] as string ?? "";

                            string mappedStatus = string.Equals(status, "Hoạt động", StringComparison.OrdinalIgnoreCase)
                                ? "Active" : "Locked";

                            string permissions = "Truy cập cơ bản";
                            if (role.Contains("Super"))     permissions = "Toàn quyền hệ thống";
                            else if (role.Contains("Content")) permissions = "Biên tập & Phê duyệt bài đăng";
                            else if (role.Contains("Affiliate")) permissions = "Quản lý liên kết & Doanh số";
                            else if (role.Contains("User"))  permissions = "Kiểm duyệt & Xử lý người dùng";

                            adminAccounts.Add(new AdminAccountViewModel
                            {
                                Id          = adminId,
                                FullName    = name,
                                Email       = email,
                                Role        = role,
                                Status      = mappedStatus,
                                StatusLabel = status,
                                Permissions = permissions,
                                LastUpdated = createdAt
                            });

                            totalAccounts++;
                            if (mappedStatus == "Active") activeAccounts++;
                            else lockedAccounts++;
                            if ((DateTime.Now - createdAt).TotalDays <= 7) newAccountsThisWeek++;
                        }
                    }
                }

                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = "SELECT COUNT(*) FROM AffiliatePartners WHERE Status = N'Hoạt động';";
                    var scalar = cmd.ExecuteScalar();
                    partnerCount = scalar == DBNull.Value ? 0 : Convert.ToInt32(scalar);
                }
            }

            ViewBag.AdminAccounts        = adminAccounts;
            ViewBag.TotalAccounts        = totalAccounts;
            ViewBag.ActiveAccounts       = activeAccounts;
            ViewBag.LockedAccounts       = lockedAccounts;
            ViewBag.NewAccountsThisWeek  = newAccountsThisWeek;
            ViewBag.ActivePercentage     = totalAccounts > 0 ? (activeAccounts * 100 / totalAccounts) : 0;
            ViewBag.PartnerCount         = partnerCount;

            return View();
        }

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
                        var result  = cmd.ExecuteScalar();
                        bool ok     = result != null && result != DBNull.Value;
                        return Json(new { success = ok, message = ok ? "Đã cập nhật vai trò thành công!" : "Không tìm thấy vai trò hoặc tài khoản." });
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

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
                        string newStatus = cmd.ExecuteScalar() as string ?? "";
                        return Json(new { success = true, newStatus = newStatus, message = newStatus == "Đã khóa" ? "Đã khóa tài khoản." : "Đã mở khóa tài khoản." });
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        public ActionResult Create()
        {
            ViewBag.Title = "Tạo Tài Khoản Admin Mới";
            return View();
        }
    }
}
