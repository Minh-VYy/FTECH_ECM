using System.Collections.Generic;
using FTECH_THUONGMAIDIENTU.Models.Posts;

namespace FTECH_THUONGMAIDIENTU.Models.ContentManager
{
    public class PostEditorViewModel
    {
        public int? PostID { get; set; }

        public string Title { get; set; }

        public string Slug { get; set; }

        public string Content { get; set; }

        public string ThumbnailURL { get; set; }

        public int CategoryID { get; set; }

        public string Status { get; set; }

        public string RejectionReason { get; set; }

        public IList<CategoryOption> Categories { get; set; } = new List<CategoryOption>();
    }

    public class PostListViewModel
    {
        public string Title { get; set; }

        public int TotalPosts { get; set; }

        public int DraftCount { get; set; }

        public int PendingCount { get; set; }

        public int ApprovedCount { get; set; }

        public int RejectedCount { get; set; }

        public string PostRowsHtml { get; set; }

        public IList<PostSummary> Posts { get; set; }
    }

}