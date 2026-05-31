using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using FTECH_THUONGMAIDIENTU.Data;
using FTECH_THUONGMAIDIENTU.Infrastructure;
using FTECH_THUONGMAIDIENTU.Models.Posts;
using FTECH_THUONGMAIDIENTU.Models.ContentManager;

namespace FTECH_THUONGMAIDIENTU.Areas.ContentManager.Controllers
{
    [FTECH_THUONGMAIDIENTU.Infrastructure.SessionRoleAuthorize(SessionKey = "AdminRole", AllowedRolesCsv = FTECH_THUONGMAIDIENTU.Infrastructure.RoleKeys.SuperAdmin + "," + FTECH_THUONGMAIDIENTU.Infrastructure.RoleKeys.ContentManager, LoginUrl = "/Admin/Account/Login")]
    public class PostController : Controller
    {
        private readonly AdminRepository adminRepository = new AdminRepository();
        private readonly PostRepository postRepository = new PostRepository();

        private int CurrentAdminId
        {
            get
            {
                var email = Session["AdminEmail"] as string;
                return adminRepository.GetByEmail(email).AdminID;
            }
        }

        public ActionResult Index()
        {
            var admin = adminRepository.GetByEmail(Session["AdminEmail"] as string);
            var posts = postRepository.GetPostsByCreator(admin.AdminID);
            ViewBag.Title = "Bài Viết Của Tôi";
            ViewBag.UserName = admin.FullName;
            ViewBag.UserRole = admin.RoleName;
            ViewBag.UserAvatar = "https://images.unsplash.com/photo-1494790108377-be9c29b29330?auto=format&fit=crop&w=120&q=80";
            var totalPosts = posts.Count;
            var draftCount = posts.Count(post => string.Equals(post.Status, "Nháp", System.StringComparison.OrdinalIgnoreCase));
            var pendingCount = posts.Count(post => string.Equals(post.Status, "Chờ duyệt", System.StringComparison.OrdinalIgnoreCase));
            var approvedCount = posts.Count(post => string.Equals(post.Status, "Đã xuất bản", System.StringComparison.OrdinalIgnoreCase));
            var rejectedCount = posts.Count(post => string.Equals(post.Status, "Từ chối", System.StringComparison.OrdinalIgnoreCase));

            return View(new PostListViewModel
            {
                Title = "Bài Viết Của Tôi",
                TotalPosts = totalPosts,
                DraftCount = draftCount,
                PendingCount = pendingCount,
                ApprovedCount = approvedCount,
                RejectedCount = rejectedCount,
                PostRowsHtml = BuildPostRowsHtml(posts),
                Posts = posts
            });
        }

        private static string BuildPostRowsHtml(System.Collections.Generic.IEnumerable<PostSummary> posts)
        {
            var builder = new StringBuilder();

            foreach (var post in posts)
            {
                var statusKey = GetStatusKey(post.Status);
                var rowClass = statusKey == "pending" ? "u-style-008" : statusKey == "rejected" ? "u-style-013" : string.Empty;
                var previewTitle = HttpUtility.JavaScriptStringEncode(post.Title ?? string.Empty);
                var previewCategory = HttpUtility.JavaScriptStringEncode(post.CategoryName ?? string.Empty);
                var previewStatus = HttpUtility.JavaScriptStringEncode(statusKey);
                var hasImage = !string.IsNullOrWhiteSpace(post.ThumbnailURL);
                var statusLabel = GetStatusLabel(statusKey);
                var postMeta = statusKey == "approved"
                    ? "Bài đã xuất bản"
                    : statusKey == "pending"
                        ? "Đang chờ Super Admin xét duyệt"
                        : statusKey == "rejected"
                            ? (string.IsNullOrWhiteSpace(post.RejectionReason) ? "Bài bị từ chối" : post.RejectionReason)
                            : "Bản nháp";

                builder.AppendLine($"<div class=\"t-row {rowClass}\" data-status=\"{statusKey}\">");
                builder.AppendLine("<div><input type=\"checkbox\" class=\"rck\" onchange=\"updBulk()\"></div>");
                builder.AppendLine("<div class=\"post-cell\">");
                builder.AppendLine("<div class=\"post-thumb\">");

                if (hasImage)
                {
                    builder.AppendLine($"<img src=\"{HttpUtility.HtmlAttributeEncode(post.ThumbnailURL)}\" alt=\"{HttpUtility.HtmlAttributeEncode(post.Title ?? string.Empty)}\">");
                }
                else
                {
                    builder.AppendLine("<div class=\"post-thumb u-style-011\">📝</div>");
                }

                builder.AppendLine("</div>");
                builder.AppendLine("<div>");
                builder.AppendLine($"<div class=\"post-title\">{HttpUtility.HtmlEncode(post.Title ?? string.Empty)}</div>");
                builder.AppendLine($"<div class=\"post-meta\">{HttpUtility.HtmlEncode(postMeta)}</div>");
                builder.AppendLine("</div>");
                builder.AppendLine("</div>");
                builder.AppendLine($"<div><span class=\"cat-tag\">{HttpUtility.HtmlEncode(post.CategoryName ?? string.Empty)}</span></div>");
                builder.AppendLine($"<div><span class=\"sp sp-{statusKey}\">{HttpUtility.HtmlEncode(statusLabel)}</span></div>");
                var metricText = statusKey == "approved" ? post.ViewCount.ToString("N0") : "—";
                builder.AppendLine("<div class=\"metric\">" + metricText + "</div>");
                builder.AppendLine($"<div class=\"u-style-006\">{post.CreatedAt:dd/MM/yyyy}</div>");
                builder.AppendLine("<div class=\"row-acts\">");
                builder.AppendLine($"<button class=\"act\" type=\"button\" onclick=\"openPreview('{previewTitle}','{previewStatus}','{previewCategory}')\" title=\"Xem trước\">👁️</button>");
                var disabledAttr = statusKey == "pending" ? " disabled=\"disabled\"" : string.Empty;
                builder.AppendLine("<button class=\"act act-edit\" type=\"button\" onclick=\"openEdit('" + previewTitle + "','" + previewStatus + "','" + previewCategory + "')\" title=\"Chỉnh sửa\"" + disabledAttr + ">✏️</button>");

                if (statusKey == "draft" || statusKey == "rejected")
                {
                    builder.AppendLine($"<button class=\"act act-send\" type=\"button\" onclick=\"openSend('{previewTitle}')\" title=\"Gửi duyệt\">📤</button>");
                }

                if (statusKey != "pending")
                {
                    builder.AppendLine($"<button class=\"act act-del\" type=\"button\" onclick=\"openDelete('{previewTitle}')\" title=\"Xóa\">🗑️</button>");
                }

                builder.AppendLine("</div>");
                builder.AppendLine("</div>");
            }

            return builder.ToString();
        }

        private static string GetStatusKey(string status)
        {
            switch (status)
            {
                case "Chờ duyệt":
                    return "pending";
                case "Đã xuất bản":
                    return "approved";
                case "Từ chối":
                    return "rejected";
                default:
                    return "draft";
            }
        }

        private static string GetStatusLabel(string statusKey)
        {
            switch (statusKey)
            {
                case "pending":
                    return "⏳ Chờ duyệt";
                case "approved":
                    return "✓ Đã duyệt";
                case "rejected":
                    return "✕ Từ chối";
                default:
                    return "📝 Bản nháp";
            }
        }

        public ActionResult Create()
        {
            ViewBag.Title = "Tạo Bài Viết Mới";
            var admin = adminRepository.GetByEmail(Session["AdminEmail"] as string);
            ViewBag.UserName = admin.FullName;
            ViewBag.UserRole = admin.RoleName;
            return View(new PostEditorViewModel
            {
                Categories = postRepository.GetCategories(),
                Status = "Nháp"
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(PostEditorRequest request)
        {
            var admin = adminRepository.GetByEmail(Session["AdminEmail"] as string);
            if (string.IsNullOrWhiteSpace(request.Title) || string.IsNullOrWhiteSpace(request.Content))
            {
                ModelState.AddModelError(string.Empty, "Tiêu đề và nội dung là bắt buộc.");
            }

            if (!ModelState.IsValid)
            {
                return View(new PostEditorViewModel
                {
                    Title = request.Title,
                    Slug = request.Slug,
                    Content = request.Content,
                    ThumbnailURL = request.ThumbnailURL,
                    CategoryID = request.CategoryID,
                    Status = string.IsNullOrWhiteSpace(request.Status) ? "Nháp" : request.Status,
                    Categories = postRepository.GetCategories()
                });
            }

            postRepository.CreateDraft(request, admin.AdminID);
            return RedirectToAction("Index");
        }

        public ActionResult Edit(int id)
        {
            ViewBag.Title = "Chỉnh Sửa Bài Viết";
            var admin = adminRepository.GetByEmail(Session["AdminEmail"] as string);
            ViewBag.UserName = admin.FullName;
            ViewBag.UserRole = admin.RoleName;
            return View(postRepository.GetForEdit(id, admin.AdminID));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(PostEditorRequest request)
        {
            var admin = adminRepository.GetByEmail(Session["AdminEmail"] as string);
            if (!ModelState.IsValid)
            {
                return View(postRepository.GetForEdit(request.PostID ?? 0, admin.AdminID));
            }

            postRepository.UpdatePost(request, admin.AdminID);
            return RedirectToAction("Index");
        }
    }
}
