using System.Globalization;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using ShoppeClone.Api.Domain.Entities;

namespace ShoppeClone.Api.Application.Payment
{
    public interface IVnPayService
    {
        string CreatePaymentUrl(Order order, string ipAddress);
        bool ValidateSignature(string queryString);
    }

    public class VnPayService : IVnPayService
    {
        private readonly IConfiguration _configuration;

        public VnPayService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string CreatePaymentUrl(Order order, string ipAddress)
        {
            // 🔥 TẠM THỜI: Dùng mock URL để test flow
            var baseUrl = "https://moira-subjugular-anna.ngrok-free.dev";
            var mockUrl = $"{baseUrl}/api/VNPay/mock-payment?orderId={order.Id}&amount={order.TotalAmount}";

            Console.WriteLine("=== USING MOCK VNPay (NGROK) ===");
            Console.WriteLine($"Mock URL: {mockUrl}");

            return mockUrl;

            // 🔥 COMMENT CODE VNPay THẬT TẠM THỜI:
            /*
            var vnp_Url = _configuration["VNPay:Url"] ?? "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html";
            var vnp_ReturnUrl = _configuration["VNPay:ReturnUrl"] ?? "https://8f0a51ee28a5.ngrok-free.app/api/VNPay/return";
            var vnp_TmnCode = _configuration["VNPay:TmnCode"] ?? "KHR6BD4F";
            var vnp_HashSecret = _configuration["VNPay:HashSecret"] ?? "3ZBR0QKIG4BHRKELEOHBYX7A5IR3DQYW";

            var vnp_Params = new SortedList<string, string>
            {
                ["vnp_Version"] = "2.1.0",
                ["vnp_Command"] = "pay",
                ["vnp_TmnCode"] = vnp_TmnCode,
                ["vnp_Amount"] = ((long)(order.TotalAmount * 100)).ToString(),
                ["vnp_BankCode"] = "",
                ["vnp_CreateDate"] = DateTime.Now.ToString("yyyyMMddHHmmss"),
                ["vnp_CurrCode"] = "VND",
                ["vnp_IpAddr"] = ipAddress,
                ["vnp_Locale"] = "vn",
                ["vnp_OrderInfo"] = $"Thanh toan don hang {order.Id}",
                ["vnp_OrderType"] = "other",
                ["vnp_ReturnUrl"] = vnp_ReturnUrl,
                ["vnp_TxnRef"] = order.Id.ToString(),
                ["vnp_ExpireDate"] = DateTime.Now.AddMinutes(15).ToString("yyyyMMddHHmmss")
            };

            // Loại bỏ các tham số rỗng
            vnp_Params = new SortedList<string, string>(vnp_Params
                .Where(kvp => !string.IsNullOrEmpty(kvp.Value))
                .ToDictionary(k => k.Key, k => k.Value));

            // Tạo query string để hash (KHÔNG encode)
            var queryString = string.Join("&", vnp_Params.Select(kvp => $"{kvp.Key}={kvp.Value}"));

            // Tạo chữ ký theo chuẩn VNPay - QUAN TRỌNG
            var signData = HashSHA512(vnp_HashSecret + queryString);

            // Tạo URL cuối cùng (có encode)
            var paymentUrl = $"{vnp_Url}?{string.Join("&", vnp_Params.Select(kvp => $"{WebUtility.UrlEncode(kvp.Key)}={WebUtility.UrlEncode(kvp.Value)}"))}&vnp_SecureHash={signData}";

            // DEBUG
            Console.WriteLine("=== VNPay Debug ===");
            Console.WriteLine($"TmnCode: {vnp_TmnCode}");
            Console.WriteLine($"QueryString (for hash): {queryString}");
            Console.WriteLine($"SignData (before hash): {vnp_HashSecret + queryString}");
            Console.WriteLine($"SecureHash: {signData}");
            Console.WriteLine($"Final URL: {paymentUrl}");

            return paymentUrl;
            */
        }

        // Sử dụng hàm HashSHA512 chuẩn hơn
        private string HashSHA512(string data)
        {
            using (var sha512 = SHA512.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(data);
                var hashBytes = sha512.ComputeHash(bytes);
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            }
        }

        public bool ValidateSignature(string queryString)
        {
            // 🔥 TẠM THỜI: Luôn return true cho mock service
            Console.WriteLine("=== USING MOCK VALIDATION ===");
            return true;

            /*
            var vnp_HashSecret = _configuration["VNPay:HashSecret"] ?? "3ZBR0QKIG4BHRKELEOHBYX7A5IR3DQYW";

            var queries = queryString.Trim('?').Split('&');
            var paramsMap = new SortedList<string, string>();

            foreach (var query in queries)
            {
                if (!string.IsNullOrEmpty(query) && !query.StartsWith("vnp_SecureHash"))
                {
                    var kv = query.Split('=');
                    if (kv.Length == 2)
                    {
                        paramsMap.Add(kv[0], kv[1]);
                    }
                }
            }

            // Tạo query string để hash (không encode)
            var signData = string.Join("&", paramsMap.Select(kvp => $"{kvp.Key}={kvp.Value}"));
            var vnp_SecureHash = HmacSHA512(vnp_HashSecret, signData);

            var receivedHash = queries.FirstOrDefault(x => x.StartsWith("vnp_SecureHash"))?.Split('=')[1];

            Console.WriteLine($"Validate - SignData: {signData}");
            Console.WriteLine($"Validate - Generated Hash: {vnp_SecureHash}");
            Console.WriteLine($"Validate - Received Hash: {receivedHash}");

            return vnp_SecureHash.Equals(receivedHash, StringComparison.InvariantCultureIgnoreCase);
            */
        }

        private string HmacSHA512(string key, string inputData)
        {
            var hash = new StringBuilder();
            var keyBytes = Encoding.UTF8.GetBytes(key);
            var inputBytes = Encoding.UTF8.GetBytes(inputData);

            using (var hmac = new HMACSHA512(keyBytes))
            {
                var hashValue = hmac.ComputeHash(inputBytes);
                foreach (var theByte in hashValue)
                {
                    hash.Append(theByte.ToString("x2"));
                }
            }
            return hash.ToString();
        }
    }
}