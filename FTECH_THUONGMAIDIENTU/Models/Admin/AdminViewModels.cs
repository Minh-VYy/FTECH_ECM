using System;

namespace FTECH_THUONGMAIDIENTU.Models.Admin
{
    public class PostViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Slug { get; set; }
        public string Status { get; set; }
        public string Thumbnail { get; set; }
        public int ViewCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public string RejectionReason { get; set; }
        public string Category { get; set; }
        public string Author { get; set; }
        public int AffLinkCount { get; set; }
        public int ClickCount { get; set; }

        public string StatusCss
        {
            get
            {
                switch (Status)
                {
                    case "Chờ duyệt": return "sp-pending";
                    case "Đã xuất bản": return "sp-approved";
                    case "Từ chối": return "sp-rejected";
                    default: return "sp-draft";
                }
            }
        }

        public string StatusLabel
        {
            get
            {
                switch (Status)
                {
                    case "Chờ duyệt": return "Chờ duyệt";
                    case "Đã xuất bản": return "Đã duyệt";
                    case "Từ chối": return "Từ chối";
                    default: return "Bản nháp";
                }
            }
        }

        public string ThumbSafe
        {
            get
            {
                return string.IsNullOrEmpty(Thumbnail)
                    ? "https://images.unsplash.com/photo-1517694712202-14dd9538aa97?auto=format&fit=crop&w=200&q=60"
                    : Thumbnail;
            }
        }
    }

    public class PartnerViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Website { get; set; }
        public string Contact { get; set; }
        public decimal CommissionRate { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public int MonthClicks { get; set; }
        public int ActiveLinks { get; set; }

        public string StatusCss
        {
            get
            {
                switch (Status)
                {
                    case "Hoạt động": return "pcs-active";
                    case "Tạm ngưng": return "pcs-suspended";
                    case "Từ chối": return "pcs-rejected";
                    default: return "pcs-pending";
                }
            }
        }

        public string LogoUrl
        {
            get { return "https://ui-avatars.com/api/?name=" + Uri.EscapeDataString(Name ?? "P") + "&background=4a7dff&color=fff&size=80&bold=true"; }
        }

        public string WebsiteShort
        {
            get { return Website != null && Website.Length > 30 ? Website.Substring(0, 30) + "..." : Website; }
        }
    }

    public class AdminAccountViewModel
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public string Status { get; set; }
        public string StatusLabel { get; set; }
        public string Permissions { get; set; }
        public DateTime LastUpdated { get; set; }

        public string Avatar
        {
            get { return "https://i.pravatar.cc/120?img=" + (10 + (Id % 20)); }
        }

        public string RoleCss
        {
            get
            {
                if (Role == null) return "cat-tag";
                if (Role.Contains("Super")) return "cat-tag role-super";
                if (Role.Contains("Content")) return "cat-tag role-content";
                if (Role.Contains("Affiliate")) return "cat-tag role-affiliate";
                return "cat-tag";
            }
        }

        public string LockBtnCss
        {
            get { return Status == "Active" ? "act act-reject" : "act act-approve"; }
        }

        public string LockBtnLabel
        {
            get { return Status == "Active" ? "🔒" : "🔓"; }
        }

        public string StatusSpanCss
        {
            get { return Status == "Active" ? "sp sp-approved" : "sp sp-rejected"; }
        }
    }
}
