using System.Configuration;
using System.Data.SqlClient;

namespace FTECH_THUONGMAIDIENTU.Infrastructure
{
    public static class SqlConnectionFactory
    {
        public static SqlConnection CreateConnection()
        {
            var connectionString = ConfigurationManager.ConnectionStrings["FTechAffiliateDb"]?.ConnectionString;
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ConfigurationErrorsException("Missing connection string 'FTechAffiliateDb'.");
            }

            return new SqlConnection(connectionString);
        }
    }
}