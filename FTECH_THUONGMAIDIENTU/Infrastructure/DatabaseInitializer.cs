using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;

namespace FTECH_THUONGMAIDIENTU.Infrastructure
{
    public static class DatabaseInitializer
    {
        private static readonly object SyncRoot = new object();
        private static bool initialized;

        public static void EnsureCreated()
        {
            if (initialized)
            {
                return;
            }

            lock (SyncRoot)
            {
                if (initialized)
                {
                    return;
                }

                var configuredConnectionString = ConfigurationManager.ConnectionStrings["FTechAffiliateDb"]?.ConnectionString;
                if (string.IsNullOrWhiteSpace(configuredConnectionString))
                {
                    throw new ConfigurationErrorsException("Missing connection string 'FTechAffiliateDb'.");
                }

                var builder = new SqlConnectionStringBuilder(configuredConnectionString);
                var databaseName = builder.InitialCatalog;
                if (string.IsNullOrWhiteSpace(databaseName))
                {
                    throw new ConfigurationErrorsException("Connection string 'FTechAffiliateDb' must define Initial Catalog.");
                }

                EnsureDatabase(builder, databaseName);
                EnsureSchema(configuredConnectionString);
                SeedData(configuredConnectionString);

                initialized = true;
            }
        }

        private static void EnsureDatabase(SqlConnectionStringBuilder builder, string databaseName)
        {
            var masterBuilder = new SqlConnectionStringBuilder(builder.ConnectionString)
            {
                InitialCatalog = "master"
            };

            using (var connection = new SqlConnection(masterBuilder.ConnectionString))
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"
IF DB_ID(@DatabaseName) IS NULL
BEGIN
    DECLARE @Sql NVARCHAR(MAX) = N'CREATE DATABASE ' + QUOTENAME(@DatabaseName);
    EXEC (@Sql);
END;";
                command.Parameters.AddWithValue("@DatabaseName", databaseName);

                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        private static void EnsureSchema(string connectionString)
        {
            ExecuteNonQuery(connectionString, @"
IF OBJECT_ID('dbo.Roles', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Roles
    (
        RoleID INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Roles PRIMARY KEY,
        RoleName NVARCHAR(100) NOT NULL CONSTRAINT UQ_Roles_RoleName UNIQUE
    );
END;

IF OBJECT_ID('dbo.Admins', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Admins
    (
        AdminID INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Admins PRIMARY KEY,
        FullName NVARCHAR(150) NOT NULL,
        Email NVARCHAR(255) NOT NULL CONSTRAINT UQ_Admins_Email UNIQUE,
        PasswordHash NVARCHAR(255) NOT NULL,
        RoleID INT NOT NULL,
        Status NVARCHAR(50) NOT NULL CONSTRAINT DF_Admins_Status DEFAULT N'Hoạt động',
        CreatedAt DATETIME NOT NULL CONSTRAINT DF_Admins_CreatedAt DEFAULT GETDATE(),
        CONSTRAINT FK_Admins_Roles FOREIGN KEY (RoleID) REFERENCES dbo.Roles(RoleID)
    );
END;

IF OBJECT_ID('dbo.Members', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Members
    (
        MemberID INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Members PRIMARY KEY,
        FullName NVARCHAR(150) NOT NULL,
        Email NVARCHAR(255) NOT NULL CONSTRAINT UQ_Members_Email UNIQUE,
        PasswordHash NVARCHAR(255) NOT NULL,
        AvatarURL NVARCHAR(500) NULL,
        Status NVARCHAR(50) NOT NULL CONSTRAINT DF_Members_Status DEFAULT N'Chưa xác nhận',
        CreatedAt DATETIME NOT NULL CONSTRAINT DF_Members_CreatedAt DEFAULT GETDATE()
    );
END;

IF OBJECT_ID('dbo.Categories', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Categories
    (
        CategoryID INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Categories PRIMARY KEY,
        CategoryName NVARCHAR(150) NOT NULL CONSTRAINT UQ_Categories_CategoryName UNIQUE
    );
END;

IF OBJECT_ID('dbo.Posts', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Posts
    (
        PostID INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Posts PRIMARY KEY,
        Title NVARCHAR(255) NOT NULL,
        Slug NVARCHAR(255) NOT NULL,
        Content NVARCHAR(MAX) NOT NULL,
        ThumbnailURL NVARCHAR(500) NULL,
        CategoryID INT NOT NULL,
        CreatedBy INT NOT NULL,
        Status NVARCHAR(50) NOT NULL CONSTRAINT DF_Posts_Status DEFAULT N'Nháp',
        RejectionReason NVARCHAR(500) NULL,
        ViewCount INT NOT NULL CONSTRAINT DF_Posts_ViewCount DEFAULT 0,
        CreatedAt DATETIME NOT NULL CONSTRAINT DF_Posts_CreatedAt DEFAULT GETDATE(),
        CONSTRAINT FK_Posts_Categories FOREIGN KEY (CategoryID) REFERENCES dbo.Categories(CategoryID),
        CONSTRAINT FK_Posts_Admins FOREIGN KEY (CreatedBy) REFERENCES dbo.Admins(AdminID)
    );
END;

IF OBJECT_ID('dbo.AffiliatePartners', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.AffiliatePartners
    (
        PartnerID INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_AffiliatePartners PRIMARY KEY,
        PartnerName NVARCHAR(150) NOT NULL,
        WebsiteUrl NVARCHAR(500) NULL,
        Status NVARCHAR(50) NOT NULL CONSTRAINT DF_AffiliatePartners_Status DEFAULT N'Hoạt động',
        CreatedAt DATETIME NOT NULL CONSTRAINT DF_AffiliatePartners_CreatedAt DEFAULT GETDATE()
    );
END;

IF OBJECT_ID('dbo.ClickTracking', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.ClickTracking
    (
        ClickID INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_ClickTracking PRIMARY KEY,
        MemberID INT NULL,
        PostID INT NULL,
        PartnerID INT NULL,
        ClickedAt DATETIME NOT NULL CONSTRAINT DF_ClickTracking_ClickedAt DEFAULT GETDATE(),
        CONSTRAINT FK_ClickTracking_Members FOREIGN KEY (MemberID) REFERENCES dbo.Members(MemberID),
        CONSTRAINT FK_ClickTracking_Posts FOREIGN KEY (PostID) REFERENCES dbo.Posts(PostID),
        CONSTRAINT FK_ClickTracking_AffiliatePartners FOREIGN KEY (PartnerID) REFERENCES dbo.AffiliatePartners(PartnerID)
    );
END;

IF OBJECT_ID('dbo.Comments', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Comments
    (
        CommentID INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Comments PRIMARY KEY,
        MemberID INT NULL,
        PostID INT NULL,
        Content NVARCHAR(MAX) NOT NULL,
        CreatedAt DATETIME NOT NULL CONSTRAINT DF_Comments_CreatedAt DEFAULT GETDATE(),
        CONSTRAINT FK_Comments_Members FOREIGN KEY (MemberID) REFERENCES dbo.Members(MemberID),
        CONSTRAINT FK_Comments_Posts FOREIGN KEY (PostID) REFERENCES dbo.Posts(PostID)
    );
END;

IF OBJECT_ID('dbo.Ratings', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Ratings
    (
        RatingID INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Ratings PRIMARY KEY,
        MemberID INT NULL,
        PostID INT NULL,
        Score INT NOT NULL,
        CreatedAt DATETIME NOT NULL CONSTRAINT DF_Ratings_CreatedAt DEFAULT GETDATE(),
        CONSTRAINT FK_Ratings_Members FOREIGN KEY (MemberID) REFERENCES dbo.Members(MemberID),
        CONSTRAINT FK_Ratings_Posts FOREIGN KEY (PostID) REFERENCES dbo.Posts(PostID)
    );
END;");
        }

        private static void SeedData(string connectionString)
        {
            ExecuteNonQuery(connectionString, @"
IF NOT EXISTS (SELECT 1 FROM dbo.Roles)
BEGIN
    INSERT INTO dbo.Roles (RoleName)
    VALUES
        (N'Super Admin'),
        (N'Content Manager'),
        (N'Affiliate Manager'),
        (N'User Account Manager');
END;

IF NOT EXISTS (SELECT 1 FROM dbo.Categories)
BEGIN
    INSERT INTO dbo.Categories (CategoryName)
    VALUES
        (N'Laptop'),
        (N'Điện thoại'),
        (N'Phụ kiện'),
        (N'Đánh giá sản phẩm');
END;

IF NOT EXISTS (SELECT 1 FROM dbo.AffiliatePartners)
BEGIN
    INSERT INTO dbo.AffiliatePartners (PartnerName, WebsiteUrl)
    VALUES
        (N'Shopee', N'https://shopee.vn'),
        (N'Lazada', N'https://www.lazada.vn'),
        (N'Tiki', N'https://tiki.vn');
END;");

            EnsureAdmin(connectionString, "Super Admin", "superadmin@ftech.vn", "Super Admin");
            EnsureAdmin(connectionString, "Content Manager", "content.baopt@ftech.vn", "Content Manager");
            EnsureAdmin(connectionString, "Affiliate Manager", "affiliate@ftech.vn", "Affiliate Manager");
            EnsureAdmin(connectionString, "User Account Manager", "user.account@ftech.vn", "User Account Manager");
            EnsureMember(connectionString);
            EnsureSamplePost(connectionString);
        }

        private static void EnsureAdmin(string connectionString, string fullName, string email, string roleName)
        {
            ExecuteNonQuery(connectionString, @"
IF NOT EXISTS (SELECT 1 FROM dbo.Admins WHERE Email = @Email)
BEGIN
    INSERT INTO dbo.Admins (FullName, Email, PasswordHash, RoleID, Status)
    SELECT @FullName, @Email, @PasswordHash, RoleID, N'Hoạt động'
    FROM dbo.Roles
    WHERE RoleName = @RoleName;
END;",
                new SqlParameter("@FullName", fullName),
                new SqlParameter("@Email", email),
                new SqlParameter("@PasswordHash", PasswordHasher.Hash("12345678")),
                new SqlParameter("@RoleName", roleName));
        }

        private static void EnsureMember(string connectionString)
        {
            ExecuteNonQuery(connectionString, @"
IF NOT EXISTS (SELECT 1 FROM dbo.Members WHERE Email = @Email)
BEGIN
    INSERT INTO dbo.Members (FullName, Email, PasswordHash, AvatarURL, Status)
    VALUES (@FullName, @Email, @PasswordHash, @AvatarURL, N'Hoạt động');
END;",
                new SqlParameter("@FullName", "Người dùng mẫu"),
                new SqlParameter("@Email", "user@example.com"),
                new SqlParameter("@PasswordHash", PasswordHasher.Hash("12345678")),
                new SqlParameter("@AvatarURL", "default-avatar.png"));
        }

        private static void EnsureSamplePost(string connectionString)
        {
            ExecuteNonQuery(connectionString, @"
IF NOT EXISTS (SELECT 1 FROM dbo.Posts)
BEGIN
    DECLARE @CategoryID INT = (SELECT TOP 1 CategoryID FROM dbo.Categories ORDER BY CategoryID);
    DECLARE @AdminID INT = (SELECT TOP 1 AdminID FROM dbo.Admins WHERE Email = N'content.baopt@ftech.vn');

    INSERT INTO dbo.Posts (Title, Slug, Content, ThumbnailURL, CategoryID, CreatedBy, Status, ViewCount, CreatedAt)
    VALUES
        (N'Đánh giá laptop AI mỏng nhẹ cho công việc', N'danh-gia-laptop-ai-mong-nhe', N'Nội dung đánh giá sản phẩm mẫu.', N'https://images.unsplash.com/photo-1496181133206-80ce9b88a853?auto=format&fit=crop&w=900&q=80', @CategoryID, @AdminID, N'Đã xuất bản', 1280, DATEADD(DAY, -5, GETDATE())),
        (N'Top phụ kiện cần có cho góc làm việc', N'top-phu-kien-goc-lam-viec', N'Nội dung bài viết mẫu.', N'https://images.unsplash.com/photo-1516321318423-f06f85e504b3?auto=format&fit=crop&w=900&q=80', @CategoryID, @AdminID, N'Nháp', 0, DATEADD(DAY, -2, GETDATE())),
        (N'So sánh điện thoại tầm trung đáng mua', N'so-sanh-dien-thoai-tam-trung', N'Nội dung bài viết chờ duyệt mẫu.', N'https://images.unsplash.com/photo-1511707171634-5f897ff02aa9?auto=format&fit=crop&w=900&q=80', @CategoryID, @AdminID, N'Chờ duyệt', 0, DATEADD(DAY, -1, GETDATE()));
END;");
        }

        private static void ExecuteNonQuery(string connectionString, string commandText, params SqlParameter[] parameters)
        {
            using (var connection = new SqlConnection(connectionString))
            using (var command = connection.CreateCommand())
            {
                command.CommandText = commandText;

                foreach (var parameter in parameters ?? new SqlParameter[0])
                {
                    command.Parameters.Add(parameter);
                }

                connection.Open();
                command.ExecuteNonQuery();
            }
        }
    }
}
