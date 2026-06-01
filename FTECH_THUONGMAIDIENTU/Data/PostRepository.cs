using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using FTECH_THUONGMAIDIENTU.Models.ContentManager;
using FTECH_THUONGMAIDIENTU.Infrastructure;
using FTECH_THUONGMAIDIENTU.Models.Dashboard;
using FTECH_THUONGMAIDIENTU.Models.Posts;

namespace FTECH_THUONGMAIDIENTU.Data
{
    public class PostRepository
    {
        public IList<PostSummary> GetPostsByCreator(int adminId)
        {
            var posts = new List<PostSummary>();

            using (var connection = SqlConnectionFactory.CreateConnection())
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"
SELECT
    p.PostID,
    p.Title,
    p.Slug,
    p.ThumbnailURL,
    c.CategoryName,
    p.Status,
    p.RejectionReason,
    p.ViewCount,
    ISNULL(r.RatingCount, 0) AS RatingCount,
    ISNULL(r.AverageRating, 0) AS AverageRating,
    p.CreatedAt
FROM Posts p
INNER JOIN Categories c ON p.CategoryID = c.CategoryID
LEFT JOIN (
    SELECT
        PostID,
        COUNT(*) AS RatingCount,
        AVG(CAST(RatingStar AS FLOAT)) AS AverageRating
    FROM Ratings
    GROUP BY PostID
) r ON p.PostID = r.PostID
WHERE p.CreatedBy = @CreatedBy
ORDER BY p.CreatedAt DESC;";
                command.Parameters.AddWithValue("@CreatedBy", adminId);

                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        posts.Add(MapPostRow(reader));
                    }
                }
            }

            return posts;
        }

        public IList<CategoryOption> GetCategories()
        {
            var categories = new List<CategoryOption>();

            using (var connection = SqlConnectionFactory.CreateConnection())
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"
SELECT CategoryID, CategoryName
FROM Categories
ORDER BY CategoryName;";

                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        categories.Add(new CategoryOption
                        {
                            CategoryID = Convert.ToInt32(reader["CategoryID"]),
                            CategoryName = reader["CategoryName"]?.ToString()
                        });
                    }
                }
            }

            return categories;
        }

        public IList<CategorySummary> GetCategoriesWithCounts()
        {
            var categories = new List<CategorySummary>();

            using (var connection = SqlConnectionFactory.CreateConnection())
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"
SELECT
    c.CategoryID,
    c.CategoryName,
    COUNT(p.PostID) AS PostCount
FROM Categories c
LEFT JOIN Posts p ON p.CategoryID = c.CategoryID
GROUP BY c.CategoryID, c.CategoryName
ORDER BY c.CategoryName;";

                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        categories.Add(new CategorySummary
                        {
                            CategoryID = Convert.ToInt32(reader["CategoryID"]),
                            CategoryName = reader["CategoryName"]?.ToString(),
                            PostCount = Convert.ToInt32(reader["PostCount"])
                        });
                    }
                }
            }

            return categories;
        }

        public IList<CommentSummary> GetRecentComments(int take)
        {
            var comments = new List<CommentSummary>();

            using (var connection = SqlConnectionFactory.CreateConnection())
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"
SELECT TOP (@Take)
    c.CommentID,
    c.PostID,
    p.Slug AS PostSlug,
    p.Title AS PostTitle,
    ISNULL(m.FullName, N'Khách') AS MemberName,
    ISNULL(m.AvatarURL, N'default-avatar.png') AS MemberAvatarURL,
    c.Content,
    c.CreatedAt
FROM Comments c
LEFT JOIN Members m ON c.MemberID = m.MemberID
LEFT JOIN Posts p ON c.PostID = p.PostID
ORDER BY c.CreatedAt DESC;";
                command.Parameters.AddWithValue("@Take", Math.Max(1, take));

                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        comments.Add(new CommentSummary
                        {
                            CommentID = Convert.ToInt32(reader["CommentID"]),
                            PostID = reader["PostID"] == DBNull.Value ? (int?)null : Convert.ToInt32(reader["PostID"]),
                            PostSlug = reader["PostSlug"]?.ToString(),
                            PostTitle = reader["PostTitle"]?.ToString(),
                            MemberName = reader["MemberName"]?.ToString(),
                            MemberAvatarURL = reader["MemberAvatarURL"]?.ToString(),
                            Content = reader["Content"]?.ToString(),
                            CreatedAt = Convert.ToDateTime(reader["CreatedAt"])
                        });
                    }
                }
            }

            return comments;
        }

        public PostEditorViewModel GetForEdit(int postId, int adminId)
        {
            using (var connection = SqlConnectionFactory.CreateConnection())
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"
SELECT TOP 1
    PostID,
    Title,
    Slug,
    Content,
    ThumbnailURL,
    CategoryID,
    Status,
    RejectionReason
FROM Posts
WHERE PostID = @PostID AND CreatedBy = @CreatedBy;";
                command.Parameters.AddWithValue("@PostID", postId);
                command.Parameters.AddWithValue("@CreatedBy", adminId);

                connection.Open();
                using (var reader = command.ExecuteReader(CommandBehavior.SingleRow))
                {
                    if (reader.Read())
                    {
                        return new PostEditorViewModel
                        {
                            PostID = Convert.ToInt32(reader["PostID"]),
                            Title = reader["Title"]?.ToString(),
                            Slug = reader["Slug"]?.ToString(),
                            Content = reader["Content"]?.ToString(),
                            ThumbnailURL = reader["ThumbnailURL"]?.ToString(),
                            CategoryID = Convert.ToInt32(reader["CategoryID"]),
                            Status = reader["Status"]?.ToString(),
                            RejectionReason = reader["RejectionReason"]?.ToString(),
                            Categories = GetCategories()
                        };
                    }
                }
            }

            return new PostEditorViewModel
            {
                Categories = GetCategories(),
                Status = "Nháp"
            };
        }

        public int CreateDraft(PostEditorRequest request, int adminId)
        {
            using (var connection = SqlConnectionFactory.CreateConnection())
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"
INSERT INTO Posts (Title, Slug, Content, ThumbnailURL, CategoryID, CreatedBy, Status, RejectionReason, ViewCount, CreatedAt)
VALUES (@Title, @Slug, @Content, @ThumbnailURL, @CategoryID, @CreatedBy, @Status, NULL, 0, GETDATE());

SELECT CAST(SCOPE_IDENTITY() AS INT);";
                command.Parameters.AddWithValue("@Title", request.Title ?? string.Empty);
                command.Parameters.AddWithValue("@Slug", request.Slug ?? string.Empty);
                command.Parameters.AddWithValue("@Content", request.Content ?? string.Empty);
                command.Parameters.AddWithValue("@ThumbnailURL", string.IsNullOrWhiteSpace(request.ThumbnailURL) ? (object)DBNull.Value : request.ThumbnailURL);
                command.Parameters.AddWithValue("@CategoryID", request.CategoryID);
                command.Parameters.AddWithValue("@CreatedBy", adminId);
                command.Parameters.AddWithValue("@Status", string.IsNullOrWhiteSpace(request.Status) ? "Nháp" : request.Status);

                connection.Open();
                return Convert.ToInt32(command.ExecuteScalar());
            }
        }

        public bool UpdatePost(PostEditorRequest request, int adminId)
        {
            using (var connection = SqlConnectionFactory.CreateConnection())
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"
UPDATE Posts
SET Title = @Title,
    Slug = @Slug,
    Content = @Content,
    ThumbnailURL = @ThumbnailURL,
    CategoryID = @CategoryID,
    Status = @Status,
    RejectionReason = @RejectionReason
WHERE PostID = @PostID AND CreatedBy = @CreatedBy;";
                command.Parameters.AddWithValue("@PostID", request.PostID ?? 0);
                command.Parameters.AddWithValue("@CreatedBy", adminId);
                command.Parameters.AddWithValue("@Title", request.Title ?? string.Empty);
                command.Parameters.AddWithValue("@Slug", request.Slug ?? string.Empty);
                command.Parameters.AddWithValue("@Content", request.Content ?? string.Empty);
                command.Parameters.AddWithValue("@ThumbnailURL", string.IsNullOrWhiteSpace(request.ThumbnailURL) ? (object)DBNull.Value : request.ThumbnailURL);
                command.Parameters.AddWithValue("@CategoryID", request.CategoryID);
                command.Parameters.AddWithValue("@Status", string.IsNullOrWhiteSpace(request.Status) ? "Nháp" : request.Status);
                command.Parameters.AddWithValue("@RejectionReason", string.IsNullOrWhiteSpace(request.RejectionReason) ? (object)DBNull.Value : request.RejectionReason);

                connection.Open();
                return command.ExecuteNonQuery() > 0;
            }
        }

        public IList<PostSummary> GetPublishedPosts(int take)
        {
            var posts = new List<PostSummary>();

            using (var connection = SqlConnectionFactory.CreateConnection())
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"
SELECT TOP (@Take)
    p.PostID,
    p.Title,
    p.Slug,
    p.Content,
    p.ThumbnailURL,
    c.CategoryName,
    p.Status,
    p.ViewCount,
    ISNULL(r.RatingCount, 0) AS RatingCount,
    ISNULL(r.AverageRating, 0) AS AverageRating,
    p.CreatedAt
FROM Posts p
INNER JOIN Categories c ON p.CategoryID = c.CategoryID
LEFT JOIN (
    SELECT
        PostID,
        COUNT(*) AS RatingCount,
        AVG(CAST(RatingStar AS FLOAT)) AS AverageRating
    FROM Ratings
    GROUP BY PostID
) r ON p.PostID = r.PostID
WHERE p.Status = N'Đã xuất bản'
ORDER BY p.CreatedAt DESC;";
                command.Parameters.AddWithValue("@Take", Math.Max(1, take));

                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        posts.Add(MapPublished(reader));
                    }
                }
            }

            return posts;
        }

        public IList<PostSummary> GetRecentPosts(int take, string searchTerm = null)
        {
            var posts = new List<PostSummary>();
            var term = string.IsNullOrWhiteSpace(searchTerm) ? string.Empty : searchTerm.Trim();

            using (var connection = SqlConnectionFactory.CreateConnection())
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"
SELECT TOP (@Take)
    p.PostID,
    p.Title,
    p.Slug,
    p.Content,
    p.ThumbnailURL,
    c.CategoryName,
    p.Status,
    p.RejectionReason,
    p.ViewCount,
    ISNULL(r.RatingCount, 0) AS RatingCount,
    ISNULL(r.AverageRating, 0) AS AverageRating,
    p.CreatedAt
FROM Posts p
INNER JOIN Categories c ON p.CategoryID = c.CategoryID
LEFT JOIN (
    SELECT
        PostID,
        COUNT(*) AS RatingCount,
        AVG(CAST(RatingStar AS FLOAT)) AS AverageRating
    FROM Ratings
    GROUP BY PostID
) r ON p.PostID = r.PostID
    WHERE (
        @SearchTerm = N''
        OR p.Title LIKE N'%' + @SearchTerm + N'%'
        OR p.Content LIKE N'%' + @SearchTerm + N'%'
        OR c.CategoryName LIKE N'%' + @SearchTerm + N'%'
    )
ORDER BY p.CreatedAt DESC;";
                command.Parameters.AddWithValue("@Take", Math.Max(1, take));
                command.Parameters.AddWithValue("@SearchTerm", term);

                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        posts.Add(MapPostRow(reader));
                    }
                }
            }

            return posts;
        }

        public PostSummary GetTopPost()
        {
            using (var connection = SqlConnectionFactory.CreateConnection())
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"
SELECT TOP 1
    p.PostID,
    p.Title,
    p.Slug,
    p.Content,
    p.ThumbnailURL,
    c.CategoryName,
    p.Status,
    p.RejectionReason,
    p.ViewCount,
    ISNULL(r.RatingCount, 0) AS RatingCount,
    ISNULL(r.AverageRating, 0) AS AverageRating,
    p.CreatedAt
FROM Posts p
INNER JOIN Categories c ON p.CategoryID = c.CategoryID
LEFT JOIN (
    SELECT
        PostID,
        COUNT(*) AS RatingCount,
        AVG(CAST(RatingStar AS FLOAT)) AS AverageRating
    FROM Ratings
    GROUP BY PostID
) r ON p.PostID = r.PostID
ORDER BY p.ViewCount DESC, p.CreatedAt DESC;";

                connection.Open();
                using (var reader = command.ExecuteReader(CommandBehavior.SingleRow))
                {
                    if (reader.Read())
                    {
                        return MapPostRow(reader);
                    }
                }
            }

            return null;
        }

        public PostSummary GetTopPublishedPost()
        {
            using (var connection = SqlConnectionFactory.CreateConnection())
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"
SELECT TOP 1
    p.PostID,
    p.Title,
    p.Slug,
    p.Content,
    p.ThumbnailURL,
    c.CategoryName,
    p.Status,
    p.ViewCount,
    ISNULL(r.RatingCount, 0) AS RatingCount,
    ISNULL(r.AverageRating, 0) AS AverageRating,
    p.CreatedAt
FROM Posts p
INNER JOIN Categories c ON p.CategoryID = c.CategoryID
LEFT JOIN (
    SELECT
        PostID,
        COUNT(*) AS RatingCount,
        AVG(CAST(RatingStar AS FLOAT)) AS AverageRating
    FROM Ratings
    GROUP BY PostID
) r ON p.PostID = r.PostID
WHERE p.Status = N'Đã xuất bản'
ORDER BY p.ViewCount DESC, p.CreatedAt DESC;";

                connection.Open();
                using (var reader = command.ExecuteReader(CommandBehavior.SingleRow))
                {
                    if (reader.Read())
                    {
                        return MapPublished(reader);
                    }
                }
            }

            return null;
        }

        public PostSummary GetPublishedPost(int? postId, string slug)
        {
            using (var connection = SqlConnectionFactory.CreateConnection())
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"
SELECT TOP 1
    p.PostID,
    p.Title,
    p.Slug,
    p.Content,
    p.ThumbnailURL,
    c.CategoryName,
    p.Status,
    p.ViewCount,
    ISNULL(r.RatingCount, 0) AS RatingCount,
    ISNULL(r.AverageRating, 0) AS AverageRating,
    p.CreatedAt,
    p.Pros,
    p.Cons,
    p.QuickSummary,
    p.SpecCpu,
    p.SpecRam,
    p.SpecStorage,
    p.SpecScreen,
    p.SpecPin,
    p.SpecWeight,
    p.SpecPorts,
    p.SpecTarget,
    p.ScoreDesign,
    p.ScorePerformance,
    p.ScoreBattery,
    p.ScoreScreen,
    p.ScoreValue
FROM Posts p
INNER JOIN Categories c ON p.CategoryID = c.CategoryID
LEFT JOIN (
    SELECT
        PostID,
        COUNT(*) AS RatingCount,
        AVG(CAST(RatingStar AS FLOAT)) AS AverageRating
    FROM Ratings
    GROUP BY PostID
) r ON p.PostID = r.PostID
WHERE p.Status = N'Đã xuất bản'
  AND (
        (@PostID IS NOT NULL AND p.PostID = @PostID)
        OR
        (@Slug <> N'' AND p.Slug = @Slug)
      )
ORDER BY p.CreatedAt DESC;";
                command.Parameters.AddWithValue("@PostID", postId.HasValue ? (object)postId.Value : DBNull.Value);
                command.Parameters.AddWithValue("@Slug", string.IsNullOrWhiteSpace(slug) ? string.Empty : slug.Trim());

                connection.Open();
                using (var reader = command.ExecuteReader(CommandBehavior.SingleRow))
                {
                    if (reader.Read())
                    {
                        var post = MapPublished(reader);
                        PopulateProductPricesAndComments(post);
                        return post;
                    }
                }
            }

            return null;
        }

        public PostSummary GetPost(int? postId, string slug)
        {
            using (var connection = SqlConnectionFactory.CreateConnection())
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"
SELECT TOP 1
    p.PostID,
    p.Title,
    p.Slug,
    p.Content,
    p.ThumbnailURL,
    c.CategoryName,
    p.Status,
    p.RejectionReason,
    p.ViewCount,
    ISNULL(r.RatingCount, 0) AS RatingCount,
    ISNULL(r.AverageRating, 0) AS AverageRating,
    p.CreatedAt,
    p.Pros,
    p.Cons,
    p.QuickSummary,
    p.SpecCpu,
    p.SpecRam,
    p.SpecStorage,
    p.SpecScreen,
    p.SpecPin,
    p.SpecWeight,
    p.SpecPorts,
    p.SpecTarget,
    p.ScoreDesign,
    p.ScorePerformance,
    p.ScoreBattery,
    p.ScoreScreen,
    p.ScoreValue
FROM Posts p
INNER JOIN Categories c ON p.CategoryID = c.CategoryID
LEFT JOIN (
    SELECT
        PostID,
        COUNT(*) AS RatingCount,
        AVG(CAST(RatingStar AS FLOAT)) AS AverageRating
    FROM Ratings
    GROUP BY PostID
) r ON p.PostID = r.PostID
WHERE (
        (@PostID IS NOT NULL AND p.PostID = @PostID)
        OR
        (@Slug <> N'' AND p.Slug = @Slug)
      )
ORDER BY p.CreatedAt DESC;";
                command.Parameters.AddWithValue("@PostID", postId.HasValue ? (object)postId.Value : DBNull.Value);
                command.Parameters.AddWithValue("@Slug", string.IsNullOrWhiteSpace(slug) ? string.Empty : slug.Trim());

                connection.Open();
                using (var reader = command.ExecuteReader(CommandBehavior.SingleRow))
                {
                    if (reader.Read())
                    {
                        var post = MapPublished(reader);
                        PopulateProductPricesAndComments(post);
                        return post;
                    }
                }
            }

            return null;
        }

        public void PopulateProductPricesAndComments(PostSummary post)
        {
            if (post == null) return;

            using (var connection = SqlConnectionFactory.CreateConnection())
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"
SELECT
    pp.PartnerID,
    ap.PartnerName,
    ap.WebsiteURL AS WebsiteUrl,
    pp.Price,
    pp.AffiliateUrl
FROM ProductPrices pp
INNER JOIN AffiliatePartners ap ON pp.PartnerID = ap.PartnerID
WHERE pp.PostID = @PostID
ORDER BY pp.Price ASC;";
                command.Parameters.AddWithValue("@PostID", post.PostID);

                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        post.Prices.Add(new ProductPriceInfo
                        {
                            PartnerID = Convert.ToInt32(reader["PartnerID"]),
                            PartnerName = reader["PartnerName"]?.ToString(),
                            WebsiteUrl = reader["WebsiteUrl"]?.ToString(),
                            Price = Convert.ToDecimal(reader["Price"]),
                            AffiliateUrl = reader["AffiliateUrl"]?.ToString()
                        });
                    }
                }
            }
        }

        public IList<CommentSummary> GetPostComments(int postId)
        {
            var comments = new List<CommentSummary>();

            using (var connection = SqlConnectionFactory.CreateConnection())
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"
SELECT
    c.CommentID,
    c.PostID,
    p.Slug AS PostSlug,
    p.Title AS PostTitle,
    ISNULL(m.FullName, N'Khách') AS MemberName,
    ISNULL(m.AvatarURL, N'default-avatar.png') AS MemberAvatarURL,
    c.Content,
    c.CreatedAt
FROM Comments c
LEFT JOIN Members m ON c.MemberID = m.MemberID
LEFT JOIN Posts p ON c.PostID = p.PostID
WHERE c.PostID = @PostID
ORDER BY c.CreatedAt DESC;";
                command.Parameters.AddWithValue("@PostID", postId);

                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        comments.Add(new CommentSummary
                        {
                            CommentID = Convert.ToInt32(reader["CommentID"]),
                            PostID = reader["PostID"] == DBNull.Value ? (int?)null : Convert.ToInt32(reader["PostID"]),
                            PostSlug = reader["PostSlug"]?.ToString(),
                            PostTitle = reader["PostTitle"]?.ToString(),
                            MemberName = reader["MemberName"]?.ToString(),
                            MemberAvatarURL = reader["MemberAvatarURL"]?.ToString(),
                            Content = reader["Content"]?.ToString(),
                            CreatedAt = Convert.ToDateTime(reader["CreatedAt"])
                        });
                    }
                }
            }

            return comments;
        }

        private static bool HasColumn(SqlDataReader reader, string columnName)
        {
            for (int i = 0; i < reader.FieldCount; i++)
            {
                if (reader.GetName(i).Equals(columnName, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }

        private static PostSummary MapPublished(SqlDataReader reader)
        {
            var post = new PostSummary
            {
                PostID = Convert.ToInt32(reader["PostID"]),
                Title = reader["Title"]?.ToString(),
                Slug = reader["Slug"]?.ToString(),
                Content = reader["Content"]?.ToString(),
                ThumbnailURL = reader["ThumbnailURL"]?.ToString(),
                CategoryName = reader["CategoryName"]?.ToString(),
                Status = reader["Status"]?.ToString(),
                ViewCount = Convert.ToInt32(reader["ViewCount"]),
                RatingCount = Convert.ToInt32(reader["RatingCount"]),
                AverageRating = Convert.ToDouble(reader["AverageRating"]),
                CreatedAt = Convert.ToDateTime(reader["CreatedAt"])
            };

            if (HasColumn(reader, "Pros")) post.Pros = reader["Pros"] == DBNull.Value ? null : reader["Pros"]?.ToString();
            if (HasColumn(reader, "Cons")) post.Cons = reader["Cons"] == DBNull.Value ? null : reader["Cons"]?.ToString();
            if (HasColumn(reader, "QuickSummary")) post.QuickSummary = reader["QuickSummary"] == DBNull.Value ? null : reader["QuickSummary"]?.ToString();

            if (HasColumn(reader, "SpecCpu")) post.SpecCpu = reader["SpecCpu"] == DBNull.Value ? null : reader["SpecCpu"]?.ToString();
            if (HasColumn(reader, "SpecRam")) post.SpecRam = reader["SpecRam"] == DBNull.Value ? null : reader["SpecRam"]?.ToString();
            if (HasColumn(reader, "SpecStorage")) post.SpecStorage = reader["SpecStorage"] == DBNull.Value ? null : reader["SpecStorage"]?.ToString();
            if (HasColumn(reader, "SpecScreen")) post.SpecScreen = reader["SpecScreen"] == DBNull.Value ? null : reader["SpecScreen"]?.ToString();
            if (HasColumn(reader, "SpecPin")) post.SpecPin = reader["SpecPin"] == DBNull.Value ? null : reader["SpecPin"]?.ToString();
            if (HasColumn(reader, "SpecWeight")) post.SpecWeight = reader["SpecWeight"] == DBNull.Value ? null : reader["SpecWeight"]?.ToString();
            if (HasColumn(reader, "SpecPorts")) post.SpecPorts = reader["SpecPorts"] == DBNull.Value ? null : reader["SpecPorts"]?.ToString();
            if (HasColumn(reader, "SpecTarget")) post.SpecTarget = reader["SpecTarget"] == DBNull.Value ? null : reader["SpecTarget"]?.ToString();

            if (HasColumn(reader, "ScoreDesign")) post.ScoreDesign = reader["ScoreDesign"] == DBNull.Value ? 0.0 : Convert.ToDouble(reader["ScoreDesign"]);
            if (HasColumn(reader, "ScorePerformance")) post.ScorePerformance = reader["ScorePerformance"] == DBNull.Value ? 0.0 : Convert.ToDouble(reader["ScorePerformance"]);
            if (HasColumn(reader, "ScoreBattery")) post.ScoreBattery = reader["ScoreBattery"] == DBNull.Value ? 0.0 : Convert.ToDouble(reader["ScoreBattery"]);
            if (HasColumn(reader, "ScoreScreen")) post.ScoreScreen = reader["ScoreScreen"] == DBNull.Value ? 0.0 : Convert.ToDouble(reader["ScoreScreen"]);
            if (HasColumn(reader, "ScoreValue")) post.ScoreValue = reader["ScoreValue"] == DBNull.Value ? 0.0 : Convert.ToDouble(reader["ScoreValue"]);

            return post;
        }

        private static PostSummary MapPostRow(SqlDataReader reader)
        {
            var post = new PostSummary
            {
                PostID = Convert.ToInt32(reader["PostID"]),
                Title = reader["Title"]?.ToString(),
                Slug = reader["Slug"]?.ToString(),
                ThumbnailURL = reader["ThumbnailURL"]?.ToString(),
                CategoryName = reader["CategoryName"]?.ToString(),
                Status = reader["Status"]?.ToString(),
                RejectionReason = reader["RejectionReason"]?.ToString(),
                ViewCount = Convert.ToInt32(reader["ViewCount"]),
                RatingCount = Convert.ToInt32(reader["RatingCount"]),
                AverageRating = Convert.ToDouble(reader["AverageRating"]),
                CreatedAt = Convert.ToDateTime(reader["CreatedAt"])
            };

            if (HasColumn(reader, "Pros")) post.Pros = reader["Pros"] == DBNull.Value ? null : reader["Pros"]?.ToString();
            if (HasColumn(reader, "Cons")) post.Cons = reader["Cons"] == DBNull.Value ? null : reader["Cons"]?.ToString();
            if (HasColumn(reader, "QuickSummary")) post.QuickSummary = reader["QuickSummary"] == DBNull.Value ? null : reader["QuickSummary"]?.ToString();

            if (HasColumn(reader, "SpecCpu")) post.SpecCpu = reader["SpecCpu"] == DBNull.Value ? null : reader["SpecCpu"]?.ToString();
            if (HasColumn(reader, "SpecRam")) post.SpecRam = reader["SpecRam"] == DBNull.Value ? null : reader["SpecRam"]?.ToString();
            if (HasColumn(reader, "SpecStorage")) post.SpecStorage = reader["SpecStorage"] == DBNull.Value ? null : reader["SpecStorage"]?.ToString();
            if (HasColumn(reader, "SpecScreen")) post.SpecScreen = reader["SpecScreen"] == DBNull.Value ? null : reader["SpecScreen"]?.ToString();
            if (HasColumn(reader, "SpecPin")) post.SpecPin = reader["SpecPin"] == DBNull.Value ? null : reader["SpecPin"]?.ToString();
            if (HasColumn(reader, "SpecWeight")) post.SpecWeight = reader["SpecWeight"] == DBNull.Value ? null : reader["SpecWeight"]?.ToString();
            if (HasColumn(reader, "SpecPorts")) post.SpecPorts = reader["SpecPorts"] == DBNull.Value ? null : reader["SpecPorts"]?.ToString();
            if (HasColumn(reader, "SpecTarget")) post.SpecTarget = reader["SpecTarget"] == DBNull.Value ? null : reader["SpecTarget"]?.ToString();

            if (HasColumn(reader, "ScoreDesign")) post.ScoreDesign = reader["ScoreDesign"] == DBNull.Value ? 0.0 : Convert.ToDouble(reader["ScoreDesign"]);
            if (HasColumn(reader, "ScorePerformance")) post.ScorePerformance = reader["ScorePerformance"] == DBNull.Value ? 0.0 : Convert.ToDouble(reader["ScorePerformance"]);
            if (HasColumn(reader, "ScoreBattery")) post.ScoreBattery = reader["ScoreBattery"] == DBNull.Value ? 0.0 : Convert.ToDouble(reader["ScoreBattery"]);
            if (HasColumn(reader, "ScoreScreen")) post.ScoreScreen = reader["ScoreScreen"] == DBNull.Value ? 0.0 : Convert.ToDouble(reader["ScoreScreen"]);
            if (HasColumn(reader, "ScoreValue")) post.ScoreValue = reader["ScoreValue"] == DBNull.Value ? 0.0 : Convert.ToDouble(reader["ScoreValue"]);

            return post;
        }
    }
}
