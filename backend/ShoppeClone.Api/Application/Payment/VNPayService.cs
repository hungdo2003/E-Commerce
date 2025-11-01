using System.Net;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace ShoppeClone.Api.Application.Payment
{
    public class VNPayService
    {
        private readonly IConfiguration _configuration;

        public VNPayService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // --- Encode theo RFC3986 (VNPAY mong muốn ' ' => %20, không phải '+')
        private static string Encode(string value)
        {
            var s = WebUtility.UrlEncode(value ?? string.Empty);
            return s?
                .Replace("+", "%20")
                .Replace("*", "%2A")
                .Replace("%7E", "~") ?? string.Empty;
        }

        private static string HmacSHA512(string key, string data)
        {
            using var h = new HMACSHA512(Encoding.UTF8.GetBytes(key));
            var hash = h.ComputeHash(Encoding.UTF8.GetBytes(data));
            return BitConverter.ToString(hash).Replace("-", "").ToUpperInvariant();
        }

        private static string BuildSignedQuery(IDictionary<string, string> parameters, string hashSecret)
        {
            // Sắp xếp A→Z và tạo chuỗi key=val (đã encode) nối bằng &
            var sorted = new SortedDictionary<string, string>(parameters, StringComparer.Ordinal);
            var pairs = sorted.Select(kv => $"{Encode(kv.Key)}={Encode(kv.Value)}");
            var signData = string.Join("&", pairs);

            var secureHash = HmacSHA512(hashSecret, signData);
            return $"{signData}&vnp_SecureHash={secureHash}";
        }

        /// <summary>
        /// Tạo payment URL cho VNPay (gateway web). amount là VND (chưa nhân 100).
        /// </summary>
        public string CreatePaymentUrl(int orderId, decimal amount, string? orderInfo, string ipAddress)
        {
            var baseUrl = _configuration["VNPay:Url"] ?? "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html";
            var tmnCode = _configuration["VNPay:TmnCode"] ?? "DEMOTMN";
            var hashSecret = _configuration["VNPay:HashSecret"] ?? "DEMO_HASH_SECRET";
            var returnUrl = _configuration["VNPay:ReturnUrl"] ?? "https://your-app.com/api/VNPay/return";

            if (orderId <= 0) throw new ArgumentException("orderId không hợp lệ.");
            if (amount <= 0) throw new ArgumentException("amount phải > 0 VND.");

            // Né IPv6 trong sandbox (ví dụ ::1). Nếu có, dùng 127.0.0.1
            if (string.IsNullOrWhiteSpace(ipAddress) || ipAddress.Contains(":"))
                ipAddress = "127.0.0.1";

            // VNPAY tính giờ theo VN (UTC+7)
            var nowVN = DateTime.UtcNow.AddHours(7);
            var createDate = nowVN.ToString("yyyyMMddHHmmss");

            // Làm tròn số tiền về số nguyên VND rồi x100
            long vnd = Convert.ToInt64(Math.Round(amount, 0, MidpointRounding.AwayFromZero));
            if (vnd <= 0) throw new ArgumentException("amount sau khi làm tròn không hợp lệ.");
            string vnpAmount = (vnd * 100).ToString();

            // Mô tả đơn hàng an toàn
            string info = string.IsNullOrWhiteSpace(orderInfo)
                ? $"Thanh toan don #{orderId}"
                : orderInfo.Trim();

            // Tuyệt đối không thêm vnp_SecureHash vào đây
            var raw = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["vnp_Version"] = "2.1.0",
                ["vnp_Command"] = "pay",
                ["vnp_TmnCode"] = tmnCode,
                ["vnp_Amount"] = vnpAmount,          // số nguyên ×100
                ["vnp_CreateDate"] = createDate,         // yyyyMMddHHmmss
                ["vnp_CurrCode"] = "VND",
                ["vnp_IpAddr"] = ipAddress,          // IPv4
                ["vnp_Locale"] = "vn",
                ["vnp_OrderInfo"] = info,
                ["vnp_ReturnUrl"] = returnUrl,          // URL hợp lệ (http/https)
                ["vnp_TxnRef"] = orderId.ToString()  // dùng orderId làm mã tham chiếu
                // Có thể thêm: ["vnp_OrderType"] = "other",
                //              ["vnp_ExpireDate"] = nowVN.AddMinutes(15).ToString("yyyyMMddHHmmss")
            };

            var query = BuildSignedQuery(raw, hashSecret);
            return $"{baseUrl}?{query}";
        }

        /// <summary>
        /// Xác minh chữ ký chiều về (return/ipn). Trả true nếu hợp lệ.
        /// </summary>
        public bool ValidateSignature(Dictionary<string, string> queryParams, string inputHash)
        {
            var hashSecret = _configuration["VNPay:HashSecret"] ?? "DEMO_HASH_SECRET";

            // Bỏ các tham số hash khỏi chuỗi ký
            var copy = new Dictionary<string, string>(queryParams, StringComparer.OrdinalIgnoreCase);
            copy.Remove("vnp_SecureHash");
            copy.Remove("vnp_SecureHashType");

            // Sắp xếp & encode lại giống chiều đi
            var sorted = new SortedDictionary<string, string>(copy, StringComparer.Ordinal);
            var pairs = sorted.Select(kv => $"{Encode(kv.Key)}={Encode(kv.Value)}");
            var signData = string.Join("&", pairs);

            var calcHash = HmacSHA512(hashSecret, signData);
            return calcHash.Equals(inputHash, StringComparison.OrdinalIgnoreCase);
        }
    }
}
