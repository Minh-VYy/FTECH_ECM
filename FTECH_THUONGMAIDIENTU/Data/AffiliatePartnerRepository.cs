using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using FTECH_THUONGMAIDIENTU.Infrastructure;
using FTECH_THUONGMAIDIENTU.Models.Dashboard;

namespace FTECH_THUONGMAIDIENTU.Data
{
    public class AffiliatePartnerRepository
    {
        public IList<AffiliatePartnerSummary> GetActivePartners(int take = 12)
        {
            var partners = new List<AffiliatePartnerSummary>();

            using (var connection = SqlConnectionFactory.CreateConnection())
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"
SELECT TOP (@Take)
    PartnerID,
    PartnerName,
    WebsiteUrl,
    Status
FROM AffiliatePartners
WHERE Status = N'Hoạt động'
ORDER BY PartnerName;";
                command.Parameters.AddWithValue("@Take", Math.Max(1, take));

                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        partners.Add(new AffiliatePartnerSummary
                        {
                            PartnerID = Convert.ToInt32(reader["PartnerID"]),
                            PartnerName = reader["PartnerName"]?.ToString(),
                            WebsiteUrl = reader["WebsiteUrl"]?.ToString(),
                            Status = reader["Status"]?.ToString()
                        });
                    }
                }
            }

            return partners;
        }
    }
}