using System;
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

IF OBJECT_ID('dbo.AffiliateLinks', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.AffiliateLinks
    (
        LinkID UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_AffiliateLinks PRIMARY KEY,
        LinkName NVARCHAR(255) NOT NULL,
        TargetURL NVARCHAR(500) NOT NULL,
        PartnerID INT NOT NULL,
        PostID INT NOT NULL,
        Status NVARCHAR(50) NOT NULL,
        CreatedAt DATETIME NOT NULL CONSTRAINT DF_AffiliateLinks_CreatedAt DEFAULT GETDATE(),
        CONSTRAINT FK_AffiliateLinks_Partners FOREIGN KEY (PartnerID) REFERENCES dbo.AffiliatePartners(PartnerID),
        CONSTRAINT FK_AffiliateLinks_Posts FOREIGN KEY (PostID) REFERENCES dbo.Posts(PostID)
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

IF OBJECT_ID('dbo.NewsletterSubscribers', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.NewsletterSubscribers
    (
        SubscriberID INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_NewsletterSubscribers PRIMARY KEY,
        Email NVARCHAR(255) NOT NULL CONSTRAINT UQ_NewsletterSubscribers_Email UNIQUE,
        Status NVARCHAR(50) NOT NULL CONSTRAINT DF_NewsletterSubscribers_Status DEFAULT N'Active',
        CreatedAt DATETIME NOT NULL CONSTRAINT DF_NewsletterSubscribers_CreatedAt DEFAULT GETDATE()
    );
END;

IF OBJECT_ID('dbo.Ratings', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Ratings
    (
        RatingID INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Ratings PRIMARY KEY,
        MemberID INT NULL,
        PostID INT NULL,
        RatingStar INT CHECK (RatingStar BETWEEN 1 AND 5),
        CreatedAt DATETIME NOT NULL CONSTRAINT DF_Ratings_CreatedAt DEFAULT GETDATE(),
        CONSTRAINT FK_Ratings_Members FOREIGN KEY (MemberID) REFERENCES dbo.Members(MemberID),
        CONSTRAINT FK_Ratings_Posts FOREIGN KEY (PostID) REFERENCES dbo.Posts(PostID)
    );
END;");

            // 1. Upgrade legacy Ratings schema if it was created with the old Score column.
            ExecuteNonQuery(connectionString, @"
IF OBJECT_ID('dbo.Ratings', 'U') IS NOT NULL
BEGIN
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Ratings') AND name = 'RatingStar')
    BEGIN
        ALTER TABLE dbo.Ratings ADD RatingStar INT NULL;
    END;

    IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Ratings') AND name = 'Score')
    BEGIN
        EXEC(N'UPDATE dbo.Ratings SET RatingStar = Score WHERE RatingStar IS NULL AND Score IS NOT NULL;');
    END;
END;
");

            // 2. Upgrade ClickTracking schema used by affiliate redirect tracking.
            ExecuteNonQuery(connectionString, @"
IF OBJECT_ID('dbo.AffiliateLinks', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.AffiliateLinks
    (
        LinkID UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_AffiliateLinks PRIMARY KEY,
        LinkName NVARCHAR(255) NOT NULL,
        TargetURL NVARCHAR(500) NOT NULL,
        PartnerID INT NOT NULL,
        PostID INT NOT NULL,
        Status NVARCHAR(50) NOT NULL,
        CreatedAt DATETIME NOT NULL CONSTRAINT DF_AffiliateLinks_CreatedAt DEFAULT GETDATE(),
        CONSTRAINT FK_AffiliateLinks_Partners FOREIGN KEY (PartnerID) REFERENCES dbo.AffiliatePartners(PartnerID),
        CONSTRAINT FK_AffiliateLinks_Posts FOREIGN KEY (PostID) REFERENCES dbo.Posts(PostID)
    );
END;

IF OBJECT_ID('dbo.ClickTracking', 'U') IS NOT NULL
BEGIN
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.ClickTracking') AND name = 'LinkID')
    BEGIN
        ALTER TABLE dbo.ClickTracking ADD LinkID UNIQUEIDENTIFIER NULL;
    END;
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.ClickTracking') AND name = 'ClickTime')
    BEGIN
        ALTER TABLE dbo.ClickTracking ADD ClickTime DATETIME NULL;
    END;
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.ClickTracking') AND name = 'IPAddress')
    BEGIN
        ALTER TABLE dbo.ClickTracking ADD IPAddress NVARCHAR(45) NULL;
    END;
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.ClickTracking') AND name = 'ReferrerURL')
    BEGIN
        ALTER TABLE dbo.ClickTracking ADD ReferrerURL NVARCHAR(500) NULL;
    END;
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.ClickTracking') AND name = 'IsSuspicious')
    BEGIN
        ALTER TABLE dbo.ClickTracking ADD IsSuspicious BIT NULL;
    END;

    IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.ClickTracking') AND name = 'ClickedAt')
    BEGIN
        EXEC(N'UPDATE dbo.ClickTracking SET ClickTime = ClickedAt WHERE ClickTime IS NULL AND ClickedAt IS NOT NULL;');
    END;
END;
");

            // 3. Alter Posts table to add columns for detailed review info if they don't exist
            ExecuteNonQuery(connectionString, @"
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Posts') AND name = 'Pros')
BEGIN
    ALTER TABLE dbo.Posts ADD Pros NVARCHAR(MAX) NULL;
END;
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Posts') AND name = 'Cons')
BEGIN
    ALTER TABLE dbo.Posts ADD Cons NVARCHAR(MAX) NULL;
END;
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Posts') AND name = 'QuickSummary')
BEGIN
    ALTER TABLE dbo.Posts ADD QuickSummary NVARCHAR(MAX) NULL;
END;
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Posts') AND name = 'SpecCpu')
BEGIN
    ALTER TABLE dbo.Posts ADD SpecCpu NVARCHAR(255) NULL;
END;
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Posts') AND name = 'SpecRam')
BEGIN
    ALTER TABLE dbo.Posts ADD SpecRam NVARCHAR(255) NULL;
END;
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Posts') AND name = 'SpecStorage')
BEGIN
    ALTER TABLE dbo.Posts ADD SpecStorage NVARCHAR(255) NULL;
END;
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Posts') AND name = 'SpecScreen')
BEGIN
    ALTER TABLE dbo.Posts ADD SpecScreen NVARCHAR(255) NULL;
END;
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Posts') AND name = 'SpecPin')
BEGIN
    ALTER TABLE dbo.Posts ADD SpecPin NVARCHAR(255) NULL;
END;
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Posts') AND name = 'SpecBattery')
BEGIN
    EXEC(N'UPDATE dbo.Posts SET SpecPin = SpecBattery WHERE SpecPin IS NULL AND SpecBattery IS NOT NULL;');
END;
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Posts') AND name = 'SpecWeight')
BEGIN
    ALTER TABLE dbo.Posts ADD SpecWeight NVARCHAR(255) NULL;
END;
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Posts') AND name = 'SpecPorts')
BEGIN
    ALTER TABLE dbo.Posts ADD SpecPorts NVARCHAR(255) NULL;
END;
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Posts') AND name = 'SpecTarget')
BEGIN
    ALTER TABLE dbo.Posts ADD SpecTarget NVARCHAR(255) NULL;
END;
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Posts') AND name = 'ScoreDesign')
BEGIN
    ALTER TABLE dbo.Posts ADD ScoreDesign FLOAT NULL;
END;
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Posts') AND name = 'ScorePerformance')
BEGIN
    ALTER TABLE dbo.Posts ADD ScorePerformance FLOAT NULL;
END;
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Posts') AND name = 'ScoreBattery')
BEGIN
    ALTER TABLE dbo.Posts ADD ScoreBattery FLOAT NULL;
END;
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Posts') AND name = 'ScoreScreen')
BEGIN
    ALTER TABLE dbo.Posts ADD ScoreScreen FLOAT NULL;
END;
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Posts') AND name = 'ScoreValue')
BEGIN
    ALTER TABLE dbo.Posts ADD ScoreValue FLOAT NULL;
END;
");

            // 4. Create the ProductPrices junction table if not exists
            ExecuteNonQuery(connectionString, @"
IF OBJECT_ID('dbo.ProductPrices', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.ProductPrices
    (
        PostID INT NOT NULL,
        PartnerID INT NOT NULL,
        Price DECIMAL(18, 0) NOT NULL,
        AffiliateUrl NVARCHAR(500) NOT NULL,
        CONSTRAINT PK_ProductPrices PRIMARY KEY (PostID, PartnerID),
        CONSTRAINT FK_ProductPrices_Posts FOREIGN KEY (PostID) REFERENCES dbo.Posts(PostID) ON DELETE CASCADE,
        CONSTRAINT FK_ProductPrices_Partners FOREIGN KEY (PartnerID) REFERENCES dbo.AffiliatePartners(PartnerID) ON DELETE CASCADE
    );
END;");

            // 4.5. Upgrade Comments table to add EditCount column if not exists
            ExecuteNonQuery(connectionString, @"
IF OBJECT_ID('dbo.Comments', 'U') IS NOT NULL
BEGIN
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Comments') AND name = 'EditCount')
    BEGIN
        ALTER TABLE dbo.Comments ADD EditCount INT NOT NULL CONSTRAINT DF_Comments_EditCount DEFAULT 0;
    END;
END;");

            // 5. Seed sample data (Roles, Admin, Categories, Partners, detailed MacBook Post and prices)
            ExecuteNonQuery(connectionString, @"
-- Seed default Roles
DECLARE @RoleId INT;
IF NOT EXISTS (SELECT * FROM dbo.Roles WHERE RoleName = N'SuperAdmin')
BEGIN
    INSERT INTO dbo.Roles (RoleName) VALUES (N'SuperAdmin');
END;
SELECT @RoleId = RoleID FROM dbo.Roles WHERE RoleName = N'SuperAdmin';

-- Seed default Admin
DECLARE @AdminId INT;
IF NOT EXISTS (SELECT * FROM dbo.Admins WHERE Email = N'admin@ftech.vn')
BEGIN
    INSERT INTO dbo.Admins (FullName, Email, PasswordHash, RoleID, Status, CreatedAt)
    VALUES (N'Trọng Nghĩa', N'admin@ftech.vn', N'admin123', @RoleId, N'Hoạt động', GETDATE());
END;
SELECT @AdminId = AdminID FROM dbo.Admins WHERE Email = N'admin@ftech.vn';

-- Seed default Categories
DECLARE @CategoryId INT;
IF NOT EXISTS (SELECT * FROM dbo.Categories WHERE CategoryName = N'Laptop')
BEGIN
    INSERT INTO dbo.Categories (CategoryName) VALUES (N'Laptop');
END;
SELECT @CategoryId = CategoryID FROM dbo.Categories WHERE CategoryName = N'Laptop';

IF NOT EXISTS (SELECT * FROM dbo.Categories WHERE CategoryName = N'Điện thoại') INSERT INTO dbo.Categories (CategoryName) VALUES (N'Điện thoại');
IF NOT EXISTS (SELECT * FROM dbo.Categories WHERE CategoryName = N'Phụ kiện') INSERT INTO dbo.Categories (CategoryName) VALUES (N'Phụ kiện');
IF NOT EXISTS (SELECT * FROM dbo.Categories WHERE CategoryName = N'Màn hình') INSERT INTO dbo.Categories (CategoryName) VALUES (N'Màn hình');
IF NOT EXISTS (SELECT * FROM dbo.Categories WHERE CategoryName = N'Gaming') INSERT INTO dbo.Categories (CategoryName) VALUES (N'Gaming');
IF NOT EXISTS (SELECT * FROM dbo.Categories WHERE CategoryName = N'Đồng hồ') INSERT INTO dbo.Categories (CategoryName) VALUES (N'Đồng hồ');

-- Seed AffiliatePartners
IF NOT EXISTS (SELECT * FROM dbo.AffiliatePartners WHERE PartnerName = N'CellphoneS') 
    INSERT INTO dbo.AffiliatePartners (PartnerName, WebsiteUrl, Status) VALUES (N'CellphoneS', N'https://cellphones.com.vn', N'Hoạt động');
IF NOT EXISTS (SELECT * FROM dbo.AffiliatePartners WHERE PartnerName = N'Thế Giới Di Động') 
    INSERT INTO dbo.AffiliatePartners (PartnerName, WebsiteUrl, Status) VALUES (N'Thế Giới Di Động', N'https://thegioididong.com', N'Hoạt động');
IF NOT EXISTS (SELECT * FROM dbo.AffiliatePartners WHERE PartnerName = N'Shopee Mall - Apple Flagship') 
    INSERT INTO dbo.AffiliatePartners (PartnerName, WebsiteUrl, Status) VALUES (N'Shopee Mall - Apple Flagship', N'https://shopee.vn', N'Hoạt động');

-- Seed or Update detail post with slug 'phu-kien-gaming-ban-chay'
DECLARE @PostId INT;
IF EXISTS (SELECT * FROM dbo.Posts WHERE Slug = N'phu-kien-gaming-ban-chay')
BEGIN
    UPDATE dbo.Posts
    SET Title = N'Đánh giá MacBook Air M3 sau 3 tuần sử dụng: đẹp, êm, pin lâu nhưng không dành cho mọi nhu cầu',
        Content = N'1. Thiết kế và cảm giác sử dụng
Macbook Air M3 vẫn giữ nguyên thiết kế mỏng nhẹ đặc trưng vô cùng quyến rũ, nhưng điểm nâng cấp đáng giá nhất chính là việc phủ một lớp Anodized giúp chống bám vân tay tốt hơn nhiều so với thế hệ trước.

Vỏ ngoài được chế tác từ nhôm nguyên khối vô cùng chắc chắn và sang trọng, máy cực kỳ mỏng nhẹ chỉ khoảng 1.24 kg và rất dễ dàng để bỏ vào balo mang đi làm việc hàng ngày.
[note]Nhờ lớp sơn Anodized mới, người dùng sẽ ít gặp tình trạng vân tay nham nhở như các phiên bản tiền nhiệm màu Midnight trước đây.[/note]

2. Hiệu năng thực tế
Nhờ trang bị vi xử lý Apple M3 thế hệ mới nhất, thiết bị cho khả năng xử lý mượt mà tất cả các tác vụ văn phòng từ Word, Excel đến các ứng dụng đồ họa 2D nhẹ nhàng như Photoshop, Illustrator, Premiere ở mức độ bán chuyên nghiệp.

Đặc biệt, hệ thống bộ nhớ được tối ưu hóa cho phép bạn mở hàng chục tab Safari mà không gặp bất kỳ hiện tượng giật lag hay tràn RAM nào.

3. Pin, nhiệt độ và độ ồn
Thời lượng pin thực sự là điểm cộng lớn nhất trên dòng MacBook Air M3 mới này. Trong điều kiện làm việc thực tế với kết nối Wi-Fi liên tục và độ sáng màn hình ở mức 70%, thiết bị có thể duy trì hoạt động bền bỉ lên đến 15 giờ liên tục mà không cần đến củ sạc.

Máy hoạt động hoàn toàn yên tĩnh nhờ thiết kế không quạt tản nhiệt, nhưng vẫn giữ được mức nhiệt độ cực kỳ mát mẻ, dễ chịu cho người dùng khi sử dụng trực tiếp trên đùi.

4. Có nên mua hay không?
Nếu bạn là học sinh, sinh viên, lập trình viên di động hay dân văn phòng đang tìm kiếm một chiếc máy mỏng nhẹ, pin cực lâu, thiết kế sang trọng và bền bỉ dùng tốt trong vòng 3 - 5 năm tới thì MacBook Air M3 chắc chắn là một sự lựa chọn không thể bỏ qua.',
        ThumbnailURL = N'https://images.unsplash.com/photo-1517336714731-489689fd1ca8?auto=format&fit=crop&w=900&q=80',
        CategoryID = @CategoryId,
        CreatedBy = @AdminId,
        Status = N'Đã xuất bản',
        QuickSummary = N'Phù hợp với học sinh, sinh viên, dân văn phòng, content creator và chuyên viên di động;Điểm mạnh nhất là thiết kế mỏng nhẹ, pin trâu, ngoại hình đẹp và độ hoàn thiện cao;Điểm hạn chế là màn hình tần số quét 60Hz và bị giới hạn số cổng kết nối.',
        Pros = N'Thiết kế mỏng, nhẹ, hoàn thiện cực kỳ tốt và không sợ lỗi thời;Thời lượng pin ấn tượng cho công việc học tập, văn phòng suốt ngày dài;Máy chạy êm ái, hoàn toàn không tiếng ồn nhờ tản nhiệt thụ động.',
        Cons = N'Tần số quét màn hình chỉ dừng lại ở 60Hz;Số lượng cổng kết nối khá hạn chế (chỉ 2 cổng Thunderbolt);Mức giá bán còn tương đối cao so với cấu hình phần cứng.',
        SpecCpu = N'Apple M3 8-core CPU',
        SpecRam = N'8GB unified memory',
        SpecStorage = N'SSD 256GB',
        SpecScreen = N'15.3-inch Liquid Retina Display',
        SpecPin = N'Lên đến 18 giờ phát video',
        SpecWeight = N'Khoảng 1.24 kg',
        SpecPorts = N'MagSafe 3, 2x Thunderbolt 3, Jack 3.5mm',
        SpecTarget = N'Học tập, văn phòng, sáng tạo nội dung nhẹ đến trung bình',
        ScoreDesign = 9.0,
        ScorePerformance = 8.5,
        ScoreBattery = 9.4,
        ScoreScreen = 8.8,
        ScoreValue = 8.1
    WHERE Slug = N'phu-kien-gaming-ban-chay';

    SELECT @PostId = PostID FROM dbo.Posts WHERE Slug = N'phu-kien-gaming-ban-chay';
END
ELSE
BEGIN
    INSERT INTO dbo.Posts (Title, Slug, Content, ThumbnailURL, CategoryID, CreatedBy, Status, ViewCount, CreatedAt, QuickSummary, Pros, Cons, SpecCpu, SpecRam, SpecStorage, SpecScreen, SpecPin, SpecWeight, SpecPorts, SpecTarget, ScoreDesign, ScorePerformance, ScoreBattery, ScoreScreen, ScoreValue)
    VALUES (N'Đánh giá MacBook Air M3 sau 3 tuần sử dụng: đẹp, êm, pin lâu nhưng không dành cho mọi nhu cầu', N'phu-kien-gaming-ban-chay', 
        N'1. Thiết kế và cảm giác sử dụng
Macbook Air M3 vẫn giữ nguyên thiết kế mỏng nhẹ đặc trưng vô cùng quyến rũ, nhưng điểm nâng cấp đáng giá nhất chính là việc phủ một lớp Anodized giúp chống bám vân tay tốt hơn nhiều so với thế hệ trước.

Vỏ ngoài được chế tác từ nhôm nguyên khối vô cùng chắc chắn và sang trọng, máy cực kỳ mỏng nhẹ chỉ khoảng 1.24 kg và rất dễ dàng để bỏ vào balo mang đi làm việc hàng ngày.
[note]Nhờ lớp sơn Anodized mới, người dùng sẽ ít gặp tình trạng vân tay nham nhở như các phiên bản tiền nhiệm màu Midnight trước đây.[/note]

2. Hiệu năng thực tế
Nhờ trang bị vi xử lý Apple M3 thế hệ mới nhất, thiết bị cho khả năng xử lý mượt mà tất cả các tác vụ văn phòng từ Word, Excel đến các ứng dụng đồ họa 2D nhẹ nhàng như Photoshop, Illustrator, Premiere ở mức độ bán chuyên nghiệp.

Đặc biệt, hệ thống bộ nhớ được tối ưu hóa cho phép bạn mở hàng chục tab Safari mà không gặp bất kỳ hiện tượng giật lag hay tràn RAM nào.

3. Pin, nhiệt độ và độ ồn
Thời lượng pin thực sự là điểm cộng lớn nhất trên dòng MacBook Air M3 mới này. Trong điều kiện làm việc thực tế với kết nối Wi-Fi liên tục và độ sáng màn hình ở mức 70%, thiết bị có thể duy trì hoạt động bền bỉ lên đến 15 giờ liên tục mà không cần đến củ sạc.

Máy hoạt động hoàn toàn yên tĩnh nhờ thiết kế không quạt tản nhiệt, nhưng vẫn giữ được mức nhiệt độ cực kỳ mát mẻ, dễ chịu cho người dùng khi sử dụng trực tiếp trên đùi.

4. Có nên mua hay không?
Nếu bạn là học sinh, sinh viên, lập trình viên di động hay dân văn phòng đang tìm kiếm một chiếc máy mỏng nhẹ, pin cực lâu, thiết kế sang trọng và bền bỉ dùng tốt trong vòng 3 - 5 năm tới thì MacBook Air M3 chắc chắn là một sự lựa chọn không thể bỏ qua.', 
        N'https://images.unsplash.com/photo-1517336714731-489689fd1ca8?auto=format&fit=crop&w=900&q=80', @CategoryId, @AdminId, N'Đã xuất bản', 1240, GETDATE(),
        N'Phù hợp với học sinh, sinh viên, dân văn phòng, content creator và chuyên viên di động;Điểm mạnh nhất là thiết kế mỏng nhẹ, pin trâu, ngoại hình đẹp và độ hoàn thiện cao;Điểm hạn chế là màn hình tần số quét 60Hz và bị giới hạn số cổng kết nối.',
        N'Thiết kế mỏng, nhẹ, hoàn thiện cực kỳ tốt và không sợ lỗi thời;Thời lượng pin ấn tượng cho công việc học tập, văn phòng suốt ngày dài;Máy chạy êm ái, hoàn toàn không tiếng ồn nhờ tản nhiệt thụ động.',
        N'Tần số quét màn hình chỉ dừng lại ở 60Hz;Số lượng cổng kết nối khá hạn chế (chỉ 2 cổng Thunderbolt);Mức giá bán còn tương đối cao so với cấu hình phần cứng.',
        N'Apple M3 8-core CPU', N'8GB unified memory', N'SSD 256GB', N'15.3-inch Liquid Retina Display', N'Lên đến 18 giờ phát video', N'Khoảng 1.24 kg', N'MagSafe 3, 2x Thunderbolt 3, Jack 3.5mm', N'Học tập, văn phòng, sáng tạo nội dung nhẹ đến trung bình',
        9.0, 8.5, 9.4, 8.8, 8.1);

    SELECT @PostId = SCOPE_IDENTITY();
END;

-- Seed AffiliatePrices for this post
DECLARE @Partner1Id INT, @Partner2Id INT, @Partner3Id INT;
SELECT @Partner1Id = PartnerID FROM dbo.AffiliatePartners WHERE PartnerName = N'CellphoneS';
SELECT @Partner2Id = PartnerID FROM dbo.AffiliatePartners WHERE PartnerName = N'Thế Giới Di Động';
SELECT @Partner3Id = PartnerID FROM dbo.AffiliatePartners WHERE PartnerName = N'Shopee Mall - Apple Flagship';

IF @PostId IS NOT NULL
BEGIN
    DELETE FROM dbo.ProductPrices WHERE PostID = @PostId;

    IF @Partner1Id IS NOT NULL
        INSERT INTO dbo.ProductPrices (PostID, PartnerID, Price, AffiliateUrl)
        VALUES (@PostId, @Partner1Id, 28010000, N'https://cellphones.com.vn');

    IF @Partner2Id IS NOT NULL
        INSERT INTO dbo.ProductPrices (PostID, PartnerID, Price, AffiliateUrl)
        VALUES (@PostId, @Partner2Id, 28400000, N'https://thegioididong.com');

    IF @Partner3Id IS NOT NULL
        INSERT INTO dbo.ProductPrices (PostID, PartnerID, Price, AffiliateUrl)
        VALUES (@PostId, @Partner3Id, 28180000, N'https://shopee.vn');
END;
");
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
