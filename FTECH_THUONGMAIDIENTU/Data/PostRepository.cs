using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using FTECH_THUONGMAIDIENTU.Models.ContentManager;
using FTECH_THUONGMAIDIENTU.Infrastructure;
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
        AVG(CAST(Score AS FLOAT)) AS AverageRating
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
        AVG(CAST(Score AS FLOAT)) AS AverageRating
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

        public IList<PostSummary> GetRecentPosts(int take)
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
        AVG(CAST(Score AS FLOAT)) AS AverageRating
    FROM Ratings
    GROUP BY PostID
) r ON p.PostID = r.PostID
ORDER BY p.CreatedAt DESC;";
                command.Parameters.AddWithValue("@Take", Math.Max(1, take));

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
        AVG(CAST(Score AS FLOAT)) AS AverageRating
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
        AVG(CAST(Score AS FLOAT)) AS AverageRating
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
    p.CreatedAt
FROM Posts p
INNER JOIN Categories c ON p.CategoryID = c.CategoryID
LEFT JOIN (
    SELECT
        PostID,
        COUNT(*) AS RatingCount,
        AVG(CAST(Score AS FLOAT)) AS AverageRating
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
                        return MapPublished(reader);
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
    p.CreatedAt
FROM Posts p
INNER JOIN Categories c ON p.CategoryID = c.CategoryID
LEFT JOIN (
    SELECT
        PostID,
        COUNT(*) AS RatingCount,
        AVG(CAST(Score AS FLOAT)) AS AverageRating
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
                        return MapPublished(reader);
                    }
                }
            }

            return null;
        }

        private static PostSummary MapPublished(SqlDataReader reader)
        {
            return new PostSummary
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
        }

        private static PostSummary MapPostRow(SqlDataReader reader)
        {
            return new PostSummary
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
        }
    }
}
