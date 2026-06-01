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

        // Pros, Cons, and QuickSummary
        public string Pros { get; set; }
        public string Cons { get; set; }
        public string QuickSummary { get; set; }

        // Technical specs
        public string SpecCpu { get; set; }
        public string SpecRam { get; set; }
        public string SpecStorage { get; set; }
        public string SpecScreen { get; set; }
        public string SpecPin { get; set; }
        public string SpecWeight { get; set; }
        public string SpecPorts { get; set; }
        public string SpecTarget { get; set; }

        // Scores
        public double ScoreDesign { get; set; }
        public double ScorePerformance { get; set; }
        public double ScoreBattery { get; set; }
        public double ScoreScreen { get; set; }
        public double ScoreValue { get; set; }

        // Partner pricing list
        public System.Collections.Generic.List<ProductPriceInfo> Prices { get; set; } = new System.Collections.Generic.List<ProductPriceInfo>();
    }

    public class ProductPriceInfo
    {
        public int PartnerID { get; set; }
        public string PartnerName { get; set; }
        public string WebsiteUrl { get; set; }
        public decimal Price { get; set; }
        public string AffiliateUrl { get; set; }
    }
}
