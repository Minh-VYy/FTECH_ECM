using System;
using System.Data;
using System.Data.SqlClient;
using FTECH_THUONGMAIDIENTU.Infrastructure;
using FTECH_THUONGMAIDIENTU.Models.Auth;

namespace FTECH_THUONGMAIDIENTU.Data
{
    public class AccountRepository
    {
        public AuthResult AuthenticatePublicAccount(AccountAuthRequest request)
        {
            var adminResult = AuthenticateAdminInternal(request.Identifier, request.Password, false);
            if (adminResult.Success)
            {
                return adminResult;
            }

            using (var connection = SqlConnectionFactory.CreateConnection())
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"
SELECT TOP 1 MemberID, FullName, Email, PasswordHash, Status
FROM Members
WHERE Email = @Identifier";
                command.Parameters.AddWithValue("@Identifier", request.Identifier ?? string.Empty);

                connection.Open();
                using (var reader = command.ExecuteReader(CommandBehavior.SingleRow))
                {
                    if (!reader.Read())
                    {
                        return Fail("Tài khoản hoặc email không tồn tại.");
                    }

                    var storedPassword = reader["PasswordHash"]?.ToString();
                    var status = reader["Status"]?.ToString();

                    if (!string.Equals(status, "Hoạt động", StringComparison.OrdinalIgnoreCase) &&
                        !string.Equals(status, "Chưa xác nhận", StringComparison.OrdinalIgnoreCase))
                    {
                        return Fail("Tài khoản đang bị khóa hoặc chưa được xác nhận.");
                    }

                    if (!PasswordHasher.Verify(request.Password, storedPassword))
                    {
                        return Fail("Mật khẩu không đúng.");
                    }

                    return new AuthResult
                    {
                        Success = true,
                        Message = "Xác thực thành công.",
                        DisplayName = reader["FullName"]?.ToString(),
                        Email = reader["Email"]?.ToString(),
                        RoleKey = RoleKeys.Customer,
                        RedirectUrl = "/"
                    };
                }
            }
        }

        public AuthResult AuthenticateAdmin(AccountAuthRequest request)
        {
            return AuthenticateAdminInternal(request.Identifier, request.Password, true);
        }

        public AuthResult RegisterMember(RegisterMemberRequest request)
        {
            if (request == null)
            {
                return Fail("Dữ liệu đăng ký không hợp lệ.");
            }

            var email = (request.Email ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(email))
            {
                return Fail("Vui lòng nhập email.");
            }

            using (var connection = SqlConnectionFactory.CreateConnection())
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"
IF EXISTS (SELECT 1 FROM Members WHERE Email = @Email)
BEGIN
    SELECT CAST(1 AS BIT) AS IsDuplicate;
    RETURN;
END

SELECT CAST(0 AS BIT) AS IsDuplicate;";

                command.Parameters.AddWithValue("@FullName", BuildFullName(request.LastName, request.FirstName));
                command.Parameters.AddWithValue("@Email", email);
                command.Parameters.AddWithValue("@PasswordHash", PasswordHasher.Hash(request.Password ?? string.Empty));

                connection.Open();
                using (var reader = command.ExecuteReader(CommandBehavior.SingleRow))
                {
                    if (!reader.Read())
                    {
                        return Fail("Không thể kiểm tra trạng thái email.");
                    }

                    if (reader.GetBoolean(0))
                    {
                        return Fail("Email đã tồn tại. Vui lòng dùng email khác.");
                    }
                }

                using (var insertCommand = connection.CreateCommand())
                {
                    insertCommand.CommandText = @"
INSERT INTO Members (FullName, Email, PasswordHash, AvatarURL, Status)
VALUES (@FullName, @Email, @PasswordHash, 'default-avatar.png', N'Chưa xác nhận');";
                    insertCommand.Parameters.AddWithValue("@FullName", BuildFullName(request.LastName, request.FirstName));
                    insertCommand.Parameters.AddWithValue("@Email", email);
                    insertCommand.Parameters.AddWithValue("@PasswordHash", PasswordHasher.Hash(request.Password ?? string.Empty));
                    insertCommand.ExecuteNonQuery();
                }

                return new AuthResult
                {
                    Success = true,
                    Message = "Đăng ký thành công. Vui lòng đăng nhập để tiếp tục.",
                    RoleKey = RoleKeys.Customer,
                    RedirectUrl = "/login.html"
                };
            }
        }

        public AuthResult ResetPassword(ResetPasswordRequest request)
        {
            if (request == null)
            {
                return Fail("Dữ liệu đặt lại mật khẩu không hợp lệ.");
            }

            var email = (request.Email ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(email))
            {
                return Fail("Vui lòng nhập email.");
            }

            var passwordHash = PasswordHasher.Hash(request.NewPassword ?? string.Empty);

            using (var connection = SqlConnectionFactory.CreateConnection())
            {
                connection.Open();

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = @"
UPDATE Members
SET PasswordHash = @PasswordHash
WHERE Email = @Email;

IF @@ROWCOUNT = 0
BEGIN
    UPDATE Admins
    SET PasswordHash = @PasswordHash
    WHERE Email = @Email;
END;

SELECT CASE WHEN @@ROWCOUNT > 0 THEN 1 ELSE 0 END;";
                    command.Parameters.AddWithValue("@Email", email);
                    command.Parameters.AddWithValue("@PasswordHash", passwordHash);

                    var affected = Convert.ToInt32(command.ExecuteScalar());
                    if (affected > 0)
                    {
                        return new AuthResult
                        {
                            Success = true,
                            Message = "Mật khẩu đã được cập nhật.",
                            RedirectUrl = "/login.html"
                        };
                    }
                }

                return Fail("Không tìm thấy tài khoản với email này.");
            }
        }

        private static AuthResult AuthenticateAdminInternal(string identifier, string password, bool adminOnly)
        {
            using (var connection = SqlConnectionFactory.CreateConnection())
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"
SELECT TOP 1 a.AdminID, a.FullName, a.Email, a.PasswordHash, a.Status, r.RoleName
FROM Admins a
INNER JOIN Roles r ON a.RoleID = r.RoleID
WHERE a.Email = @Identifier OR a.FullName = @Identifier";
                command.Parameters.AddWithValue("@Identifier", identifier ?? string.Empty);

                connection.Open();
                using (var reader = command.ExecuteReader(CommandBehavior.SingleRow))
                {
                    if (!reader.Read())
                    {
                        return Fail(adminOnly ? "Không tìm thấy tài khoản quản trị." : "Tài khoản hoặc email không tồn tại.");
                    }

                    var storedPassword = reader["PasswordHash"]?.ToString();
                    var status = reader["Status"]?.ToString();
                    var roleName = reader["RoleName"]?.ToString();

                    if (!string.Equals(status, "Hoạt động", StringComparison.OrdinalIgnoreCase))
                    {
                        return Fail("Tài khoản đang bị khóa.");
                    }

                    if (!PasswordHasher.Verify(password, storedPassword))
                    {
                        return Fail("Mật khẩu không đúng.");
                    }

                    return new AuthResult
                    {
                        Success = true,
                        Message = "Xác thực thành công.",
                        DisplayName = reader["FullName"]?.ToString(),
                        Email = reader["Email"]?.ToString(),
                        RoleKey = MapRoleKey(roleName),
                        RedirectUrl = MapRoleRedirect(roleName)
                    };
                }
            }
        }

        private static string BuildFullName(string lastName, string firstName)
        {
            var fullName = string.Format("{0} {1}", lastName ?? string.Empty, firstName ?? string.Empty).Trim();
            return string.IsNullOrWhiteSpace(fullName) ? "Người dùng mới" : fullName;
        }

        private static AuthResult Fail(string message)
        {
            return new AuthResult
            {
                Success = false,
                Message = message
            };
        }

        private static string MapRoleKey(string roleName)
        {
            if (string.IsNullOrWhiteSpace(roleName))
            {
                return RoleKeys.Customer;
            }

            var normalized = roleName.Trim().Replace(" ", "").ToLowerInvariant();
            switch (normalized)
            {
                case "superadmin":
                    return RoleKeys.SuperAdmin;
                case "contentmanager":
                    return RoleKeys.ContentManager;
                case "affiliatemanager":
                    return RoleKeys.AffiliateManager;
                case "useraccountmanager":
                    return RoleKeys.UserAccountManager;
                default:
                    return RoleKeys.Customer;
            }
        }

        private static string MapRoleRedirect(string roleName)
        {
            if (string.IsNullOrWhiteSpace(roleName))
            {
                return "/";
            }

            var normalized = roleName.Trim().Replace(" ", "").ToLowerInvariant();
            switch (normalized)
            {
                case "superadmin":
                    return "/Admin/Dashboard";
                case "contentmanager":
                    return "/ContentManager/Post";
                case "affiliatemanager":
                    return "/AffiliateManager/Dashboard";
                case "useraccountmanager":
                    return "/SuperAdmin/AdminAccount";
                default:
                    return "/";
            }
        }
    }
}