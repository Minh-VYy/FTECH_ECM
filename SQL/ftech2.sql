USE master;
GO

IF EXISTS (SELECT name FROM sys.databases WHERE name = N'FTechAffiliateDB')
BEGIN
    ALTER DATABASE FTechAffiliateDB SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE FTechAffiliateDB;
END
GO

CREATE DATABASE FTechAffiliateDB;
GO

USE FTechAffiliateDB;
GO

-- ====================================================================
-- I. KHỞI TẠO CẤU TRÚC BẢNG
-- ====================================================================

CREATE TABLE Roles (
    RoleID INT IDENTITY(1,1) PRIMARY KEY,
    RoleName NVARCHAR(50) NOT NULL UNIQUE,
    Description NVARCHAR(255)
);

CREATE TABLE Admins (
    AdminID INT IDENTITY(1,1) PRIMARY KEY,
    FullName NVARCHAR(100) NOT NULL,
    Email VARCHAR(100) NOT NULL UNIQUE,
    PasswordHash VARCHAR(255) NOT NULL,
    RoleID INT NOT NULL,
    Status NVARCHAR(20) DEFAULT N'Hoạt động' CHECK (Status IN (N'Hoạt động', N'Đã khóa')),
    CreatedAt DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (RoleID) REFERENCES Roles(RoleID)
);

CREATE TABLE Members (
    MemberID INT IDENTITY(1,1) PRIMARY KEY,
    FullName NVARCHAR(100) NOT NULL,
    Email VARCHAR(100) NOT NULL UNIQUE,
    PasswordHash VARCHAR(255) NOT NULL,
    AvatarURL VARCHAR(255) DEFAULT 'default-avatar.png',
    Status NVARCHAR(20) DEFAULT N'Chưa xác nhận' CHECK (Status IN (N'Chưa xác nhận', N'Hoạt động', N'Đã khóa')),
    CreatedAt DATETIME DEFAULT GETDATE()
);

CREATE TABLE AffiliatePartners (
    PartnerID INT IDENTITY(1,1) PRIMARY KEY,
    PartnerName NVARCHAR(100) NOT NULL,
    WebsiteURL VARCHAR(255) NOT NULL,
    ContactInfo NVARCHAR(255),
    CurrentCommissionRate DECIMAL(5,2) DEFAULT 0.00 CHECK (CurrentCommissionRate BETWEEN 0 AND 100),
    Status NVARCHAR(20) DEFAULT N'Chờ duyệt' CHECK (Status IN (N'Chờ duyệt', N'Hoạt động', N'Từ chối', N'Tạm ngưng')),
    CreatedAt DATETIME DEFAULT GETDATE()
);

CREATE TABLE CommissionHistory (
    HistoryID INT IDENTITY(1,1) PRIMARY KEY,
    PartnerID INT NOT NULL,
    OldRate DECIMAL(5,2) NOT NULL CHECK (OldRate BETWEEN 0 AND 100),
    NewRate DECIMAL(5,2) NOT NULL CHECK (NewRate BETWEEN 0 AND 100),
    ChangedBy INT NOT NULL,
    EffectiveDate DATETIME NOT NULL,
    ChangedAt DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (PartnerID) REFERENCES AffiliatePartners(PartnerID),
    FOREIGN KEY (ChangedBy) REFERENCES Admins(AdminID)
);

CREATE TABLE Categories (
    CategoryID INT IDENTITY(1,1) PRIMARY KEY,
    CategoryName NVARCHAR(100) NOT NULL UNIQUE,
    Description NVARCHAR(255)
);

CREATE TABLE Posts (
    PostID INT IDENTITY(1,1) PRIMARY KEY,
    Title NVARCHAR(255) NOT NULL,
    Slug VARCHAR(255) NOT NULL UNIQUE,
    Content NVARCHAR(MAX) NOT NULL,
    ThumbnailURL VARCHAR(255),
    CategoryID INT NOT NULL,
    CreatedBy INT NOT NULL,
    Status NVARCHAR(20) DEFAULT N'Nháp' CHECK (Status IN (N'Nháp', N'Chờ duyệt', N'Đã xuất bản', N'Từ chối')),
    RejectionReason NVARCHAR(255),
    ViewCount INT DEFAULT 0 CHECK (ViewCount >= 0),
    CreatedAt DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (CategoryID) REFERENCES Categories(CategoryID),
    FOREIGN KEY (CreatedBy) REFERENCES Admins(AdminID)
);

CREATE TABLE AffiliateLinks (
    LinkID UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    LinkName NVARCHAR(150) NOT NULL,
    TargetURL VARCHAR(MAX) NOT NULL,
    PartnerID INT NOT NULL,
    PostID INT,
    Status NVARCHAR(20) DEFAULT N'Hoạt động' CHECK (Status IN (N'Hoạt động', N'Vô hiệu hóa')),
    CreatedAt DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (PartnerID) REFERENCES AffiliatePartners(PartnerID),
    FOREIGN KEY (PostID) REFERENCES Posts(PostID) ON DELETE SET NULL
);

CREATE TABLE ClickTracking (
    ClickID BIGINT IDENTITY(1,1) PRIMARY KEY,
    LinkID UNIQUEIDENTIFIER NOT NULL,
    PartnerID INT NOT NULL,
    PostID INT NOT NULL,
    MemberID INT NULL,
    ClickTime DATETIME DEFAULT GETDATE(),
    IPAddress VARCHAR(45) NOT NULL,
    ReferrerURL VARCHAR(MAX),
    IsSuspicious BIT DEFAULT 0,
    FOREIGN KEY (LinkID) REFERENCES AffiliateLinks(LinkID),
    FOREIGN KEY (PartnerID) REFERENCES AffiliatePartners(PartnerID),
    FOREIGN KEY (PostID) REFERENCES Posts(PostID),
    FOREIGN KEY (MemberID) REFERENCES Members(MemberID)
);

CREATE TABLE Comments (
    CommentID INT IDENTITY(1,1) PRIMARY KEY,
    PostID INT NOT NULL,
    MemberID INT NOT NULL,
    Content NVARCHAR(MAX) NOT NULL,
    ParentCommentID INT NULL,
    CreatedAt DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (PostID) REFERENCES Posts(PostID) ON DELETE CASCADE,
    FOREIGN KEY (MemberID) REFERENCES Members(MemberID),
    FOREIGN KEY (ParentCommentID) REFERENCES Comments(CommentID) ON DELETE NO ACTION
);

CREATE TABLE Ratings (
    RatingID INT IDENTITY(1,1) PRIMARY KEY,
    PostID INT NOT NULL,
    MemberID INT NOT NULL,
    RatingStar INT CHECK (RatingStar BETWEEN 1 AND 5),
    ReviewText NVARCHAR(MAX),
    CreatedAt DATETIME DEFAULT GETDATE(),
    CONSTRAINT UC_Member_Post_Rating UNIQUE (PostID, MemberID),
    FOREIGN KEY (PostID) REFERENCES Posts(PostID) ON DELETE CASCADE,
    FOREIGN KEY (MemberID) REFERENCES Members(MemberID)
);
GO

-- ====================================================================
-- II. INDEX
-- ====================================================================
CREATE INDEX IX_ClickTracking_LinkID_ClickTime ON ClickTracking(LinkID, ClickTime);
CREATE INDEX IX_ClickTracking_IPAddress_ClickTime ON ClickTracking(IPAddress, ClickTime);
CREATE INDEX IX_Posts_Status_CreatedAt ON Posts(Status, CreatedAt);
GO

-- ====================================================================
-- III. SEED DATA
-- ====================================================================

-- 1. Roles
INSERT INTO Roles (RoleName, Description) VALUES
(N'Super Admin',          N'Quản trị viên tối cao, toàn quyền hệ thống'),
(N'Content Manager',      N'Quản lý biên tập nội dung bài viết đánh giá'),
(N'Affiliate Manager',    N'Quản lý liên kết, đối tác và hoa hồng'),
(N'User Account Manager', N'Quản lý tài khoản người dùng và xử lý vi phạm');

-- 2. Admins
INSERT INTO Admins (FullName, Email, PasswordHash, RoleID, Status) VALUES
(N'Nguyễn Tuấn Anh',      'admin.anhnt@ftech.vn',      'hash_pass_superadmin_123',  1, N'Hoạt động'),
(N'Phạm Thái Bảo',        'content.baopt@ftech.vn',    'hash_pass_content_123',     2, N'Hoạt động'),
(N'Võ Minh Hoàng',        'affiliate.hoangvm@ftech.vn','hash_pass_affiliate_123',   3, N'Hoạt động'),
(N'Trương Thị Kiều Nhi',  'user.nhittk@ftech.vn',      'hash_pass_user_123',        4, N'Hoạt động');

-- 3. Members (seed thật - MemberID 1..10)
INSERT INTO Members (FullName, Email, PasswordHash, AvatarURL, Status) VALUES
(N'Trần Văn Hùng',        'hungtv@gmail.com',   'hash_member_hung123', 'avatar1.png',        N'Hoạt động'),
(N'Lê Thị Mai',           'mailt@gmail.com',    'hash_member_mai123',  'avatar2.png',        N'Hoạt động'),
(N'Nguyễn Minh Vỹ',       'vynm@gmail.com',     'hash_member_vy123',   'default-avatar.png', N'Chưa xác nhận'),
(N'Phạm Quốc Bảo',        'baopq@gmail.com',    'hash_member_bao123',  'default-avatar.png', N'Đã khóa'),
(N'Lê Hoàng Long',        'longlh@gmail.com',   'pass_hash_1',         'user1.png',          N'Hoạt động'),
(N'Nguyễn Thị Minh Thư',  'thunm@gmail.com',    'pass_hash_2',         'user2.png',          N'Hoạt động'),
(N'Đặng Đăng Khoa',       'khoadd@gmail.com',   'pass_hash_3',         'user3.png',          N'Hoạt động'),
(N'Vũ Hoàng Giang',       'giangvh@gmail.com',  'pass_hash_4',         'default-avatar.png', N'Hoạt động'),
(N'Trần Minh Triết',      'triettm@gmail.com',  'pass_hash_5',         'default-avatar.png', N'Chưa xác nhận'),
(N'Nguyễn Văn Hải',       'hainv@gmail.com',    'pass_hash_6',         'default-avatar.png', N'Đã khóa');

-- 4. AffiliatePartners
INSERT INTO AffiliatePartners (PartnerName, WebsiteURL, ContactInfo, CurrentCommissionRate, Status) VALUES
(N'Shopee Việt Nam',    'https://shopee.vn',          N'Hotline: 19001221 - support@shopee.vn',      5.50, N'Hoạt động'),
(N'Lazada Việt Nam',    'https://lazada.vn',           N'Hotline: 19001008 - partner@lazada.vn',      6.00, N'Hoạt động'),
(N'Tiki Việt Nam',      'https://tiki.vn',             N'Liên hệ: b2b@tiki.vn',                       4.50, N'Hoạt động'),
(N'Thế Giới Di Động',   'https://thegioididong.com',   N'Phòng Kinh Doanh: contact@tgdd.com',         2.50, N'Tạm ngưng');

-- 5. CommissionHistory
INSERT INTO CommissionHistory (PartnerID, OldRate, NewRate, ChangedBy, EffectiveDate) VALUES
(1, 4.00, 5.50, 3, '2026-05-01 00:00:00'),
(2, 5.00, 6.00, 3, '2026-05-15 00:00:00');

-- 6. Categories
INSERT INTO Categories (CategoryName, Description) VALUES
(N'Điện thoại', N'Đánh giá các dòng smartphone mới nhất'),
(N'Laptop',     N'Đánh giá máy tính xách tay văn phòng, gaming'),
(N'Phụ kiện',   N'Tai nghe, sạc dự phòng, chuột, bàn phím');

-- 7. Posts
INSERT INTO Posts (Title, Slug, Content, ThumbnailURL, CategoryID, CreatedBy, Status, RejectionReason, ViewCount, CreatedAt) VALUES
(N'Đánh giá chi tiết iPhone 15 Pro Max sau 6 tháng: Khung Titan có thực sự đáng tiền?',
 'danh-gia-iphone-15-pro-max-sau-6-thang',
 N'Sau hơn nửa năm ra mắt, iPhone 15 Pro Max vẫn là chiếc flagship thu hút nhiều sự chú ý nhất. Điểm nâng cấp đáng giá nhất chính là phần vỏ bọc chất liệu Titan cấp vũ trụ giúp máy nhẹ hơn rõ rệt so với thế hệ tiền nhiệm 14 Pro Max.',
 'iphone-15-pro-max.jpg', 1, 2, N'Đã xuất bản', NULL, 3450, '2026-01-10 08:30:00'),

(N'Đánh giá MacBook Air M3 2026: Chiếc laptop văn phòng hoàn hảo nhất thời điểm hiện tại',
 'danh-gia-macbook-air-m3-2026',
 N'Apple tiếp tục khẳng định vị thế dẫn đầu phân khúc laptop mỏng nhẹ với phiên bản MacBook Air chip M3. Hiệu năng từ chip M3 mang lại tốc độ xử lý nhanh hơn 20% so với chip M2.',
 'macbook-air-m3.jpg', 2, 2, N'Đã xuất bản', NULL, 2180, '2026-02-18 14:20:00'),

(N'Đánh giá Samsung Galaxy S24 Ultra: Quyền năng AI có thực sự thực dụng hay chỉ là quảng cáo?',
 'danh-gia-samsung-galaxy-s24-ultra-ai',
 N'Galaxy S24 Ultra năm nay không thay đổi nhiều về ngoại hình ngoại trừ màn hình được làm phẳng hoàn toàn. Điểm nhấn lớn nhất nằm ở bộ tính năng Galaxy AI.',
 'galaxy-s24-ultra.jpg', 1, 2, N'Đã xuất bản', NULL, 1890, '2026-03-05 10:15:00'),

(N'Trên tay Tai nghe chống ồn Sony WH-1000XM5: Vua chống ồn phân khúc cao cấp',
 'tren-tay-tai-nghe-sony-wh-1000xm5',
 N'Sony WH-1000XM5 sở hữu ngôn ngữ thiết kế hoàn toàn lột xác so với thế hệ XM4 tiền nhiệm. Khả năng chống ồn chủ động (ANC) của Sony vẫn giữ vững ngôi vương.',
 'sony-wh-1000xm5.jpg', 3, 2, N'Đã xuất bản', NULL, 1120, '2026-04-22 11:40:00'),

(N'Đánh giá máy chơi game PlayStation 5 Slim: Bản nâng cấp mỏng nhẹ đáng giá',
 'danh-gia-playstation-5-slim',
 N'Phiên bản PS5 Slim ra mắt mang đến một kích thước nhỏ gọn hơn tới 30% so với bản tiêu chuẩn cồng kềnh trước đó.',
 'ps5-slim.jpg', 3, 2, N'Chờ duyệt', NULL, 0, '2026-05-26 15:00:00'),

(N'Đánh giá sạc dự phòng dỏm không rõ nguồn gốc',
 'danh-gia-sac-du-phong-dom',
 N'Bài viết review sản phẩm kém chất lượng...',
 'sac-du-phong.jpg', 3, 2, N'Từ chối', N'Nội dung không mang tính xây dựng, sai tôn chỉ web', 0, '2026-05-21 16:00:00'),

(N'Đánh giá iPhone 16',          'iphone-16-review',          N'iPhone 16 mang đến hiệu năng mạnh mẽ cùng camera cải tiến.',                'https://picsum.photos/seed/iphone16/800/600',       1, 2, N'Đã xuất bản', NULL, 2500, GETDATE()),
(N'Đánh giá iPhone 16 Plus',     'iphone-16-plus-review',     N'Phiên bản màn hình lớn với thời lượng pin ấn tượng.',                        'https://picsum.photos/seed/iphone16plus/800/600',   1, 2, N'Đã xuất bản', NULL, 1800, GETDATE()),
(N'Đánh giá iPhone 16 Pro',      'iphone-16-pro-review',      N'Khung titan, camera chuyên nghiệp và chip thế hệ mới.',                      'https://picsum.photos/seed/iphone16pro/800/600',    1, 2, N'Đã xuất bản', NULL, 3200, GETDATE()),
(N'Đánh giá iPhone 16 Pro Max',  'iphone-16-pro-max-review',  N'Flagship cao cấp nhất của Apple năm nay.',                                   'https://picsum.photos/seed/iphone16promax/800/600', 1, 2, N'Đã xuất bản', NULL, 4100, GETDATE()),
(N'Đánh giá Samsung Galaxy S25', 'galaxy-s25-review',         N'Hiệu năng mạnh cùng Galaxy AI mới.',                                         'https://picsum.photos/seed/galaxys25/800/600',      1, 2, N'Đã xuất bản', NULL, 2100, GETDATE()),
(N'Đánh giá Samsung Galaxy S25 Ultra', 'galaxy-s25-ultra-review', N'Camera zoom ấn tượng và thiết kế cao cấp.',                              'https://picsum.photos/seed/galaxys25ultra/800/600', 1, 2, N'Đã xuất bản', NULL, 3900, GETDATE()),
(N'Đánh giá Xiaomi 15 Ultra',    'xiaomi-15-ultra-review',    N'Camera Leica và hiệu năng hàng đầu.',                                        'https://picsum.photos/seed/xiaomi15ultra/800/600',  1, 2, N'Đã xuất bản', NULL, 2200, GETDATE()),
(N'Đánh giá OPPO Find X8 Pro',   'find-x8-pro-review',        N'Flagship với khả năng chụp ảnh vượt trội.',                                  'https://picsum.photos/seed/findx8pro/800/600',      1, 2, N'Đã xuất bản', NULL, 1700, GETDATE()),
(N'Đánh giá Vivo X200 Pro',      'vivo-x200-pro-review',      N'Camera tele cực mạnh.',                                                      'https://picsum.photos/seed/vivox200pro/800/600',    1, 2, N'Đã xuất bản', NULL, 1600, GETDATE()),
(N'Đánh giá Google Pixel 10 Pro','pixel10pro-review',         N'Android gốc cùng AI thông minh.',                                            'https://picsum.photos/seed/pixel10pro/800/600',     1, 2, N'Đã xuất bản', NULL, 2000, GETDATE()),
(N'Đánh giá MacBook Air M4',     'macbook-air-m4-review',     N'Laptop mỏng nhẹ cho dân văn phòng.',                                         'https://picsum.photos/seed/macbookairm4/800/600',   2, 2, N'Đã xuất bản', NULL, 3500, GETDATE()),
(N'Đánh giá MacBook Pro M4',     'macbook-pro-m4-review',     N'Hiệu năng xử lý cực mạnh.',                                                  'https://picsum.photos/seed/macbookprom4/800/600',   2, 2, N'Đã xuất bản', NULL, 3000, GETDATE()),
(N'Đánh giá Dell XPS 13',        'dell-xps13-review',         N'Thiết kế sang trọng và cao cấp.',                                            'https://picsum.photos/seed/dellxps13/800/600',      2, 2, N'Đã xuất bản', NULL, 1800, GETDATE()),
(N'Đánh giá Lenovo Legion 5',    'legion5-review',            N'Laptop gaming phổ biến.',                                                    'https://picsum.photos/seed/legion5/800/600',        2, 2, N'Đã xuất bản', NULL, 2100, GETDATE()),
(N'Đánh giá ASUS ROG G16',       'rog-g16-review',            N'Cấu hình mạnh cho game thủ.',                                                'https://picsum.photos/seed/rogg16/800/600',         2, 2, N'Đã xuất bản', NULL, 2800, GETDATE()),
(N'Đánh giá AirPods Pro 2',      'airpods-pro2-review',       N'Tai nghe true wireless hàng đầu.',                                           'https://picsum.photos/seed/airpodspro2/800/600',    3, 2, N'Đã xuất bản', NULL, 2300, GETDATE()),
(N'Đánh giá Sony WH-1000XM6',    'sony-xm6-review',           N'Khả năng chống ồn xuất sắc.',                                                'https://picsum.photos/seed/sonyxm6/800/600',        3, 2, N'Đã xuất bản', NULL, 2400, GETDATE()),
(N'Đánh giá Galaxy Buds 3 Pro',  'buds3pro-review',           N'Tai nghe cao cấp của Samsung.',                                              'https://picsum.photos/seed/buds3pro/800/600',       3, 2, N'Đã xuất bản', NULL, 1600, GETDATE()),
(N'Đánh giá Apple Watch Ultra 3','watch-ultra3-review',        N'Đồng hồ thông minh cao cấp.',                                                'https://picsum.photos/seed/watchultra3/800/600',    3, 2, N'Đã xuất bản', NULL, 1700, GETDATE()),
(N'Đánh giá Garmin Fenix 8',     'fenix8-review',             N'Đồng hồ dành cho thể thao chuyên nghiệp.',                                   'https://picsum.photos/seed/fenix8/800/600',         3, 2, N'Đã xuất bản', NULL, 1400, GETDATE());
GO

-- ====================================================================
-- 8. AFFILIATE LINKS (seed thật dùng GUID cố định)
-- ====================================================================
DECLARE @LinkIphoneShopee UNIQUEIDENTIFIER = '11111111-1111-1111-1111-111111111111';
DECLARE @LinkIphoneLazada UNIQUEIDENTIFIER = '22222222-2222-2222-2222-222222222222';
DECLARE @LinkMacbookTiki  UNIQUEIDENTIFIER = '33333333-3333-3333-3333-333333333333';
DECLARE @LinkSonyShopee   UNIQUEIDENTIFIER = '44444444-4444-4444-4444-444444444444';

INSERT INTO AffiliateLinks (LinkID, LinkName, TargetURL, PartnerID, PostID, Status) VALUES
(@LinkIphoneShopee, N'Mua iPhone 15 Pro Max giá rẻ nhất tại Shopee',           'https://shopee.vn/apple-iphone-15-pro-max-sale',      1, 1, N'Hoạt động'),
(@LinkIphoneLazada, N'Mua iPhone 15 Pro Max chính hãng Apple Store tại Lazada', 'https://lazada.vn/apple-store-iphone15promax',        2, 1, N'Hoạt động'),
(@LinkMacbookTiki,  N'Mua MacBook Air M3 chính hãng 100% tại Tiki Trading',     'https://tiki.vn/macbook-air-m3-chinh-hang',           3, 2, N'Hoạt động'),
(@LinkSonyShopee,   N'Săn Deal tai nghe Sony WH-1000XM5 chính hãng tại Shopee', 'https://shopee.vn/sony-wh-1000xm5-mall',              1, 4, N'Hoạt động');

-- Click thật cho seed data
INSERT INTO ClickTracking (LinkID, PartnerID, PostID, MemberID, ClickTime, IPAddress, ReferrerURL, IsSuspicious) VALUES
(@LinkIphoneShopee, 1, 1, 1,    '2026-05-20 09:15:00', '114.123.45.67',  'https://ftech.vn/danh-gia-iphone-15-pro-max-sau-6-thang', 0),
(@LinkIphoneShopee, 1, 1, 2,    '2026-05-20 10:22:14', '27.65.184.22',   'https://ftech.vn/danh-gia-iphone-15-pro-max-sau-6-thang', 0),
(@LinkIphoneLazada, 2, 1, NULL, '2026-05-21 14:05:33', '171.244.12.90',  'https://ftech.vn/danh-gia-iphone-15-pro-max-sau-6-thang', 0),
(@LinkMacbookTiki,  3, 2, 3,    '2026-05-22 16:45:10', '14.232.88.45',   'https://ftech.vn/danh-gia-macbook-air-m3-2026',           0),
(@LinkSonyShopee,   1, 4, NULL, '2026-05-23 21:10:02', '118.70.155.61',  'https://ftech.vn/tren-tay-tai-nghe-sony-wh-1000xm5',      0),
(@LinkIphoneShopee, 1, 1, NULL, '2026-05-24 08:00:01', '1.1.1.1',        'https://ftech.vn/danh-gia-iphone-15-pro-max-sau-6-thang', 1),
(@LinkIphoneShopee, 1, 1, NULL, '2026-05-24 08:00:05', '1.1.1.1',        'https://ftech.vn/danh-gia-iphone-15-pro-max-sau-6-thang', 1),
(@LinkIphoneShopee, 1, 1, NULL, '2026-05-24 08:00:12', '1.1.1.1',        'https://ftech.vn/danh-gia-iphone-15-pro-max-sau-6-thang', 1);
GO

-- ====================================================================
-- 9. COMMENTS (seed thật)
-- ====================================================================
INSERT INTO Comments (PostID, MemberID, Content, ParentCommentID, CreatedAt) VALUES
(1, 1, N'Bài viết viết rất đúng trọng tâm. Mình đang dùng bản 14 Pro Max thấy nặng tay quá, cầm em này bọc Titan nhẹ đi hẳn, đáng tiền nâng cấp thực sự.', NULL, '2026-01-10 09:00:00'),
(1, 2, N'Admin cho mình hỏi là chip A17 Pro chơi game lâu có bị tụt độ sáng màn hình nhiều không ạ?',                                                       NULL, '2026-01-10 10:15:00');

INSERT INTO Comments (PostID, MemberID, Content, ParentCommentID, CreatedAt) VALUES
(1, 3, N'Có tụt nha bạn ơi, nếu chơi phòng điều hòa thì không sao chứ chơi phòng quạt tầm 20 phút là máy nóng lên, màn hình tự giảm độ sáng xuống khoảng 70% đó.', 2, '2026-01-10 10:45:00'),
(1, 2, N'Cảm ơn bác Khoa đã tư vấn nhé, chắc mình phải sắm thêm cái sò lạnh để cày game rồi.',                                                                      3, '2026-01-10 11:00:00'),
(2, 4, N'Đúng là bản 8GB RAM giờ mở tầm 10 tab Chrome kèm Photoshop nhẹ là bắt đầu thấy báo tràn RAM rồi. Ai mua làm việc lâu dài khuyên thật lòng nên cố tiền lên hẳn bản 16GB RAM.', NULL, '2026-02-18 15:30:00');

-- ====================================================================
-- 10. RATINGS (seed thật - chỉ dùng MemberID 1..6, PostID 1/2/4)
-- ====================================================================
INSERT INTO Ratings (PostID, MemberID, RatingStar, ReviewText, CreatedAt) VALUES
(1, 1, 5, N'Thiết kế quá đẹp, cầm nhẹ tay, link mua hàng của Shopee admin gắn giao hàng siêu nhanh, hàng chính hãng nguyên seal VN/A cực kỳ an tâm.', '2026-01-10 09:05:00'),
(1, 2, 4, N'Máy dùng mượt mà hoàn hảo, trừ việc chơi game nặng hơi nóng nhanh ra thì không có điểm gì để chê cả.',                                       '2026-01-10 10:20:00'),
(1, 3, 5, N'Camera quay video đỉnh chóp, chống rung tốt, mua qua link Lazada của admin nhận được mã giảm thêm 1 triệu đồng.',                             '2026-01-11 14:00:00'),
(2, 4, 4, N'Máy siêu mỏng nhẹ, pin trâu dùng cả ngày không cần mang theo cục sạc. Trừ 1 sao vì bản tiêu chuẩn RAM hơi ít.',                             '2026-02-18 15:35:00'),
(2, 5, 5, N'Màn hình hiển thị xuất sắc, loa nghe hay, phục vụ nhu cầu làm việc văn phòng rất tuyệt vời.',                                                '2026-02-19 09:00:00'),
(4, 6, 5, N'Chống ồn đỉnh cao nhất phân khúc. Mình đeo đi xe buýt bật nhạc lên là như cô lập hoàn toàn với thế giới bên ngoài luôn, rất đáng tiền.',     '2026-04-22 13:00:00');
GO

-- ====================================================================
-- 11. THÊM 40 MEMBERS (demo - MemberID sẽ bắt đầu từ 11)
-- ====================================================================
DECLARE @i INT = 1;
WHILE @i <= 40
BEGIN
    INSERT INTO Members (FullName, Email, PasswordHash, AvatarURL, Status)
    VALUES (
        N'Thành viên Demo ' + CAST(@i AS NVARCHAR),
        'member' + CAST(@i AS VARCHAR) + '@gmail.com',
        'hash_demo_' + CAST(@i AS VARCHAR),
        'default-avatar.png',
        N'Hoạt động'
    );
    SET @i += 1;
END
GO

-- ====================================================================
-- 12. THÊM AFFILIATE LINKS THEO TỪNG POST (loop)
-- ====================================================================
DECLARE @PostID INT = 1;
WHILE @PostID <= (SELECT MAX(PostID) FROM Posts)
BEGIN
    INSERT INTO AffiliateLinks (LinkName, TargetURL, PartnerID, PostID, Status) VALUES
    (N'Shopee Link Post ' + CAST(@PostID AS NVARCHAR), 'https://shopee.vn/product-' + CAST(@PostID AS VARCHAR), 1, @PostID, N'Hoạt động'),
    (N'Lazada Link Post ' + CAST(@PostID AS NVARCHAR), 'https://lazada.vn/product-' + CAST(@PostID AS VARCHAR), 2, @PostID, N'Hoạt động');
    SET @PostID += 1;
END
GO

-- ====================================================================
-- 13. THÊM 100 COMMENTS (demo)
-- ====================================================================
DECLARE @c INT = 1;
WHILE @c <= 100
BEGIN
    INSERT INTO Comments (PostID, MemberID, Content)
    VALUES (
        ((@c - 1) % 26) + 1,
        ((@c - 1) % 40) + 11,  -- dùng MemberID 11..50 (demo members)
        N'Đây là bình luận thử nghiệm số ' + CAST(@c AS NVARCHAR) + N'. Nội dung đánh giá khá hữu ích.'
    );
    SET @c += 1;
END
GO

-- ====================================================================
-- 14. THÊM 80 RATINGS (demo - tránh trùng UC_Member_Post_Rating)
-- ====================================================================
DECLARE @r   INT = 1;
DECLARE @rPostID   INT;
DECLARE @rMemberID INT;

WHILE @r <= 80
BEGIN
    -- PostID xoay 1..26, MemberID bắt đầu từ 11 để không trùng seed thật (1..6)
    SET @rPostID   = ((@r - 1) % 26) + 1;
    SET @rMemberID = ((@r - 1) % 40) + 11;

    IF NOT EXISTS (
        SELECT 1 FROM Ratings
        WHERE PostID = @rPostID AND MemberID = @rMemberID
    )
    BEGIN
        INSERT INTO Ratings (PostID, MemberID, RatingStar, ReviewText)
        VALUES (
            @rPostID,
            @rMemberID,
            ((@r - 1) % 5) + 1,
            N'Đánh giá thử nghiệm số ' + CAST(@r AS NVARCHAR)
        );
    END

    SET @r += 1;
END
GO

-- ====================================================================
-- 15. THÊM 500 CLICK TRACKING (demo)
-- ====================================================================
DECLARE @k INT = 1;
WHILE @k <= 500
BEGIN
    INSERT INTO ClickTracking (LinkID, PartnerID, PostID, MemberID, IPAddress, ReferrerURL, IsSuspicious)
    SELECT TOP 1
        LinkID,
        PartnerID,
        PostID,
        CASE WHEN @k % 3 = 0 THEN NULL ELSE ((@k - 1) % 40) + 11 END,
        '192.168.1.' + CAST((@k % 255) AS VARCHAR),
        'https://ftech.vn',
        CASE WHEN @k % 25 = 0 THEN 1 ELSE 0 END
    FROM AffiliateLinks
    ORDER BY NEWID();

    SET @k += 1;
END
GO

-- ====================================================================
-- IV. KIỂM TRA KẾT QUẢ
-- ====================================================================

-- Thống kê 1: Hiệu suất bài viết (View, Click, CTR)
SELECT
    p.PostID,
    p.Title        AS [Tiêu đề bài viết],
    p.ViewCount    AS [Số lượt xem],
    COUNT(c.ClickID) AS [Tổng số lượt click],
    CASE
        WHEN p.ViewCount > 0
        THEN CAST(COUNT(c.ClickID) AS FLOAT) / p.ViewCount * 100
        ELSE 0
    END AS [Tỷ lệ CTR (%)]
FROM Posts p
LEFT JOIN ClickTracking c ON p.PostID = c.PostID
GROUP BY p.PostID, p.Title, p.ViewCount;

-- Thống kê 2: Hiệu suất đối tác & click gian lận
SELECT
    ap.PartnerName              AS [Tên đối tác],
    ap.CurrentCommissionRate    AS [Tỷ lệ hoa hồng (%)],
    COUNT(CASE WHEN ct.IsSuspicious = 0 THEN 1 END) AS [Lượt click hợp lệ],
    COUNT(CASE WHEN ct.IsSuspicious = 1 THEN 1 END) AS [Lượt click gian lận]
FROM AffiliatePartners ap
LEFT JOIN ClickTracking ct ON ap.PartnerID = ct.PartnerID
GROUP BY ap.PartnerName, ap.CurrentCommissionRate;

-- Thống kê 3: Điểm đánh giá trung bình
SELECT
    p.PostID,
    p.Title AS [Sản phẩm Review],
    AVG(CAST(r.RatingStar AS FLOAT)) AS [Điểm đánh giá trung bình],
    COUNT(r.RatingID)                AS [Tổng số lượt đánh giá]
FROM Posts p
LEFT JOIN Ratings r ON p.PostID = r.PostID
WHERE p.Status = N'Đã xuất bản'
GROUP BY p.PostID, p.Title;
GO