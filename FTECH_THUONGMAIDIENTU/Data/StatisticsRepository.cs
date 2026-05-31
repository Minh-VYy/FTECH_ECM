using System;
using System.Data.SqlClient;
using FTECH_THUONGMAIDIENTU.Infrastructure;
using FTECH_THUONGMAIDIENTU.Models.Dashboard;

namespace FTECH_THUONGMAIDIENTU.Data
{
    public class StatisticsRepository
    {
        public DashboardSummary GetSummary()
        {
            using (var connection = SqlConnectionFactory.CreateConnection())
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"
SELECT
    (SELECT COUNT(*) FROM Admins) AS AdminCount,
    (SELECT COUNT(*) FROM Members) AS MemberCount,
    (SELECT COUNT(*) FROM Posts) AS PostCount,
    (SELECT COUNT(*) FROM Posts WHERE Status = N'Đã xuất bản') AS PublishedPostCount,
    (SELECT COUNT(*) FROM Posts WHERE Status = N'Nháp') AS DraftPostCount,
    (SELECT COUNT(*) FROM Posts WHERE Status = N'Chờ duyệt') AS PendingPostCount,
    (SELECT COUNT(*) FROM AffiliatePartners) AS PartnerCount,
    (SELECT COUNT(*) FROM ClickTracking) AS ClickCount,
    (SELECT COUNT(*) FROM Comments) AS CommentCount,
    (SELECT COUNT(*) FROM Ratings) AS RatingCount;";

                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    if (!reader.Read())
                    {
                        return new DashboardSummary();
                    }

                    return new DashboardSummary
                    {
                        AdminCount = Convert.ToInt32(reader["AdminCount"]),
                        MemberCount = Convert.ToInt32(reader["MemberCount"]),
                        PostCount = Convert.ToInt32(reader["PostCount"]),
                        PublishedPostCount = Convert.ToInt32(reader["PublishedPostCount"]),
                        DraftPostCount = Convert.ToInt32(reader["DraftPostCount"]),
                        PendingPostCount = Convert.ToInt32(reader["PendingPostCount"]),
                        PartnerCount = Convert.ToInt32(reader["PartnerCount"]),
                        ClickCount = Convert.ToInt32(reader["ClickCount"]),
                        CommentCount = Convert.ToInt32(reader["CommentCount"]),
                        RatingCount = Convert.ToInt32(reader["RatingCount"])
                    };
                }
            }
        }
    }
}