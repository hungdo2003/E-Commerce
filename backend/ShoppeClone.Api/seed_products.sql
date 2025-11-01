-- Xóa dữ liệu cũ
DELETE FROM CartItems WHERE ProductId IN (SELECT Id FROM Products);
DELETE FROM OrderItems WHERE ProductId IN (SELECT Id FROM Products);
DELETE FROM Products;
DELETE FROM Categories;

-- Reset identity
DBCC CHECKIDENT ('Categories', RESEED, 0);
DBCC CHECKIDENT ('Products', RESEED, 0);

-- Thêm Categories
SET IDENTITY_INSERT Categories ON;
INSERT INTO Categories (Id, Name, CreatedAt) VALUES 
(1, N'Điện Thoại & Phụ Kiện', GETUTCDATE()),
(2, N'Laptop & Máy Tính', GETUTCDATE()),
(3, N'Thời Trang Nam', GETUTCDATE()),
(4, N'Thời Trang Nữ', GETUTCDATE()),
(5, N'Đồng Hồ', GETUTCDATE()),
(6, N'Giày Dép', GETUTCDATE()),
(7, N'Gia Dụng & Đời Sống', GETUTCDATE()),
(8, N'Sức Khỏe & Làm Đẹp', GETUTCDATE());
SET IDENTITY_INSERT Categories OFF;

-- Thêm Products
SET IDENTITY_INSERT Products ON;

-- Điện Thoại
INSERT INTO Products (Id, Name, Description, Price, Stock, CategoryId, ThumbnailUrl, CreatedAt) VALUES
(1, N'iPhone 15 Pro Max 256GB', N'Điện thoại cao cấp nhất của Apple với thiết kế khung viền titan sang trọng, bền bỉ. Trang bị chip A17 Pro mạnh mẽ nhất thế giới smartphone, xử lý mọi tác vụ mượt mà. Camera chính 48MP chụp ảnh siêu nét, quay video 4K ProRes. Màn hình Super Retina XDR 6.7 inch hiển thị sống động. Hỗ trợ 5G, sạc nhanh, chống nước IP68. Bảo hành chính hãng 12 tháng.', 29990000, 50, 1, 'https://images.fpt.shop/unsafe/fit-in/800x800/filters:quality(90):fill(white)/fptshop.com.vn/Uploads/Originals/2023/9/13/638299666919240690_iphone-15-pro-max-xanh-1.jpg', GETUTCDATE()),

(2, N'Samsung Galaxy S24 Ultra 256GB', N'Siêu phẩm flagship của Samsung với bút S Pen tích hợp độc quyền, viết vẽ mượt mà như trên giấy. Màn hình Dynamic AMOLED 2X 6.8 inch siêu lớn, độ phân giải QHD+ sắc nét. Camera chính 200MP chụp ảnh chi tiết đến từng pixel, zoom quang học 10x. Chip Snapdragon 8 Gen 3 mạnh mẽ, RAM 12GB đa nhiệm tốt. Pin 5000mAh sử dụng cả ngày, sạc nhanh 45W. Chống nước IP68, bảo hành 12 tháng.', 27990000, 45, 1, 'https://images.fpt.shop/unsafe/fit-in/800x800/filters:quality(90):fill(white)/fptshop.com.vn/Uploads/Originals/2024/1/17/638412346738344529_samsung-galaxy-s24-ultra-xam-1.jpg', GETUTCDATE()),

(3, N'Xiaomi 14 Ultra 16GB/512GB', N'Điện thoại chụp ảnh chuyên nghiệp với hệ thống camera Leica 50MP, cảm biến Sony LYT-900 siêu lớn. Màn hình AMOLED 6.73 inch 120Hz mượt mà, độ sáng 3000 nits. Chip Snapdragon 8 Gen 3 hiệu năng khủng, RAM 16GB đa nhiệm mạnh mẽ. Bộ nhớ 512GB lưu trữ thoải mái. Pin 5000mAh, sạc nhanh 90W đầy pin trong 20 phút, sạc không dây 50W tiện lợi. Thiết kế cao cấp, chống nước IP68.', 24990000, 30, 1, 'https://images.fpt.shop/unsafe/fit-in/800x800/filters:quality(90):fill(white)/fptshop.com.vn/Uploads/Originals/2024/2/22/638442716334984262_xiaomi-14-ultra-den-1.jpg', GETUTCDATE()),

(4, N'OPPO Find X7 Ultra 16GB/512GB', N'Điện thoại camera đỉnh cao với hệ thống 4 camera Hasselblad 50MP chuyên nghiệp, chụp mọi khoảnh khắc hoàn hảo. Màn hình AMOLED 6.82 inch 120Hz siêu mượt, độ sáng 4500 nits ngoài trời. Chip Snapdragon 8 Gen 3 mạnh mẽ, RAM 16GB xử lý tốt mọi tác vụ. Bộ nhớ 512GB lưu trữ tha hồ. Pin 5000mAh bền bỉ, sạc siêu nhanh 100W chỉ 25 phút đầy pin. Thiết kế sang trọng, chống nước IP68.', 23990000, 35, 1, 'https://images.fpt.shop/unsafe/fit-in/800x800/filters:quality(90):fill(white)/fptshop.com.vn/Uploads/Originals/2024/3/22/638466895175961467_oppo-find-x7-ultra-xanh-1.jpg', GETUTCDATE()),

(5, N'iPhone 14 128GB', N'Điện thoại iPhone thế hệ 14 với màn hình Super Retina XDR 6.1 inch sắc nét, màu sắc chân thực. Chip A15 Bionic mạnh mẽ, xử lý mượt mà mọi tác vụ. Camera kép 12MP chụp ảnh đẹp, quay video 4K Cinematic Mode. Face ID bảo mật cao, mở khóa nhanh chóng. Hỗ trợ 5G tốc độ cao, kháng nước IP68 an tâm sử dụng. Pin cả ngày, sạc nhanh 20W. Thiết kế sang trọng với nhiều màu sắc lựa chọn.', 18990000, 60, 1, 'https://images.fpt.shop/unsafe/fit-in/800x800/filters:quality(90):fill(white)/fptshop.com.vn/Uploads/Originals/2022/9/28/638000899919346336_iphone-14-tim-1.jpg', GETUTCDATE()),

(6, N'Samsung Galaxy A55 5G 8GB/128GB', N'Điện thoại tầm trung cao cấp với màn hình Super AMOLED 6.6 inch 120Hz mượt mà, hiển thị sống động. Chip Exynos 1480 mạnh mẽ, RAM 8GB đa nhiệm tốt. Camera chính 50MP chụp ảnh sắc nét, quay video 4K ổn định. Pin 5000mAh dùng cả ngày, sạc nhanh 25W. Thiết kế khung kim loại cao cấp, chống nước IP67 bảo vệ tốt. Hỗ trợ 5G, bảo hành chính hãng 12 tháng. Giá tốt, đáng mua nhất phân khúc.', 9990000, 100, 1, 'https://images.fpt.shop/unsafe/fit-in/800x800/filters:quality(90):fill(white)/fptshop.com.vn/Uploads/Originals/2024/3/11/638457347624220944_samsung-galaxy-a55-5g-xanh-dam-1.jpg', GETUTCDATE()),

-- Laptop
(7, N'MacBook Air M2 13 inch 256GB', N'Laptop siêu mỏng nhẹ chỉ 11.3mm, trọng lượng 1.24kg dễ dàng mang theo. Chip M2 thế hệ mới hiệu năng vượt trội, xử lý nhanh mọi tác vụ. Màn hình Liquid Retina 13.6 inch độ phân giải 2560x1664 hiển thị sắc nét. Pin 18 giờ sử dụng liên tục, sạc nhanh MagSafe tiện lợi. Bàn phím Magic Keyboard êm ái, trackpad lớn thao tác chính xác. Touch ID bảo mật vân tay. RAM 8GB, SSD 256GB. Thiết kế sang trọng 4 màu: bạc, xám, vàng, xanh.', 26990000, 30, 2, 'https://cdn.tgdd.vn/Products/Images/44/282827/apple-macbook-air-m2-2022-xam-1.jpg', GETUTCDATE()),

(8, N'Dell XPS 13 Plus i7 1360P 16GB/512GB', N'Laptop doanh nhân cao cấp với thiết kế tối giản, sang trọng. Màn hình OLED 13.4 inch 3.5K siêu nét, độ phủ màu 100% DCI-P3. Chip Intel Core i7 thế hệ 13 mạnh mẽ, RAM 16GB đa nhiệm tốt. SSD 512GB tốc độ cao. Bàn phím cảm ứng công nghệ mới, touchpad lớn. 2 cổng Thunderbolt 4 kết nối đa năng. Vỏ nhôm nguyên khối bền bỉ. Trọng lượng 1.23kg siêu nhẹ. Pin 10 giờ làm việc. Bảo hành 12 tháng.', 35990000, 20, 2, 'https://images.fpt.shop/unsafe/fit-in/800x800/filters:quality(90):fill(white)/fptshop.com.vn/Uploads/Originals/2023/5/18/638201098988234068_dell-xps-13-plus-9320-bac-1.jpg', GETUTCDATE()),

(9, N'Asus ROG Strix G16 i7 RTX4060 16GB/512GB', N'Laptop gaming mạnh mẽ với màn hình 16 inch FHD 165Hz mượt mà, thời gian phản hồi 3ms. Chip Intel Core i7 thế hệ 13 hiệu năng cao, card đồ họa RTX 4060 8GB chơi game mượt. RAM 16GB DDR5 tốc độ nhanh, SSD 512GB NVMe. Hệ thống tản nhiệt tiên tiến với 2 quạt và nhiều ống dẫn nhiệt. Bàn phím RGB Aura Sync đẹp mắt. Cổng kết nối đầy đủ. Pin 90Wh dùng lâu. Thiết kế gaming cá tính, đèn LED nhiều màu.', 32990000, 25, 2, 'https://images.fpt.shop/unsafe/fit-in/800x800/filters:quality(90):fill(white)/fptshop.com.vn/Uploads/Originals/2023/11/13/638355287696837925_asus-rog-strix-g16-g614jv-den-2.jpg', GETUTCDATE()),

(10, N'Lenovo ThinkPad X1 Carbon Gen 11 i7 16GB/512GB', N'Laptop doanh nghiệp cao cấp nhất với trọng lượng siêu nhẹ chỉ 1.12kg, độ mỏng 14.9mm. Màn hình 14 inch 2.8K độ phân giải cao, độ sáng 400 nits. Chip Intel Core i7 thế hệ 13 mạnh mẽ, RAM 16GB LPDDR5 tốc độ cao. SSD 512GB NVMe. Pin 57Wh dùng 16 giờ liên tục, sạc nhanh 80% trong 1 giờ. Đạt chuẩn quân đội MIL-STD-810H bền bỉ. Bàn phím ThinkPad huyền thoại. Bảo mật vân tay và nhận diện khuôn mặt. Bảo hành 3 năm.', 38990000, 15, 2, 'https://images.fpt.shop/unsafe/fit-in/800x800/filters:quality(90):fill(white)/fptshop.com.vn/Uploads/Originals/2023/4/5/638163409084689223_lenovo-thinkpad-x1-carbon-gen-11-den-1.jpg', GETUTCDATE());

SET IDENTITY_INSERT Products OFF;

PRINT 'Seed data thành công!';

