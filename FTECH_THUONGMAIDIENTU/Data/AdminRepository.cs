using System;
using System.Data;
using System.Data.SqlClient;
using FTECH_THUONGMAIDIENTU.Infrastructure;
using FTECH_THUONGMAIDIENTU.Models.Admin;

namespace FTECH_THUONGMAIDIENTU.Data
{
    public class AdminRepository
    {
        public AdminProfileSummary GetByEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                email = "content.baopt@ftech.vn";
            }

            using (var connection = SqlConnectionFactory.CreateConnection())
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"
SELECT TOP 1 a.AdminID, a.FullName, a.Email, r.RoleName, a.Status, a.CreatedAt
FROM Admins a
INNER JOIN Roles r ON a.RoleID = r.RoleID
WHERE a.Email = @Email OR a.FullName = @Email
ORDER BY a.CreatedAt DESC;";
                command.Parameters.AddWithValue("@Email", email);

                connection.Open();
                using (var reader = command.ExecuteReader(CommandBehavior.SingleRow))
                {
                    if (reader.Read())
                    {
                        return new AdminProfileSummary
                        {
                            AdminID = Convert.ToInt32(reader["AdminID"]),
                            FullName = reader["FullName"]?.ToString(),
                            Email = reader["Email"]?.ToString(),
                            RoleName = reader["RoleName"]?.ToString(),
                            Status = reader["Status"]?.ToString(),
                            CreatedAt = Convert.ToDateTime(reader["CreatedAt"])
                        };
                    }
                }
            }

            return new AdminProfileSummary
            {
                FullName = "Content Manager",
                Email = email,
                RoleName = "Content Manager",
                Status = "Hoạt động",
                CreatedAt = DateTime.Now
            };
        }
    }
}