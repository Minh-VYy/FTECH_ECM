using System;
using System.Data;
using System.Data.SqlClient;
using FTECH_THUONGMAIDIENTU.Infrastructure;
using FTECH_THUONGMAIDIENTU.Models.UserAccount;

namespace FTECH_THUONGMAIDIENTU.Data
{
    public class MemberRepository
    {
        public MemberProfileSummary GetByEmail(string email)
        {
            using (var connection = SqlConnectionFactory.CreateConnection())
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"
SELECT TOP 1
    m.MemberID,
    m.FullName,
    m.Email,
    m.AvatarURL,
    m.Status,
    m.CreatedAt,
    (SELECT COUNT(*) FROM ClickTracking ct WHERE ct.MemberID = m.MemberID) AS ClickCount,
    (SELECT COUNT(*) FROM Comments cm WHERE cm.MemberID = m.MemberID) AS CommentCount,
    (SELECT COUNT(*) FROM Ratings rt WHERE rt.MemberID = m.MemberID) AS RatingCount
FROM Members m
WHERE m.Email = @Email
ORDER BY m.CreatedAt DESC;";
                command.Parameters.AddWithValue("@Email", string.IsNullOrWhiteSpace(email) ? string.Empty : email);

                connection.Open();
                using (var reader = command.ExecuteReader(CommandBehavior.SingleRow))
                {
                    if (reader.Read())
                    {
                        return Map(reader);
                    }
                }
            }

            return GetFallbackMember();
        }

        public bool UpdateProfile(string email, MemberProfileUpdateRequest request)
        {
            if (string.IsNullOrWhiteSpace(email) || request == null)
            {
                return false;
            }

            using (var connection = SqlConnectionFactory.CreateConnection())
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"
UPDATE Members
SET FullName = @FullName,
    AvatarURL = @AvatarURL
WHERE Email = @Email;";
                command.Parameters.AddWithValue("@Email", email);
                command.Parameters.AddWithValue("@FullName", string.IsNullOrWhiteSpace(request.FullName) ? "Người dùng" : request.FullName.Trim());
                command.Parameters.AddWithValue("@AvatarURL", string.IsNullOrWhiteSpace(request.AvatarURL) ? "default-avatar.png" : request.AvatarURL.Trim());

                connection.Open();
                return command.ExecuteNonQuery() > 0;
            }
        }

        private static MemberProfileSummary GetFallbackMember()
        {
            using (var connection = SqlConnectionFactory.CreateConnection())
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"
SELECT TOP 1
    m.MemberID,
    m.FullName,
    m.Email,
    m.AvatarURL,
    m.Status,
    m.CreatedAt,
    (SELECT COUNT(*) FROM ClickTracking ct WHERE ct.MemberID = m.MemberID) AS ClickCount,
    (SELECT COUNT(*) FROM Comments cm WHERE cm.MemberID = m.MemberID) AS CommentCount,
    (SELECT COUNT(*) FROM Ratings rt WHERE rt.MemberID = m.MemberID) AS RatingCount
FROM Members m
ORDER BY m.CreatedAt DESC;";

                connection.Open();
                using (var reader = command.ExecuteReader(CommandBehavior.SingleRow))
                {
                    if (reader.Read())
                    {
                        return Map(reader);
                    }
                }
            }

            return new MemberProfileSummary
            {
                FullName = "Người dùng",
                Email = "user@example.com",
                AvatarURL = "default-avatar.png",
                Status = "Chưa xác nhận",
                CreatedAt = DateTime.Now
            };
        }

        private static MemberProfileSummary Map(SqlDataReader reader)
        {
            return new MemberProfileSummary
            {
                MemberID = Convert.ToInt32(reader["MemberID"]),
                FullName = reader["FullName"]?.ToString(),
                Email = reader["Email"]?.ToString(),
                AvatarURL = reader["AvatarURL"]?.ToString(),
                Status = reader["Status"]?.ToString(),
                CreatedAt = Convert.ToDateTime(reader["CreatedAt"]),
                ClickCount = Convert.ToInt32(reader["ClickCount"]),
                CommentCount = Convert.ToInt32(reader["CommentCount"]),
                RatingCount = Convert.ToInt32(reader["RatingCount"])
            };
        }
    }
}