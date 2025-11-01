# 🔧 Hướng Dẫn Sửa Lỗi VNPay

## ❌ Vấn Đề: Lỗi Chữ Ký và Không Mở Được Link Thanh Toán

### Nguyên nhân chính:
**ReturnUrl đang dùng `localhost`** - VNPay không thể redirect về localhost từ mobile!

---

## ✅ Giải Pháp

### **Option 1: Dùng ngrok (Khuyến nghị cho development)**

#### Bước 1: Cài đặt ngrok
```bash
# Download từ: https://ngrok.com/download
# Hoặc dùng chocolatey (Windows):
choco install ngrok
```

#### Bước 2: Chạy backend
```bash
cd backend/ShoppeClone.Api
dotnet run
# Backend chạy ở https://localhost:5001
```

#### Bước 3: Expose backend qua ngrok
```bash
# Mở terminal mới
ngrok http 5001
```

Bạn sẽ nhận được URL public kiểu: `https://abc123.ngrok.io`

#### Bước 4: Cập nhật appsettings.json
```json
"VNPay": {
    "Url": "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html",
    "TmnCode": "KHR6BD4F",
    "HashSecret": "3ZBR0QKIG4BHRKELEOHBYX7A5IR3DQYW",
    "ReturnUrl": "https://abc123.ngrok.io/api/VNPay/return"  // ← Thay đổi này
}
```

#### Bước 5: Cập nhật BaseUrl trong Android app
Trong `android/ShoppeCloneApp/app/src/main/java/com/example/shoppeclone/net/ApiClient.java`:
```java
private static final String BASE_URL = "https://abc123.ngrok.io/";  // Dùng ngrok URL
```

---

### **Option 2: Dùng IP Local (Nếu mobile cùng WiFi)**

#### Bước 1: Lấy IP máy tính
```bash
# Windows
ipconfig
# Tìm "IPv4 Address" (ví dụ: 192.168.1.100)

# Mac/Linux
ifconfig
```

#### Bước 2: Cho phép backend lắng nghe mọi IP
Trong `backend/ShoppeClone.Api/Program.cs`:
```csharp
builder.WebHost.UseUrls("https://0.0.0.0:5001", "http://0.0.0.0:5000");
```

#### Bước 3: Cập nhật appsettings.json
```json
"VNPay": {
    "ReturnUrl": "https://192.168.1.100:5001/api/VNPay/return"  // Dùng IP máy bạn
}
```

#### Bước 4: Cập nhật Android app
```java
private static final String BASE_URL = "https://192.168.1.100:5001/";
```

⚠️ **Lưu ý**: Option này có thể gặp lỗi SSL certificate.

---

### **Option 3: Deploy lên Server Public (Production)**

Deploy backend lên:
- Azure App Service
- Heroku
- Railway
- VPS (DigitalOcean, AWS EC2, etc.)

Sau đó cập nhật ReturnUrl với domain thật của bạn.

---

## 🧪 Testing

### 1. Test endpoint mới (không cần auth):
```bash
# Test từ browser hoặc Postman
GET https://your-url/api/VNPay/test
```

Response sẽ trả về:
```json
{
  "success": true,
  "paymentUrl": "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html?...",
  "message": "URL test được tạo thành công",
  "note": "Kiểm tra URL này trên VNPay sandbox"
}
```

### 2. Copy `paymentUrl` và mở trong browser
- Nếu mở được trang VNPay → Chữ ký đúng! ✅
- Nếu báo lỗi "Invalid signature" → Kiểm tra HashSecret ❌

### 3. Test thanh toán VNPay sandbox:
Thông tin thẻ test của VNPay:
- **Số thẻ**: 9704198526191432198
- **Tên chủ thẻ**: NGUYEN VAN A
- **Ngày phát hành**: 07/15
- **Mật khẩu OTP**: 123456

---

## 🔍 Debug Checklist

- [ ] ReturnUrl KHÔNG phải localhost
- [ ] ReturnUrl là URL public hoặc IP accessible từ mobile
- [ ] Backend đang chạy và accessible từ mobile
- [ ] TmnCode và HashSecret đúng (từ VNPay sandbox)
- [ ] Android app đang dùng đúng BASE_URL
- [ ] Đã test endpoint `/api/VNPay/test` và nhận được URL hợp lệ

---

## 📞 Cấu Hình VNPay Sandbox Hiện Tại

```json
"TmnCode": "KHR6BD4F",
"HashSecret": "3ZBR0QKIG4BHRKELEOHBYX7A5IR3DQYW"
```

Đây là config của bạn - nếu không work, có thể bạn cần đăng ký lại trên VNPay sandbox:
👉 https://sandbox.vnpayment.vn/

---

## 🎯 Tóm Tắt

**Vấn đề chính**: Mobile app không thể kết nối tới `localhost` của máy dev

**Giải pháp nhanh nhất**: 
1. Cài ngrok
2. Chạy `ngrok http 5001`
3. Thay ReturnUrl trong appsettings.json
4. Cập nhật BASE_URL trong Android app
5. Test lại!

---

## ✅ Sau Khi Sửa

1. Restart backend
2. Rebuild Android app
3. Test endpoint: `GET /api/VNPay/test`
4. Thử thanh toán từ app

Good luck! 🚀

