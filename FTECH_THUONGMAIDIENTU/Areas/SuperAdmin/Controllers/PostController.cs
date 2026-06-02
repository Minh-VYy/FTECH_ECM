using System;
using System.Collections.Generic;
using System.Data;
using System.Web.Mvc;
using FTECH_THUONGMAIDIENTU.Infrastructure;

namespace FTECH_THUONGMAIDIENTU.Areas.SuperAdmin.Controllers
{
    [SessionRoleAuthorize(SessionKey = "AdminRole", AllowedRolesCsv = RoleKeys.SuperAdmin, LoginUrl = "/Admin/Account/Login")]
    public class PostController : Controller
    {
        // GET: SuperAdmin/Post
        public ActionResult Index(string status = "all")
        {
            ViewBag.Title = "Duyệt Bài Viết";
            ViewBag.CurrentStatus = status;

            var posts = new List<object>();
            int totalCount = 0, draftCount = 0, pendingCount = 0, approvedCount = 0, rejectedCount = 0;

            using (var connection = SqlConnectionFactory.CreateConnection())
            {
                connection.Open();

                // Load KPI counts
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = @"
SELECT
    COUNT(*) AS Total,
    SUM(CASE WHEN Status = N'Nháp' THEN 1 ELSE 0 END) AS DraftCount,
    SUM(CASE WHEN Status = N'Chờ duyệt' THEN 1 ELSE 0 END) AS PendingCount,
    SUM(CASE WHEN Status = N'Đã xuất bản' THEN 1 ELSE 0 END) AS ApprovedCount,
    SUM(CASE WHEN Status = N'Từ chối' THEN 1 ELSE 0 END) AS RejectedCount
FROM Posts;";
                    using (var reader = cmd.ExecuteReader(CommandBehavior.SingleRow))
                    {
                        if (reader.Read())
                        {
                            totalCount = Convert.ToInt32(reader["Total"]);
                            draftCount = Convert.ToInt32(reader["DraftCount"]);
                            pendingCount = Convert.ToInt32(reader["PendingCount"]);
                            approvedCount = Convert.ToInt32(reader["ApprovedCount"]);
                            rejectedCount = Convert.ToInt32(reader["RejectedCount"]);
                        }
                    }
                }

                // Load posts list (filtered by status if requested)
                using (var cmd = connection.CreateCommand())
                {
                    var statusFilter = "";
                    if (!string.IsNullOrEmpty(status) && status != "all")
                    {
                        switch (status)
                        {
                            case "draft": statusFilter = "AND p.Status = N'Nháp'"; break;
                            case "pending": statusFilter = "AND p.Status = N'Chờ duyệt'"; break;
                            case "approved": statusFilter = "AND p.Status = N'Đã xuất bản'"; break;
                            case "rejected": statusFilter = "AND p.Status = N'Từ chối'"; break;
                        }
                    }

                    cmd.CommandText = $@"
SELECT TOP 50
    p.PostID, p.Title, p.Slug, p.Status, p.ThumbnailURL,
    p.ViewCount, p.CreatedAt, p.RejectionReason,
    c.CategoryName,
    a.FullName AS AuthorName,
    (SELECT COUNT(*) FROM AffiliateLinks al WHERE al.PostID = p.PostID AND al.Status = N'Hoạt động') AS AffLinkCount,
    (SELECT COUNT(*) FROM ClickTracking ct WHERE ct.PostID = p.PostID) AS ClickCount
FROM Posts p
INNER JOIN Categories c ON p.CategoryID = c.CategoryID
INNER JOIN Admins a ON p.CreatedBy = a.AdminID
WHERE 1=1 {statusFilter}
ORDER BY p.CreatedAt DESC;";

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            posts.Add(new
                            {
                                Id = Convert.ToInt32(reader["PostID"]),
                                Title = reader["Title"]?.ToString() ?? "",
                                Slug = reader["Slug"]?.ToString() ?? "",
                                Status = reader["Status"]?.ToString() ?? "",
                                Thumbnail = reader["ThumbnailURL"] != DBNull.Value ? reader["ThumbnailURL"]?.ToString() : "",
                                ViewCount = Convert.ToInt32(reader["ViewCount"]),
                                CreatedAt = reader["CreatedAt"] != DBNull.Value ? Convert.ToDateTime(reader["CreatedAt"]) : DateTime.Now,
                                RejectionReason = reader["RejectionReason"] != DBNull.Value ? reader["RejectionReason"]?.ToString() : "",
                                Category = reader["CategoryName"]?.ToString() ?? "",
                                Author = reader["AuthorName"]?.ToString() ?? "",
                                AffLinkCount = Convert.ToInt32(reader["AffLinkCount"]),
                                ClickCount = Convert.ToInt32(reader["ClickCount"])
                            });
                        }
                    }
                }
            }

            ViewBag.Posts = posts;
            ViewBag.TotalCount = totalCount;
            ViewBag.DraftCount = draftCount;
            ViewBag.PendingCount = pendingCount;
            ViewBag.ApprovedCount = approvedCount;
            ViewBag.RejectedCount = rejectedCount;

            return View();
        }

        // POST: SuperAdmin/Post/Approve
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
                        cmd.CommandText = "UPDATE Posts SET Status = N'Đã xuất bản', RejectionReason = NULL WHERE PostID = @id";
                        cmd.Parameters.AddWithValue("@id", id);
                        var rows = cmd.ExecuteNonQuery();
                        return Json(new { success = rows > 0, message = rows > 0 ? "Bài viết đã được duyệt thành công!" : "Không tìm thấy bài viết." });
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        // POST: SuperAdmin/Post/Reject
        [HttpPost]
        public JsonResult Reject(int id, string reason)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(reason))
                {
                    return Json(new { success = false, message = "Vui lòng nhập lý do từ chối." });
                }

                using (var connection = SqlConnectionFactory.CreateConnection())
                {
                    connection.Open();
                    using (var cmd = connection.CreateCommand())
                    {
                        cmd.CommandText = "UPDATE Posts SET Status = N'Từ chối', RejectionReason = @reason WHERE PostID = @id";
                        cmd.Parameters.AddWithValue("@id", id);
                        cmd.Parameters.AddWithValue("@reason", reason);
                        var rows = cmd.ExecuteNonQuery();
                        return Json(new { success = rows > 0, message = rows > 0 ? "Đã từ chối bài viết." : "Không tìm thấy bài viết." });
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        // POST: SuperAdmin/Post/UpdatePost
        [HttpPost]
        public JsonResult UpdatePost(int id, string title, string statusVal, string note)
        {
            try
            {
                using (var connection = SqlConnectionFactory.CreateConnection())
                {
                    connection.Open();
                    using (var cmd = connection.CreateCommand())
                    {
                        cmd.CommandText = @"UPDATE Posts SET Title = @title, Status = @status WHERE PostID = @id";
                        cmd.Parameters.AddWithValue("@id", id);
                        cmd.Parameters.AddWithValue("@title", title ?? "");
                        cmd.Parameters.AddWithValue("@status", statusVal ?? "Nháp");
                        var rows = cmd.ExecuteNonQuery();
                        return Json(new { success = rows > 0, message = rows > 0 ? "Đã cập nhật bài viết thành công!" : "Không tìm thấy bài viết." });
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        // GET: SuperAdmin/Post/GetDetail/5
        public JsonResult GetDetail(int id)
        {
            try
            {
                using (var connection = SqlConnectionFactory.CreateConnection())
                {
                    connection.Open();
                    using (var cmd = connection.CreateCommand())
                    {
                        cmd.CommandText = @"
SELECT TOP 1
    p.PostID, p.Title, p.Slug, p.Content, p.Status, p.ThumbnailURL,
    p.ViewCount, p.CreatedAt, p.RejectionReason,
    c.CategoryName,
    a.FullName AS AuthorName
FROM Posts p
INNER JOIN Categories c ON p.CategoryID = c.CategoryID
INNER JOIN Admins a ON p.CreatedBy = a.AdminID
WHERE p.PostID = @id;";
                        cmd.Parameters.AddWithValue("@id", id);
                        using (var reader = cmd.ExecuteReader(CommandBehavior.SingleRow))
                        {
                            if (reader.Read())
                            {
                                return Json(new
                                {
                                    success = true,
                                    data = new
                                    {
                                        id = Convert.ToInt32(reader["PostID"]),
                                        title = reader["Title"]?.ToString() ?? "",
                                        content = reader["Content"]?.ToString() ?? "",
                                        status = reader["Status"]?.ToString() ?? "",
                                        thumbnail = reader["ThumbnailURL"] != DBNull.Value ? reader["ThumbnailURL"]?.ToString() : "",
                                        viewCount = Convert.ToInt32(reader["ViewCount"]),
                                        createdAt = reader["CreatedAt"] != DBNull.Value ? Convert.ToDateTime(reader["CreatedAt"]).ToString("dd/MM/yyyy") : "",
                                        rejectionReason = reader["RejectionReason"] != DBNull.Value ? reader["RejectionReason"]?.ToString() : "",
                                        category = reader["CategoryName"]?.ToString() ?? "",
                                        author = reader["AuthorName"]?.ToString() ?? ""
                                    }
                                }, JsonRequestBehavior.AllowGet);
                            }
                        }
                    }
                }
                return Json(new { success = false, message = "Không tìm thấy bài viết." }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}
