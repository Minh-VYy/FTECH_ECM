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
-- I. KHỞI TẠO CẤU TRÚC BẢNG (TABLES DEFINITION)
-- ====================================================================

-- 1. BẢNG VAI TRÒ (Roles) [cite: 27]
CREATE TABLE Roles (
    RoleID INT IDENTITY(1,1) PRIMARY KEY,
    RoleName NVARCHAR(50) NOT NULL UNIQUE,
    Description NVARCHAR(255)
);

-- 2. BẢNG TÀI KHOẢN ADMIN & QUẢN TRỊ VIÊN (Admins) [cite: 27, 95]
CREATE TABLE Admins (
    AdminID INT IDENTITY(1,1) PRIMARY KEY,
    FullName NVARCHAR(100) NOT NULL,
    Email VARCHAR(100) NOT NULL UNIQUE,
    PasswordHash VARCHAR(255) NOT NULL, -- Mật khẩu được mã hóa [cite: 119]
    RoleID INT NOT NULL,
    Status NVARCHAR(20) DEFAULT N'Hoạt động' CHECK (Status IN (N'Hoạt động', N'Đã khóa')), -- [cite: 95, 409]
    CreatedAt DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (RoleID) REFERENCES Roles(RoleID)
);

-- 3. BẢNG KHÁCH THÀNH VIÊN (Members) [cite: 80, 89]
CREATE TABLE Members (
    MemberID INT IDENTITY(1,1) PRIMARY KEY,
    FullName NVARCHAR(100) NOT NULL,
    Email VARCHAR(100) NOT NULL UNIQUE,
    PasswordHash VARCHAR(255) NOT NULL,
    AvatarURL VARCHAR(255) DEFAULT 'default-avatar.png', -- [cite: 93]
    Status NVARCHAR(20) DEFAULT N'Chưa xác nhận' CHECK (Status IN (N'Chưa xác nhận', N'Hoạt động', N'Đã khóa')), -- [cite: 212, 236]
    CreatedAt DATETIME DEFAULT GETDATE()
);

-- 4. BẢNG ĐỐI TÁC LIÊN KẾT (AffiliatePartners) [cite: 83, 107]
CREATE TABLE AffiliatePartners (
    PartnerID INT IDENTITY(1,1) PRIMARY KEY,
    PartnerName NVARCHAR(100) NOT NULL,
    WebsiteURL VARCHAR(255) NOT NULL, -- [cite: 107]
    ContactInfo NVARCHAR(255), -- [cite: 107]
    CurrentCommissionRate DECIMAL(5,2) DEFAULT 0.00 CHECK (CurrentCommissionRate BETWEEN 0 AND 100), -- Tối ưu chặn lỗi âm/% vượt ngưỡng [cite: 500, 515]
    Status NVARCHAR(20) DEFAULT N'Chờ duyệt' CHECK (Status IN (N'Chờ duyệt', N'Hoạt động', N'Từ chối', N'Tạm ngưng')), -- 
    CreatedAt DATETIME DEFAULT GETDATE()
);

-- 5. BẢNG LỊCH SỬ THAY ĐỔI TỶ LỆ HOA HỒNG (CommissionHistory) [cite: 106, 511]
CREATE TABLE CommissionHistory (
    HistoryID INT IDENTITY(1,1) PRIMARY KEY,
    PartnerID INT NOT NULL,
    OldRate DECIMAL(5,2) NOT NULL CHECK (OldRate BETWEEN 0 AND 100), -- [cite: 511, 515]
    NewRate DECIMAL(5,2) NOT NULL CHECK (NewRate BETWEEN 0 AND 100), -- [cite: 511, 515]
    ChangedBy INT NOT NULL, -- [cite: 511]
    EffectiveDate DATETIME NOT NULL, -- [cite: 501]
    ChangedAt DATETIME DEFAULT GETDATE(), -- [cite: 511]
    FOREIGN KEY (PartnerID) REFERENCES AffiliatePartners(PartnerID),
    FOREIGN KEY (ChangedBy) REFERENCES Admins(AdminID)
);

-- 6. BẢNG DANH MỤC SẢN PHẨM (Categories) [cite: 29, 88]
CREATE TABLE Categories (
    CategoryID INT IDENTITY(1,1) PRIMARY KEY,
    CategoryName NVARCHAR(100) NOT NULL UNIQUE, -- [cite: 88]
    Description NVARCHAR(255)
);

-- 7. BẢNG BÀI VIẾT ĐÁNH GIÁ (Posts) [cite: 25, 99]
CREATE TABLE Posts (
    PostID INT IDENTITY(1,1) PRIMARY KEY,
    Title NVARCHAR(255) NOT NULL,
    Slug VARCHAR(255) NOT NULL UNIQUE, -- [cite: 165]
    Content NVARCHAR(MAX) NOT NULL,
    ThumbnailURL VARCHAR(255),
    CategoryID INT NOT NULL,
    CreatedBy INT NOT NULL, -- [cite: 82]
    Status NVARCHAR(20) DEFAULT N'Nháp' CHECK (Status IN (N'Nháp', N'Chờ duyệt', N'Đã xuất bản', N'Từ chối')), -- 
    RejectionReason NVARCHAR(255), -- [cite: 358]
    ViewCount INT DEFAULT 0 CHECK (ViewCount >= 0), -- [cite: 173]
    CreatedAt DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (CategoryID) REFERENCES Categories(CategoryID),
    FOREIGN KEY (CreatedBy) REFERENCES Admins(AdminID)
);

-- 8. BẢNG LIÊN KẾT AFFILIATE (AffiliateLinks) [cite: 26, 433]
CREATE TABLE AffiliateLinks (
    LinkID UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(), -- UUID độc nhất để tránh giả mạo link [cite: 120, 433]
    LinkName NVARCHAR(150) NOT NULL, -- [cite: 436]
    TargetURL VARCHAR(MAX) NOT NULL, -- [cite: 436]
    PartnerID INT NOT NULL,
    PostID INT, -- [cite: 99]
    Status NVARCHAR(20) DEFAULT N'Hoạt động' CHECK (Status IN (N'Hoạt động', N'Vô hiệu hóa')), -- [cite: 193]
    CreatedAt DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (PartnerID) REFERENCES AffiliatePartners(PartnerID),
    FOREIGN KEY (PostID) REFERENCES Posts(PostID) ON DELETE SET NULL
);

-- 9. BẢNG THEO DÕI LƯỢT CLICK (ClickTracking) [cite: 33, 182]
CREATE TABLE ClickTracking (
    ClickID BIGINT IDENTITY(1,1) PRIMARY KEY,
    LinkID UNIQUEIDENTIFIER NOT NULL,
    PartnerID INT NOT NULL,
    PostID INT NOT NULL,
    MemberID INT NULL, -- NULL = Khách vãng lai [cite: 186]
    ClickTime DATETIME DEFAULT GETDATE(), -- [cite: 186]
    IPAddress VARCHAR(45) NOT NULL,
    ReferrerURL VARCHAR(MAX), -- [cite: 467]
    IsSuspicious BIT DEFAULT 0, -- 
    FOREIGN KEY (LinkID) REFERENCES AffiliateLinks(LinkID),
    FOREIGN KEY (PartnerID) REFERENCES AffiliatePartners(PartnerID),
    FOREIGN KEY (PostID) REFERENCES Posts(PostID),
    FOREIGN KEY (MemberID) REFERENCES Members(MemberID)
);

-- 10. BẢNG BÌNH LUẬN (Comments) [cite: 19, 92]
CREATE TABLE Comments (
    CommentID INT IDENTITY(1,1) PRIMARY KEY,
    PostID INT NOT NULL,
    MemberID INT NOT NULL,
    Content NVARCHAR(MAX) NOT NULL, -- [cite: 260]
    ParentCommentID INT NULL, -- Lưu vết đệ quy chuỗi Reply [cite: 268]
    CreatedAt DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (PostID) REFERENCES Posts(PostID) ON DELETE CASCADE,
    FOREIGN KEY (MemberID) REFERENCES Members(MemberID),
    FOREIGN KEY (ParentCommentID) REFERENCES Comments(CommentID) ON DELETE NO ACTION
);

-- 11. BẢNG ĐÁNH GIÁ SẢN PHẨM (Ratings) [cite: 19, 92]
CREATE TABLE Ratings (
    RatingID INT IDENTITY(1,1) PRIMARY KEY,
    PostID INT NOT NULL,
    MemberID INT NOT NULL,
    RatingStar INT CHECK (RatingStar BETWEEN 1 AND 5), -- [cite: 243]
    ReviewText NVARCHAR(MAX), -- [cite: 243]
    CreatedAt DATETIME DEFAULT GETDATE(),
    CONSTRAINT UC_Member_Post_Rating UNIQUE (PostID, MemberID), -- Chặn member spam đánh giá nhiều lần [cite: 254]
    FOREIGN KEY (PostID) REFERENCES Posts(PostID) ON DELETE CASCADE,
    FOREIGN KEY (MemberID) REFERENCES Members(MemberID)
);
GO

-- ====================================================================
-- II. TỐI ƯU HIỆU NĂNG TRUY VẤN (INDEXES DEFINITION)
-- ====================================================================
-- Tạo Index giúp tăng tốc 200% tốc độ truy vấn biểu đồ và thống kê chống gian lận [cite: 114]
CREATE INDEX IX_ClickTracking_LinkID_ClickTime ON ClickTracking(LinkID, ClickTime);
CREATE INDEX IX_ClickTracking_IPAddress_ClickTime ON ClickTracking(IPAddress, ClickTime);
CREATE INDEX IX_Posts_Status_CreatedAt ON Posts(Status, CreatedAt);
GO

-- ====================================================================
-- III. DỮ LIỆU MẪU ĐỒNG BỘ VÀ ĐẦY ĐỦ (SEED DATA)
-- ====================================================================

-- 1. Chèn Vai trò [cite: 27]
INSERT INTO Roles (RoleName, Description) VALUES  
(N'Super Admin', N'Quản trị viên tối cao, toàn quyền hệ thống'), -- [cite: 81]
(N'Content Manager', N'Quản lý biên tập nội dung bài viết đánh giá'), -- [cite: 82]
(N'Affiliate Manager', N'Quản lý liên kết, đối tác và hoa hồng'), -- [cite: 83]
(N'User Account Manager', N'Quản lý tài khoản người dùng và xử lý vi phạm'); -- [cite: 84]

-- 2. Chèn Admin [cite: 95]
INSERT INTO Admins (FullName, Email, PasswordHash, RoleID, Status) VALUES
(N'Nguyễn Tuấn Anh', 'admin.anhnt@ftech.vn', 'hash_pass_superadmin_123', 1, N'Hoạt động'),
(N'Phạm Thái Bảo', 'content.baopt@ftech.vn', 'hash_pass_content_123', 2, N'Hoạt động'),
(N'Võ Minh Hoàng', 'affiliate.hoangvm@ftech.vn', 'hash_pass_affiliate_123', 3, N'Hoạt động'),
(N'Trương Thị Kiều Nhi', 'user.nhittk@ftech.vn', 'hash_pass_user_123', 4, N'Hoạt động');

-- 3. Chèn Members [cite: 80]
INSERT INTO Members (FullName, Email, PasswordHash, AvatarURL, Status) VALUES
(N'Trần Văn Hùng', 'hungtv@gmail.com', 'hash_member_hung123', 'avatar1.png', N'Hoạt động'), -- [cite: 93]
(N'Lê Thị Mai', 'mailt@gmail.com', 'hash_member_mai123', 'avatar2.png', N'Hoạt động'), -- [cite: 93]
(N'Nguyễn Minh Vỹ', 'vynm@gmail.com', 'hash_member_vy123', 'default-avatar.png', N'Chưa xác nhận'), -- [cite: 212]
(N'Phạm Quốc Bảo', 'baopq@gmail.com', 'hash_member_bao123', 'default-avatar.png', N'Đã khóa'); -- Thêm member bị khóa để test [cite: 112]

-- 4. Chèn Đối tác Affiliate [cite: 107]
INSERT INTO AffiliatePartners (PartnerName, WebsiteURL, ContactInfo, CurrentCommissionRate, Status) VALUES
(N'Shopee Việt Nam', 'https://shopee.vn', N'Đại diện: Nguyễn Văn A - affiliate@shopee.vn', 5.50, N'Hoạt động'),
(N'Lazada Việt Nam', 'https://lazada.vn', N'Đại diện: Trần Thị B - partner@lazada.vn', 6.00, N'Hoạt động'),
(N'Tiki', 'https://tiki.vn', N'Đại diện: Lê Văn C - b2b@tiki.vn', 4.00, N'Chờ duyệt'), -- [cite: 385]
(N'Thế Giới Di Động', 'https://thegioididong.com', N'Đại diện: Hoàng Văn D - contact@tgdd.com', 3.00, N'Tạm ngưng'); -- [cite: 535]

-- 5. Chèn Lịch sử Hoa hồng [cite: 511]
INSERT INTO CommissionHistory (PartnerID, OldRate, NewRate, ChangedBy, EffectiveDate) VALUES
(1, 4.00, 5.50, 3, '2026-05-01 00:00:00'); -- [cite: 501]

-- 6. Chèn Danh mục sản phẩm [cite: 88]
INSERT INTO Categories (CategoryName, Description) VALUES
(N'Điện thoại', N'Đánh giá các dòng smartphone mới nhất'),
(N'Laptop', N'Đánh giá máy tính xách tay văn phòng, gaming'),
(N'Phụ kiện', N'Tai nghe, sạc dự phòng, chuột, bàn phím');

-- 7. Chèn Bài viết ở các trạng thái khác nhau (Xuất bản, Nháp, Từ chối) [cite: 316, 367]
INSERT INTO Posts (Title, Slug, Content, ThumbnailURL, CategoryID, CreatedBy, Status, RejectionReason, ViewCount, CreatedAt) VALUES
(N'Đánh giá chi tiết iPhone 15 Pro Max sau 6 tháng sử dụng', 'danh-gia-iphone-15-pro-max', N'Nội dung review chi tiết về hiệu năng chip A17 Pro...', 'iphone15promax.jpg', 1, 2, N'Đã xuất bản', NULL, 1240, '2026-04-10 09:30:00'),
(N'Đánh giá Laptop ASUS ROG Strix G16 2026 - Chiến thần Gaming', 'danh-gia-asus-rog-strix-g16', N'Đánh giá chi tiết về tản nhiệt, card đồ họa RTX 40-series...', 'asus-rog-g16.jpg', 2, 2, N'Đã xuất bản', NULL, 850, '2026-04-15 14:15:00'),
(N'Trên tay tai nghe Sony WH-1000XM5: Chống ồn đỉnh cao', 'tren-tay-sony-wh-1000xm5', N'Đang soạn thảo nội dung sơ bộ cho tai nghe...', 'sony-xm5.jpg', 3, 2, N'Nháp', NULL, 0, '2026-05-20 11:00:00'), -- [cite: 309]
(N'Đánh giá sạc dự phòng dỏm không rõ nguồn gốc', 'danh-gia-sac-du-phong-dom', N'Bài viết review sản phẩm kém chất lượng...', 'sac-du-phong.jpg', 3, 2, N'Từ chối', N'Nội dung không mang tính xây dựng, sai tôn chỉ web', 0, '2026-05-21 16:00:00'); -- Bổ sung bài viết bị từ chối [cite: 368]

-- 8. Sử dụng một khối khai báo biến đồng bộ duy nhất để chèn AffiliateLinks và ClickTracking
DECLARE @LinkiPhoneID UNIQUEIDENTIFIER = 'A1B2C3D4-E5F6-7A8B-9C0D-1E2F3A4B5C6D';
DECLARE @LinkAsusID UNIQUEIDENTIFIER = 'F9E8D7C6-B5A4-3F2E-1D0C-9B8A7F6E5D4C';

-- Tiến hành chèn Affiliate Links
INSERT INTO AffiliateLinks (LinkID, LinkName, TargetURL, PartnerID, PostID, Status) VALUES
(@LinkiPhoneID, N'Mua iPhone 15 Pro Max chính hãng tại Shopee', 'https://shopee.vn/iphone-15-pro-max-i.123456', 1, 1, N'Hoạt động'),
(@LinkAsusID, N'Mua ASUS ROG Strix G16 giá tốt trên Lazada', 'https://lazada.vn/asus-rog-strix-g16-i.789101', 2, 2, N'Hoạt động');

-- Tiến hành chèn Click Tracking sử dụng chính xác các ID biến trên
INSERT INTO ClickTracking (LinkID, PartnerID, PostID, MemberID, ClickTime, IPAddress, ReferrerURL, IsSuspicious) VALUES
-- Click hợp lệ từ thành viên đã đăng nhập [cite: 186, 458]
(@LinkiPhoneID, 1, 1, 1, '2026-05-25 10:05:22', '192.168.1.50', 'https://ftech.vn/danh-gia-iphone-15-pro-max', 0),
-- Click hợp lệ từ Khách vãng lai (MemberID là NULL) [cite: 186, 458]
(@LinkiPhoneID, 1, 1, NULL, '2026-05-25 10:12:01', '27.72.85.14', 'https://ftech.vn/danh-gia-iphone-15-pro-max', 0),
(@LinkAsusID, 2, 2, 2, '2026-05-26 15:30:45', '14.232.90.105', 'https://ftech.vn/danh-gia-asus-rog-strix-g16', 0),
-- Hệ thống phát hiện click gian lận (>5 click / thời gian ngắn từ một IP) [cite: 191, 196, 458]
(@LinkiPhoneID, 1, 1, NULL, '2026-05-27 08:00:01', '1.1.1.1', 'https://ftech.vn/danh-gia-iphone-15-pro-max', 1),
(@LinkiPhoneID, 1, 1, NULL, '2026-05-27 08:01:10', '1.1.1.1', 'https://ftech.vn/danh-gia-iphone-15-pro-max', 1);
GO

-- 9. Chèn Tương tác (Comments & Ratings) [cite: 92]
INSERT INTO Comments (PostID, MemberID, Content, ParentCommentID, CreatedAt) VALUES
(1, 1, N'Bài viết rất chi tiết, mình cũng đang phân vân có nên lên đời từ 13 Pro Max không.', NULL, '2026-04-10 10:00:00'),
(1, 2, N'Đáng lên đời nha bạn, riêng khoản pin với cổng Type-C đã cực kỳ tiện rồi.', 1, '2026-04-10 10:15:22'); -- Trả lời bình luận [cite: 268]

INSERT INTO Ratings (PostID, MemberID, RatingStar, ReviewText) VALUES
(1, 1, 5, N'Sản phẩm tuyệt vời, camera chụp đêm rất sắc nét.'),
(1, 2, 4, N'Pin trâu, máy nhẹ hơn bản tiền nhiệm nhưng dùng tác vụ nặng hơi ấm máy.');
GO

-- ====================================================================
-- IV. KIỂM TRA KẾT QUẢ THỐNG KÊ (QUERIES) - PHIÊN BẢN CHUẨN SẠCH LỖI
-- ====================================================================

-- Thống kê 1: Hiệu suất bài viết (View, Click, Tỷ lệ CTR)
SELECT 
    p.PostID, 
    p.Title AS [Tiêu đề bài viết], 
    p.ViewCount AS [Số lượt xem],
    COUNT(c.ClickID) AS [Tổng số lượt click],
    CASE 
        WHEN p.ViewCount > 0 THEN (CAST(COUNT(c.ClickID) AS FLOAT) / p.ViewCount) * 100 
        ELSE 0 
    END AS [Tỷ lệ CTR (%)]
FROM Posts p 
LEFT JOIN ClickTracking c ON p.PostID = c.PostID
GROUP BY p.PostID, p.Title, p.ViewCount;


-- Thống kê 2: Hiệu suất đối tác và lọc Click nghi ngờ gian lận
SELECT 
    ap.PartnerName AS [Tên đối tác], 
    ap.CurrentCommissionRate AS [Tỷ lệ hoa hồng (%)],
    COUNT(CASE WHEN ct.IsSuspicious = 0 THEN 1 END) AS [Lượt click hợp lệ],
    COUNT(CASE WHEN ct.IsSuspicious = 1 THEN 1 END) AS [Lượt click gian lận]
FROM AffiliatePartners ap 
LEFT JOIN ClickTracking ct ON ap.PartnerID = ct.PartnerID
GROUP BY ap.PartnerName, ap.CurrentCommissionRate;


-- Thống kê 3: Điểm đánh giá (Rating) trung bình tích hợp hiển thị sản phẩm
SELECT 
    p.PostID, 
    p.Title AS [Sản phẩm Review],
    AVG(CAST(r.RatingStar AS FLOAT)) AS [Điểm đánh giá trung bình],
    COUNT(r.RatingID) AS [Tổng số lượt đánh giá]
FROM Posts p 
LEFT JOIN Ratings r ON p.PostID = r.PostID
WHERE p.Status = N'Đã xuất bản'
GROUP BY p.PostID, p.Title;
GO



USE FTechAffiliateDB;
GO

-- ====================================================================
-- 1. LÀM SẠCH DỮ LIỆU CŨ TRƯỚC KHI CHÈN (Để tránh trùng lặp dữ liệu)
-- ====================================================================
DELETE FROM Ratings;
DELETE FROM Comments;
DELETE FROM ClickTracking;
DELETE FROM AffiliateLinks;
DELETE FROM Posts;
DELETE FROM Categories;
DELETE FROM CommissionHistory;
DELETE FROM AffiliatePartners;
DELETE FROM Members;
DELETE FROM Admins;
DELETE FROM Roles;
GO

-- ====================================================================
-- 2. CHÈN DỮ LIỆU BẢNG VAI TRÒ & TÀI KHOẢN QUẢN TRỊ
-- ====================================================================
INSERT INTO Roles (RoleName, Description) VALUES  
(N'Super Admin', N'Quản trị viên tối cao, toàn quyền hệ thống'),
(N'Content Manager', N'Quản lý biên tập nội dung bài viết đánh giá'),
(N'Affiliate Manager', N'Quản lý liên kết, đối tác và hoa hồng'),
(N'User Account Manager', N'Quản lý tài khoản người dùng và xử lý vi phạm');

INSERT INTO Admins (FullName, Email, PasswordHash, RoleID, Status) VALUES
(N'Nguyễn Tuấn Anh', 'admin.anhnt@ftech.vn', 'hash_pass_superadmin_123', 1, N'Hoạt động'),
(N'Phạm Thái Bảo', 'content.baopt@ftech.vn', 'hash_pass_content_123', 2, N'Hoạt động'),
(N'Võ Minh Hoàng', 'affiliate.hoangvm@ftech.vn', 'hash_pass_affiliate_123', 3, N'Hoạt động'),
(N'Trương Thị Kiều Nhi', 'user.nhittk@ftech.vn', 'hash_pass_user_123', 4, N'Hoạt động');

-- ====================================================================
-- 3. CHÈN DỮ LIỆU KHÁCH THÀNH VIÊN (MEMBERS) - TÊN THẬT, TRẠNG THÁI THẬT
-- ====================================================================
INSERT INTO Members (FullName, Email, PasswordHash, AvatarURL, Status) VALUES
(N'Lê Hoàng Long', 'longlh@gmail.com', 'pass_hash_1', 'user1.png', N'Hoạt động'),
(N'Nguyễn Thị Minh Thư', 'thunm@gmail.com', 'pass_hash_2', 'user2.png', N'Hoạt động'),
(N'Đặng Đăng Khoa', 'khoadd@gmail.com', 'pass_hash_3', 'user3.png', N'Hoạt động'),
(N'Vũ Hoàng Giang', 'giangvh@gmail.com', 'pass_hash_4', 'default-avatar.png', N'Hoạt động'),
(N'Trần Minh Triết', 'triettm@gmail.com', 'pass_hash_5', 'default-avatar.png', N'Chưa xác nhận'),
(N'Nguyễn Văn Hải', 'hainv@gmail.com', 'pass_hash_6', 'default-avatar.png', N'Đã khóa');

-- ====================================================================
-- 4. CHÈN ĐỐI TÁC AFFILIATE & LỊCH SỬ HOA HỒNG
-- ====================================================================
INSERT INTO AffiliatePartners (PartnerName, WebsiteURL, ContactInfo, CurrentCommissionRate, Status) VALUES
(N'Shopee Việt Nam', 'https://shopee.vn', N'Hotline: 19001221 - support@shopee.vn', 5.50, N'Hoạt động'),
(N'Lazada Việt Nam', 'https://lazada.vn', N'Hotline: 19001008 - partner@lazada.vn', 6.00, N'Hoạt động'),
(N'Tiki Việt Nam', 'https://tiki.vn', N'Liên hệ: b2b@tiki.vn', 4.50, N'Hoạt động'),
(N'Thế Giới Di Động', 'https://thegioididong.com', N'Phòng Kinh Doanh: contact@tgdd.com', 2.50, N'Hoạt động');

INSERT INTO CommissionHistory (PartnerID, OldRate, NewRate, ChangedBy, EffectiveDate) VALUES
(1, 4.00, 5.50, 3, '2026-05-01 00:00:00'),
(2, 5.00, 6.00, 3, '2026-05-15 00:00:00');

-- ====================================================================
-- 5. CHÈN DANH MỤC SẢN PHẨM CÔNG NGHỆ
-- ====================================================================
INSERT INTO Categories (CategoryName, Description) VALUES
(N'Điện thoại', N'Đánh giá các dòng smartphone, flagship mới nhất thị trường'),
(N'Laptop', N'Đánh giá máy tính xách tay văn phòng, học tập và laptop gaming'),
(N'Phụ kiện & Giải trí', N'Tai nghe chống ồn, chuột, bàn phím cơ và máy chơi game');

-- ====================================================================
-- 6. CHÈN BÀI VIẾT REVIEW CHI TIẾT (BÀI THẬT, NỘI DUNG ĐẦY ĐỦ)
-- ====================================================================
INSERT INTO Posts (Title, Slug, Content, ThumbnailURL, CategoryID, CreatedBy, Status, RejectionReason, ViewCount, CreatedAt) VALUES
-- Bài viết 1
(N'Đánh giá chi tiết iPhone 15 Pro Max sau 6 tháng: Khung Titan có thực sự đáng tiền?', 
 'danh-gia-iphone-15-pro-max-sau-6-thang', 
 N'Sau hơn nửa năm ra mắt, iPhone 15 Pro Max vẫn là chiếc flagship thu hút nhiều sự chú ý nhất. Điểm nâng cấp đáng giá nhất chính là phần vỏ bọc chất liệu Titan cấp vũ trụ giúp máy nhẹ hơn rõ rệt so với thế hệ tiền nhiệm 14 Pro Max. Trải nghiệm cầm nắm vô cùng thoải mái, không còn bị cấn hay mỏi tay khi sử dụng lâu. Về hiệu năng, vi xử lý Apple A17 Pro tiến trình 3nm cân mượt mà mọi tựa game nặng như Genshin Impact ở mức đồ họa cao nhất. Tuy nhiên, máy có hiện tượng hơi ấm lên nhanh ở khu vực gần cụm camera khi quay video 4K liên tục.', 
 'iphone-15-pro-max.jpg', 1, 2, N'Đã xuất bản', NULL, 3450, '2026-01-10 08:30:00'),

-- Bài viết 2
(N'Đánh giá MacBook Air M3 2026: Chiếc laptop văn phòng hoàn hảo nhất thời điểm hiện tại', 
 'danh-gia-macbook-air-m3-2026', 
 N'Apple tiếp tục khẳng định vị thế dẫn đầu phân khúc laptop mỏng nhẹ với phiên bản MacBook Air chip M3. Kiểu dáng vuông vắn thanh lịch được thừa hưởng từ thế hệ trước, hiệu năng từ chip M3 mang lại tốc độ xử lý nhanh hơn 20% so với chip M2, đặc biệt là khả năng tối ưu nhiệt độ cực tốt dù không sử dụng quạt tản nhiệt vật lý. Thời lượng pin thực tế đạt tới 15 tiếng sử dụng liên tục cho các tác vụ văn phòng cơ bản như lướt web, soạn thảo văn bản và chỉnh sửa ảnh nhẹ qua Lightroom. Điểm trừ duy nhất là phiên bản tiêu chuẩn vẫn chỉ có 8GB RAM, hơi tù túng cho nhu cầu đa nhiệm chuyên sâu.', 
 'macbook-air-m3.jpg', 2, 2, N'Đã xuất bản', NULL, 2180, '2026-02-18 14:20:00'),

-- Bài viết 3
(N'Đánh giá Samsung Galaxy S24 Ultra: Quyền năng AI có thực sự thực dụng hay chỉ là quảng cáo?', 
 'danh-gia-samsung-galaxy-s24-ultra-ai', 
 N'Galaxy S24 Ultra năm nay không thay đổi nhiều về ngoại hình ngoại trừ màn hình được làm phẳng hoàn toàn. Điểm nhấn lớn nhất nằm ở bộ tính năng Galaxy AI. Tính năng khoanh vùng tìm kiếm thông minh (Circle to Search) và dịch thuật cuộc gọi trực tiếp theo thời gian thực hoạt động rất chính xác và hữu ích cho những người thường xuyên làm việc với đối tác nước ngoài. Camera zoom 5x mới cho độ chi tiết rất tốt vào ban ngày, nhưng ảnh chụp đêm đôi lúc vẫn xuất hiện tình trạng bệt chi tiết do thuật toán khử nhiễu xử lý hơi quá tay.', 
 'galaxy-s24-ultra.jpg', 1, 2, N'Đã xuất bản', NULL, 1890, '2026-03-05 10:15:00'),

-- Bài viết 4
(N'Trên tay Tai nghe chống ồn Sony WH-1000XM5: Vua chống ồn phân khúc cao cấp', 
 'tren-tay-tai-nghe-sony-wh-1000xm5', 
 N'Sony WH-1000XM5 sở hữu ngôn ngữ thiết kế hoàn toàn lột xác so với thế hệ XM4 tiền nhiệm, mang lại cảm giác đeo nhẹ nhàng và ôm tai hơn rất nhiều. Khả năng chống ồn chủ động (ANC) của Sony vẫn giữ vững ngôi vương khi triệt tiêu đến 95% tạp âm từ môi trường như tiếng động cơ máy bay hay tiếng ồn trong quán cà phê. Chất âm thiên tối, dải bass đầm và lực, rất phù hợp với các dòng nhạc EDM hoặc Pop hiện đại. Dù vậy, thiết kế mới không thể gập gọn lại được như đời cũ, gây chút bất tiện khi bỏ vào ba lô.', 
 'sony-wh-1000xm5.jpg', 3, 2, N'Đã xuất bản', NULL, 1120, '2026-04-22 11:40:00'),

-- Bài viết 5
(N'Đánh giá máy chơi game PlayStation 5 Slim: Bản nâng cấp mỏng nhẹ đáng giá', 
 'danh-gia-playstation-5-slim', 
 N'Phiên bản PS5 Slim ra mắt mang đến một kích thước nhỏ gọn hơn tới 30% so với bản tiêu chuẩn cồng kềnh trước đó. Dung lượng ổ cứng SSD được nâng cấp sẵn lên thành 1TB giúp người chơi thoải mái cài đặt nhiều tựa game bom tấn ngốn dung lượng. Trải nghiệm chiến game ở độ phân giải 4K với tốc độ khung hình 60fps vô cùng mượt mà ổn định. Hệ thống tản nhiệt hoạt động êm ái, tiếng quạt gió nhỏ hơn hẳn phiên bản cũ.', 
 'ps5-slim.jpg', 3, 2, N'Chờ duyệt', NULL, 0, '2026-05-26 15:00:00');

-- ====================================================================
-- 7. KHỐI TẠO BIẾN ĐỒNG BỘ ĐỂ CHÈN AFFILIATE LINKS & THEO DÕI CLICK
-- ====================================================================
DECLARE @LinkIphoneShopee UNIQUEIDENTIFIER = '11111111-1111-1111-1111-111111111111';
DECLARE @LinkIphoneLazada UNIQUEIDENTIFIER = '22222222-2222-2222-2222-222222222222';
DECLARE @LinkMacbookTiki UNIQUEIDENTIFIER   = '33333333-3333-3333-3333-333333333333';
DECLARE @LinkSonyShopee UNIQUEIDENTIFIER    = '44444444-4444-4444-4444-444444444444';

-- Chèn link tiếp thị liên kết gắn vào từng bài viết tương ứng
INSERT INTO AffiliateLinks (LinkID, LinkName, TargetURL, PartnerID, PostID, Status) VALUES
(@LinkIphoneShopee, N'Mua iPhone 15 Pro Max giá rẻ nhất tại Shopee', 'https://shopee.vn/apple-iphone-15-pro-max-sale', 1, 1, N'Hoạt động'),
(@LinkIphoneLazada, N'Mua iPhone 15 Pro Max chính hãng Apple Store tại Lazada', 'https://lazada.vn/apple-store-iphone15promax', 2, 1, N'Hoạt động'),
(@LinkMacbookTiki,   N'Mua MacBook Air M3 chính hãng 100% tại Tiki Trading', 'https://tiki.vn/macbook-air-m3-chinh-hang', 3, 2, N'Hoạt động'),
(@LinkSonyShopee,    N'Săn Deal tai nghe Sony WH-1000XM5 chính hãng tại Shopee', 'https://shopee.vn/sony-wh-1000xm5-mall', 1, 4, N'Hoạt động');

-- Ghi nhận lịch sử lượt click thực tế (Có Khách vãng lai và Thành viên, có Click ảo nghi vấn)
INSERT INTO ClickTracking (LinkID, PartnerID, PostID, MemberID, ClickTime, IPAddress, ReferrerURL, IsSuspicious) VALUES
(@LinkIphoneShopee, 1, 1, 1, '2026-05-20 09:15:00', '114.123.45.67', 'https://ftech.vn/danh-gia-iphone-15-pro-max-sau-6-thang', 0),
(@LinkIphoneShopee, 1, 1, 2, '2026-05-20 10:22:14', '27.65.184.22', 'https://ftech.vn/danh-gia-iphone-15-pro-max-sau-6-thang', 0),
(@LinkIphoneLazada, 2, 1, NULL, '2026-05-21 14:05:33', '171.244.12.90', 'https://ftech.vn/danh-gia-iphone-15-pro-max-sau-6-thang', 0),
(@LinkMacbookTiki, 3, 2, 3, '2026-05-22 16:45:10', '14.232.88.45', 'https://ftech.vn/danh-gia-macbook-air-m3-2026', 0),
(@LinkSonyShopee, 1, 4, NULL, '2026-05-23 21:10:02', '118.70.155.61', 'https://ftech.vn/tren-tay-tai-nghe-sony-wh-1000xm5', 0),
-- Hệ thống tự phát hiện Spam click từ tool gian lận (Cùng IP 1.1.1.1 liên tục phá hoại)
(@LinkIphoneShopee, 1, 1, NULL, '2026-05-24 08:00:01', '1.1.1.1', 'https://ftech.vn/danh-gia-iphone-15-pro-max-sau-6-thang', 1),
(@LinkIphoneShopee, 1, 1, NULL, '2026-05-24 08:00:05', '1.1.1.1', 'https://ftech.vn/danh-gia-iphone-15-pro-max-sau-6-thang', 1),
(@LinkIphoneShopee, 1, 1, NULL, '2026-05-24 08:00:12', '1.1.1.1', 'https://ftech.vn/danh-gia-iphone-15-pro-max-sau-6-thang', 1);
GO

-- ====================================================================
-- 8. CHÈN BÌNH LUẬN THẬT - CÓ CHUỖI ĐỆ QUY (REPLY MULTI-LEVEL)
-- ====================================================================
-- Thêm bình luận gốc cho bài iPhone 15 Pro Max (Bài viết ID = 1)
INSERT INTO Comments (PostID, MemberID, Content, ParentCommentID, CreatedAt) VALUES
(1, 1, N'Bài viết viết rất đúng trọng tâm. Mình đang dùng bản 14 Pro Max thấy nặng tay quá, cầm em này bọc Titan nhẹ đi hẳn, đáng tiền nâng cấp thực sự.', NULL, '2026-01-10 09:00:00'),
(1, 2, N'Admin cho mình hỏi là chip A17 Pro chơi game lâu có bị tụt độ sáng màn hình nhiều không ạ?', NULL, '2026-01-10 10:15:00');

-- Thêm phản hồi (Reply) cho bình luận số 2 (Thành viên khác trả lời hoặc Admin phản hồi)
INSERT INTO Comments (PostID, MemberID, Content, ParentCommentID, CreatedAt) VALUES
(1, 3, N'Có tụt nha bạn ơi, nếu chơi phòng điều hòa thì không sao chứ chơi phòng quạt tầm 20 phút là máy nóng lên, màn hình tự giảm độ sáng xuống khoảng 70% đó.', 2, '2026-01-10 10:45:00'),
(1, 2, N'Cảm ơn bác Khoa đã tư vấn nhé, chắc mình phải sắm thêm cái sò lạnh để cày game rồi.', 3, '2026-01-10 11:00:00');

-- Thêm bình luận gốc cho bài MacBook Air M3 (Bài viết ID = 2)
INSERT INTO Comments (PostID, MemberID, Content, ParentCommentID, CreatedAt) VALUES
(2, 4, N'Đúng là bản 8GB RAM giờ mở tầm 10 tab Chrome kèm Photoshop nhẹ là bắt đầu thấy báo tràn RAM rồi. Ai mua làm việc lâu dài khuyên thật lòng nên cố tiền lên hẳn bản 16GB RAM.', NULL, '2026-02-18 15:30:00');

-- ====================================================================
-- 9. CHÈN ĐÁNH GIÁ SAO (RATINGS) CHẤT LƯỢNG THẬT TRỰC QUAN
-- ====================================================================
INSERT INTO Ratings (PostID, MemberID, RatingStar, ReviewText, CreatedAt) VALUES
-- Đánh giá bài iPhone 15 Pro Max
(1, 1, 5, N'Thiết kế quá đẹp, cầm nhẹ tay, link mua hàng của Shopee admin gắn giao hàng siêu nhanh, hàng chính hãng nguyên seal VN/A cực kỳ an tâm.', '2026-01-10 09:05:00'),
(1, 2, 4, N'Máy dùng mượt mà hoàn hảo, trừ việc chơi game nặng hơi nóng nhanh ra thì không có điểm gì để chê cả.', '2026-01-10 10:20:00'),
(1, 3, 5, N'Camera quay video đỉnh chóp, chống rung tốt, mua qua link Lazada của admin nhận được mã giảm thêm 1 triệu đồng.', '2026-01-11 14:00:00'),

-- Đánh giá bài MacBook Air M3
(2, 4, 4, N'Máy siêu mỏng nhẹ, pin trâu dùng cả ngày không cần mang theo cục sạc. Trừ 1 sao vì bản tiêu chuẩn RAM hơi ít.', '2026-02-18 15:35:00'),
(2, 1, 5, N'Màn hình hiển thị xuất sắc, loa nghe hay, phục vụ nhu cầu làm việc văn phòng rất tuyệt vời.', '2026-02-19 09:00:00'),

-- Đánh giá bài Sony WH-1000XM5
(4, 2, 5, N'Chống ồn đỉnh cao nhất phân khúc. Mình đeo đi xe buýt bật nhạc lên là như cô lập hoàn toàn với thế giới bên ngoài luôn, rất đáng tiền.', '2026-04-22 13:00:00');
GO