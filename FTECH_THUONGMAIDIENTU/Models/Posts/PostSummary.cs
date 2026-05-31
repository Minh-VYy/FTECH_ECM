using System;

namespace FTECH_THUONGMAIDIENTU.Models.Posts
{
    public class PostSummary
    {
        public int PostID { get; set; }

        public string Title { get; set; }

        public string Slug { get; set; }

        public string ThumbnailURL { get; set; }

        public string Content { get; set; }

        public string CategoryName { get; set; }

        public string Status { get; set; }

        public string RejectionReason { get; set; }

        public int ViewCount { get; set; }

        public int RatingCount { get; set; }

        public double AverageRating { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
