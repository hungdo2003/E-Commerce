using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShoppeClone.Api.Migrations
{
    public partial class SeedRealProducts : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Xóa dữ liệu cũ (nếu có)
            migrationBuilder.Sql("DELETE FROM CartItems");
            migrationBuilder.Sql("DELETE FROM OrderItems");
            migrationBuilder.Sql("DELETE FROM Products");
            migrationBuilder.Sql("DELETE FROM Categories");

            // Seed Categories
            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "Name", "CreatedAt" },
                values: new object[,]
                {
                    { 1, "Điện Thoại & Phụ Kiện", DateTime.UtcNow },
                    { 2, "Laptop & Máy Tính", DateTime.UtcNow },
                    { 3, "Thời Trang Nam", DateTime.UtcNow },
                    { 4, "Thời Trang Nữ", DateTime.UtcNow },
                    { 5, "Đồng Hồ", DateTime.UtcNow },
                    { 6, "Giày Dép", DateTime.UtcNow },
                    { 7, "Gia Dụng & Đời Sống", DateTime.UtcNow },
                    { 8, "Sức Khỏe & Làm Đẹp", DateTime.UtcNow }
                });

            // Seed Products - Điện Thoại (Category 1)
            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "Id", "Name", "Description", "Price", "Stock", "CategoryId", "ThumbnailUrl", "CreatedAt" },
                values: new object[,]
                {
                    { 1, "iPhone 15 Pro Max 256GB", "iPhone 15 Pro Max - Thiết kế titan chuẩn Pro, chip A17 Pro mạnh mẽ, camera 48MP, màn hình 6.7 inch Super Retina XDR. Hỗ trợ 5G, sạc nhanh, kháng nước IP68.", 29990000m, 50, 1, "https://cdn.tgdd.vn/Products/Images/42/305658/iphone-15-pro-max-blue-thumbnew-600x600.jpg", DateTime.UtcNow },
                    { 2, "Samsung Galaxy S24 Ultra 12GB/256GB", "Galaxy S24 Ultra - Bút S Pen tích hợp, màn hình Dynamic AMOLED 2X 6.8 inch, camera 200MP, chip Snapdragon 8 Gen 3. Pin 5000mAh, sạc nhanh 45W.", 27990000m, 45, 1, "https://cdn.tgdd.vn/Products/Images/42/307174/samsung-galaxy-s24-ultra-grey-thumbnew-600x600.jpg", DateTime.UtcNow },
                    { 3, "Xiaomi 14 Ultra 16GB/512GB", "Xiaomi 14 Ultra - Camera Leica 50MP, màn hình AMOLED 6.73 inch 120Hz, chip Snapdragon 8 Gen 3. Pin 5000mAh, sạc nhanh 90W, sạc không dây 50W.", 24990000m, 30, 1, "https://cdn.tgdd.vn/Products/Images/42/321785/xiaomi-14-ultra-den-thumb-600x600.jpg", DateTime.UtcNow },
                    { 4, "OPPO Find X7 Ultra 16GB/512GB", "OPPO Find X7 Ultra - Hệ thống camera Hasselblad 50MP, màn hình AMOLED 6.82 inch 120Hz, chip Snapdragon 8 Gen 3. Pin 5000mAh, sạc siêu nhanh 100W.", 23990000m, 35, 1, "https://cdn.tgdd.vn/Products/Images/42/319953/oppo-find-x7-ultra-xanh-thumb-600x600.jpg", DateTime.UtcNow },
                    { 5, "iPhone 14 128GB", "iPhone 14 - Màn hình 6.1 inch Super Retina XDR, chip A15 Bionic, camera kép 12MP. Hỗ trợ 5G, Face ID, kháng nước IP68. Thiết kế sang trọng, hiệu năng ổn định.", 18990000m, 60, 1, "https://cdn.tgdd.vn/Products/Images/42/289700/iphone-14-storage-purple-600x600.jpg", DateTime.UtcNow },
                    { 6, "Samsung Galaxy A55 5G 8GB/128GB", "Galaxy A55 5G - Màn hình Super AMOLED 6.6 inch 120Hz, chip Exynos 1480, camera 50MP. Pin 5000mAh, sạc nhanh 25W. Thiết kế khung kim loại cao cấp.", 9990000m, 100, 1, "https://cdn.tgdd.vn/Products/Images/42/320721/samsung-galaxy-a55-5g-xanh-thumb-600x600.jpg", DateTime.UtcNow },

                    // Laptop (Category 2)
                    { 7, "MacBook Air M2 13 inch 2024 8GB/256GB", "MacBook Air M2 - Thiết kế siêu mỏng 11.3mm, chip M2 mạnh mẽ, màn hình Liquid Retina 13.6 inch. Pin 18 giờ, bàn phím Magic Keyboard, cảm biến Touch ID.", 26990000m, 30, 2, "https://cdn.tgdd.vn/Products/Images/44/282827/apple-macbook-air-m2-2022-xam-1-600x600.jpg", DateTime.UtcNow },
                    { 8, "Dell XPS 13 Plus i7-1360P/16GB/512GB", "Dell XPS 13 Plus - Thiết kế cao cấp, màn hình OLED 13.4 inch 3.5K, chip Intel Core i7 Gen 13. RAM 16GB, SSD 512GB. Bàn phím cảm ứng, cổng Thunderbolt 4.", 35990000m, 20, 2, "https://cdn.tgdd.vn/Products/Images/44/304273/dell-xps-13-plus-9320-i7-evo-1340p-win11-thumb-600x600.jpg", DateTime.UtcNow },
                    { 9, "Asus ROG Strix G16 i7-13650HX/16GB/512GB/RTX4060", "Asus ROG Strix G16 - Gaming laptop mạnh mẽ, màn hình 16 inch FHD 165Hz, chip Intel Core i7, RTX 4060 8GB. RAM 16GB DDR5, SSD 512GB. Tản nhiệt tiên tiến.", 32990000m, 25, 2, "https://cdn.tgdd.vn/Products/Images/44/319548/asus-rog-strix-g16-i7-g614jv-n3135w-thumb-600x600.jpg", DateTime.UtcNow },
                    { 10, "Lenovo ThinkPad X1 Carbon Gen 11 i7/16GB/512GB", "Lenovo ThinkPad X1 Carbon - Laptop doanh nghiệp cao cấp, màn hình 14 inch 2.8K, chip Intel Core i7 Gen 13. Siêu nhẹ 1.12kg, pin 16 giờ, chuẩn quân đội MIL-STD.", 38990000m, 15, 2, "https://cdn.tgdd.vn/Products/Images/44/313103/lenovo-thinkpad-x1-carbon-gen-11-i7-1355u-thumb-600x600.jpg", DateTime.UtcNow },

                    // Thời Trang Nam (Category 3)
                    { 11, "Áo Polo Nam Cotton Premium", "Áo polo nam chất liệu cotton cao cấp, thấm hút mồ hôi tốt, form dáng regular fit. Phù hợp đi làm, đi chơi. Có 5 màu: trắng, đen, xanh navy, xám, be.", 299000m, 200, 3, "https://down-vn.img.susercontent.com/file/vn-11134207-7r98o-lpk5jxvhwnhh3b", DateTime.UtcNow },
                    { 12, "Quần Jeans Nam Slim Fit", "Quần jeans nam form slim fit ôm dáng, chất liệu denim co giãn nhẹ. Thiết kế 5 túi cổ điển, màu xanh đen và xanh nhạt. Phù hợp mọi vóc dáng.", 450000m, 150, 3, "https://down-vn.img.susercontent.com/file/vn-11134207-7r98o-ls5dvxqzk1wv52", DateTime.UtcNow },
                    { 13, "Áo Sơ Mi Nam Công Sở Trắng", "Áo sơ mi nam công sở chất liệu cotton pha, không nhăn, không xù lông. Form regular fit, cổ kent hiện đại. Phù hợp đi làm, đi sự kiện.", 350000m, 180, 3, "https://down-vn.img.susercontent.com/file/vn-11134207-7r98o-lpk5jxvhwnhh3b", DateTime.UtcNow },

                    // Thời Trang Nữ (Category 4)
                    { 14, "Váy Đầm Nữ Hoa Nhí Vintage", "Váy đầm nữ họa tiết hoa nhí vintage, chất liệu voan lụa mềm mại. Thiết kế dáng xòe nhẹ nhàng, phù hợp đi chơi, đi làm. Form chuẩn Việt Nam.", 380000m, 120, 4, "https://down-vn.img.susercontent.com/file/vn-11134207-7r98o-lq7h5xxjxnb54e", DateTime.UtcNow },
                    { 15, "Áo Kiểu Nữ Tay Bồng Công Sở", "Áo kiểu nữ tay bồng sang trọng, chất liệu lụa cao cấp. Thiết kế cổ tim, tay bồng nhẹ nhàng. Phù hợp đi làm, đi dự tiệc. Có 4 màu: trắng, đen, hồng, xanh.", 320000m, 160, 4, "https://down-vn.img.susercontent.com/file/vn-11134207-7r98o-lqg8jzpqm8w5c7", DateTime.UtcNow },
                    { 16, "Quần Culottes Nữ Ống Rộng", "Quần culottes nữ ống rộng thoải mái, chất liệu kaki cao cấp. Thiết kế ống suông, lưng cao tôn dáng. Phù hợp mọi hoàn cảnh. Form chuẩn Việt Nam.", 295000m, 140, 4, "https://down-vn.img.susercontent.com/file/vn-11134207-7r98o-lqfz8apqkn2l0b", DateTime.UtcNow },

                    // Đồng Hồ (Category 5)
                    { 17, "Đồng Hồ Nam Casio MTP-VD01D-1BVUDF", "Đồng hồ nam Casio kim trôi, mặt kính khoáng, dây da cao cấp. Chống nước 5ATM, bảo hành 1 năm. Thiết kế sang trọng, phù hợp công sở.", 1890000m, 50, 5, "https://cdn.tgdd.vn/Products/Images/7264/88768/casio-mtp-vd01d-1bvudf-nam-1-org.jpg", DateTime.UtcNow },
                    { 18, "Apple Watch Series 9 GPS 41mm", "Apple Watch Series 9 - Chip S9 mạnh mẽ, màn hình Retina LTPO OLED 1.7 inch. Theo dõi sức khỏe toàn diện, chống nước 50m, pin 18 giờ. Hỗ trợ watchOS 10.", 10990000m, 40, 5, "https://cdn.tgdd.vn/Products/Images/7077/309081/apple-watch-s9-gps-41mm-vien-nhom-day-silicone-1-1.jpg", DateTime.UtcNow },
                    { 19, "Citizen Eco-Drive BM7108-81L", "Đồng hồ nam Citizen Eco-Drive - Năng lượng ánh sáng vĩnh cửu, dây da cao cấp. Kính Sapphire chống xước, chống nước 10ATM. Bảo hành 5 năm.", 5890000m, 30, 5, "https://cdn.tgdd.vn/Products/Images/7264/236537/citizen-eco-drive-bm7108-81l-nam-1-org.jpg", DateTime.UtcNow },

                    // Giày Dép (Category 6)
                    { 20, "Giày Thể Thao Nam Nike Air Max 270", "Giày thể thao Nike Air Max 270 - Đế Air độc quyền êm ái, upper mesh thoáng khí. Thiết kế hiện đại, phù hợp chạy bộ, tập gym. Nhiều màu sắc.", 3290000m, 80, 6, "https://static.nike.com/a/images/c_limit,w_592,f_auto/t_product_v1/ce42ff0e-b057-42bd-8d2e-6a2ac83c3c34/air-max-270-mens-shoes-KkLcGR.png", DateTime.UtcNow },
                    { 21, "Giày Sneaker Adidas Ultraboost 22", "Adidas Ultraboost 22 - Công nghệ Boost đệm êm tối đa, upper Primeknit thoáng khí. Thiết kế thời trang, phù hợp chạy bộ và hàng ngày.", 4590000m, 60, 6, "https://assets.adidas.com/images/h_840,f_auto,q_auto,fl_lossy,c_fill,g_auto/ac39a6a6aa08477398e4af3c0180c949_9366/Ultraboost_22_Shoes_Black_GZ0127_01_standard.jpg", DateTime.UtcNow },
                    { 22, "Dép Quai Ngang Nam Adidas Adilette", "Dép quai ngang Adidas Adilette - Chất liệu EVA siêu nhẹ, đế mềm êm ái. Logo 3 sọc nổi bật. Phù hợp đi trong nhà, đi biển, đi bể bơi.", 690000m, 200, 6, "https://assets.adidas.com/images/h_840,f_auto,q_auto,fl_lossy,c_fill,g_auto/c21a8d11e96348d09d6cac85011d637f_9366/Adilette_Comfort_Slides_Black_AP9971_01_standard.jpg", DateTime.UtcNow },

                    // Gia Dụng (Category 7)
                    { 23, "Nồi Cơm Điện Tử Sharp 1.8L KS-COM18V", "Nồi cơm điện tử Sharp 1.8L - Công nghệ Fuzzy Logic nấu cơm ngon, lòng nồi chống dính ceramic. 11 chế độ nấu, hẹn giờ 24h. Công suất 860W.", 1890000m, 50, 7, "https://cdn.tgdd.vn/Products/Images/1922/78867/sharp-ks-com18v-sl-1-org.jpg", DateTime.UtcNow },
                    { 24, "Máy Hút Bụi Xiaomi Vacuum G11", "Máy hút bụi cầm tay Xiaomi G11 - Lực hút 150W mạnh mẽ, pin 3000mAh. Màn hình LED, lọc HEPA 5 lớp. Trọng lượng nhẹ 1.4kg, 5 đầu hút đa năng.", 4290000m, 40, 7, "https://cdn.tgdd.vn/Products/Images/8417/270693/xiaomi-g11-1-1.jpg", DateTime.UtcNow },
                    { 25, "Quạt Điều Hòa Kangaroo KG50F88", "Quạt điều hòa Kangaroo 50L - Làm mát nhanh, tiết kiệm điện. Bình chứa nước 50L, điều khiển từ xa. 3 chế độ gió, đảo chiều tự động. Công suất 220W.", 3590000m, 35, 7, "https://cdn.tgdd.vn/Products/Images/1984/306208/quat-dieu-hoa-kangaroo-kg50f88-1-1.jpg", DateTime.UtcNow },

                    // Sức Khỏe & Làm Đẹp (Category 8)
                    { 26, "Kem Chống Nắng Anessa Perfect UV SPF50+ PA++++", "Kem chống nắng Anessa - Công nghệ Aqua Booster chống nước, mồ hôi. SPF50+ PA++++ bảo vệ tối ưu, không gây nhờn rít. Dung tích 60ml.", 650000m, 150, 8, "https://product.hstatic.net/200000551679/product/a12_5b2e44ac2e184f5db6fd7a0c1edd1ad0.jpg", DateTime.UtcNow },
                    { 27, "Serum Vitamin C Some By Mi 30ml", "Serum Vitamin C Some By Mi - Làm sáng da, mờ thâm nám. 75% chiết xuất quả Cam Jeju, Niacinamide làm đều màu da. Dung tích 30ml, phù hợp mọi loại da.", 380000m, 120, 8, "https://product.hstatic.net/200000551679/product/2_2c9cd26d35e547b9a577b3cb3b17ce1f.jpg", DateTime.UtcNow },
                    { 28, "Máy Sấy Tóc Dyson Supersonic HD15", "Máy sấy tóc Dyson Supersonic - Động cơ V9 mạnh mẽ, sấy nhanh không gây hư tổn. 4 đầu từ tính đa năng, 3 chế độ nhiệt. Thiết kế sang trọng, bảo hành 2 năm.", 13990000m, 25, 8, "https://cdn.tgdd.vn/Products/Images/4552/321032/may-say-toc-dyson-supersonic-hd15-1-1.jpg", DateTime.UtcNow }
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Xóa dữ liệu đã seed
            migrationBuilder.Sql("DELETE FROM Products");
            migrationBuilder.Sql("DELETE FROM Categories");
        }
    }
}

