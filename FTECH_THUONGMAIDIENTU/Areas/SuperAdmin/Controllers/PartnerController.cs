using System;
using System.Collections.Generic;
using System.Data;
using System.Web.Mvc;
using FTECH_THUONGMAIDIENTU.Infrastructure;

namespace FTECH_THUONGMAIDIENTU.Areas.SuperAdmin.Controllers
{
    [SessionRoleAuthorize(SessionKey = "AdminRole", AllowedRolesCsv = RoleKeys.SuperAdmin, LoginUrl = "/Admin/Account/Login")]
    public class PartnerController : Controller
    {
        // GET: SuperAdmin/Partner
        public ActionResult Index()
        {
            ViewBag.Title = "Duyệt Đối Tác Mới";

            var partners = new List<object>();
            int totalCount = 0, pendingCount = 0, activeCount = 0, rejectedCount = 0, suspendedCount = 0;
            long totalClicks = 0;

            using (var connection = SqlConnectionFactory.CreateConnection())
            {
                connection.Open();

                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = @"
SELECT
    COUNT(*) AS Total,
    SUM(CASE WHEN Status = N'Chờ duyệt' THEN 1 ELSE 0 END) AS PendingCount,
    SUM(CASE WHEN Status = N'Hoạt động' THEN 1 ELSE 0 END) AS ActiveCount,
    SUM(CASE WHEN Status = N'Từ chối' THEN 1 ELSE 0 END) AS RejectedCount,
    SUM(CASE WHEN Status = N'Tạm ngưng' THEN 1 ELSE 0 END) AS SuspendedCount
FROM AffiliatePartners;";
                    using (var reader = cmd.ExecuteReader(CommandBehavior.SingleRow))
                    {
                        if (reader.Read())
                        {
                            totalCount = Convert.ToInt32(reader["Total"]);
                            pendingCount = Convert.ToInt32(reader["PendingCount"]);
                            activeCount = Convert.ToInt32(reader["ActiveCount"]);
                            rejectedCount = Convert.ToInt32(reader["RejectedCount"]);
                            suspendedCount = Convert.ToInt32(reader["SuspendedCount"]);
                        }
                    }
                }

                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = "SELECT COUNT(*) FROM ClickTracking WHERE MONTH(ClickTime) = MONTH(GETDATE()) AND YEAR(ClickTime) = YEAR(GETDATE());";
                    totalClicks = Convert.ToInt64(cmd.ExecuteScalar());
                }

                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = @"
SELECT
    ap.PartnerID, ap.PartnerName, ap.WebsiteURL, ap.ContactInfo,
    ap.CurrentCommissionRate, ap.Status, ap.CreatedAt,
    (SELECT COUNT(*) FROM ClickTracking ct WHERE ct.PartnerID = ap.PartnerID AND MONTH(ct.ClickTime) = MONTH(GETDATE())) AS MonthClicks,
    (SELECT COUNT(*) FROM AffiliateLinks al WHERE al.PartnerID = ap.PartnerID AND al.Status = N'Hoạt động') AS ActiveLinks
FROM AffiliatePartners ap
ORDER BY
    CASE ap.Status WHEN N'Chờ duyệt' THEN 0 WHEN N'Hoạt động' THEN 1 ELSE 2 END,
    ap.CreatedAt DESC;";
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            partners.Add(new
                            {
                                Id = Convert.ToInt32(reader["PartnerID"]),
                                Name = reader["PartnerName"]?.ToString() ?? "",
                                Website = reader["WebsiteURL"]?.ToString() ?? "",
                                Contact = reader["ContactInfo"] != DBNull.Value ? reader["ContactInfo"]?.ToString() : "",
                                CommissionRate = reader["CurrentCommissionRate"] != DBNull.Value ? Convert.ToDecimal(reader["CurrentCommissionRate"]) : 0m,
                                Status = reader["Status"]?.ToString() ?? "",
                                CreatedAt = reader["CreatedAt"] != DBNull.Value ? Convert.ToDateTime(reader["CreatedAt"]) : DateTime.Now,
                                MonthClicks = Convert.ToInt32(reader["MonthClicks"]),
                                ActiveLinks = Convert.ToInt32(reader["ActiveLinks"])
                            });
                        }
                    }
                }
            }

            ViewBag.Partners = partners;
            ViewBag.TotalCount = totalCount;
            ViewBag.PendingCount = pendingCount;
            ViewBag.ActiveCount = activeCount;
            ViewBag.RejectedCount = rejectedCount;
            ViewBag.SuspendedCount = suspendedCount;
            ViewBag.TotalClicks = totalClicks;

            return View();
        }

        [HttpPost]
        public JsonResult Approve(int id)
        {
            try
            {
                using (var connection = SqlConnectionFactory.CreateConnection())
                {
                    connection.Open();
                    using (var cmd = connection.CreateCommand())
                    {
                        cmd.CommandText = "UPDATE AffiliatePartners SET Status = N'Hoạt động' WHERE PartnerID = @id";
                        cmd.Parameters.AddWithValue("@id", id);
                        var rows = cmd.ExecuteNonQuery();
                        return Json(new { success = rows > 0, message = rows > 0 ? "Đã duyệt đối tác thành công!" : "Không tìm thấy đối tác." });
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        [HttpPost]
        public JsonResult Reject(int id, string reason)
        {
            try
            {
                using (var connection = SqlConnectionFactory.CreateConnection())
                {
                    connection.Open();
                    using (var cmd = connection.CreateCommand())
                    {
                        cmd.CommandText = "UPDATE AffiliatePartners SET Status = N'Từ chối' WHERE PartnerID = @id";
                        cmd.Parameters.AddWithValue("@id", id);
                        var rows = cmd.ExecuteNonQuery();
                        return Json(new { success = rows > 0, message = rows > 0 ? "Đã từ chối đối tác." : "Không tìm thấy đối tác." });
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        [HttpPost]
        public JsonResult Suspend(int id)
        {
            try
            {
                using (var connection = SqlConnectionFactory.CreateConnection())
                {
                    connection.Open();
                    using (var cmd = connection.CreateCommand())
                    {
                        cmd.CommandText = "UPDATE AffiliatePartners SET Status = N'Tạm ngưng' WHERE PartnerID = @id";
                        cmd.Parameters.AddWithValue("@id", id);
                        var rows = cmd.ExecuteNonQuery();
                        return Json(new { success = rows > 0, message = rows > 0 ? "Đã tạm ngưng đối tác." : "Không tìm thấy đối tác." });
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }
    }
}
