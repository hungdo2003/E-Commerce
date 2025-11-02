using System.Net;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ShoppeClone.Api.Application.Payment
{
    public class VNPayService
    {
        private readonly IConfiguration _cfg;
        private readonly ILogger<VNPayService> _log;

        // ======= CHỌN KIỂU KÝ  =======
        // 1: Encode VALUE only (space -> '+')  [nhiều sandbox dùng kiểu này]
        // 2: Encode KEY và VALUE               [một số cổng yêu cầu]
        // 3: Encode VALUE only, RIÊNG vnp_ReturnUrl KHÔNG encode  [ít gặp nhưng có]
        private const int SIGN_MODE = 2;

        public VNPayService(IConfiguration cfg, ILogger<VNPayService> log)
        {
            _cfg = cfg; _log = log;
        }

        private static string UrlEnc(string? s) => WebUtility.UrlEncode(s ?? string.Empty);

        private static string HmacSHA512(string key, string data)
        {
            using var h = new HMACSHA512(Encoding.UTF8.GetBytes((key ?? "").Trim()));
            var hash = h.ComputeHash(Encoding.UTF8.GetBytes(data));
            return BitConverter.ToString(hash).Replace("-", "").ToUpperInvariant();
        }

        // Xây signData theo 3 biến thể thường gặp
        private static string BuildSignData(
            IDictionary<string, string> parameters,
            int signMode
        )
        {
            var sorted = new SortedDictionary<string, string>(parameters, StringComparer.Ordinal);
            var parts = new List<string>();

            foreach (var kv in sorted)
            {
                string key = kv.Key;
                string val = kv.Value ?? string.Empty;

                switch (signMode)
                {
                    case 1: // encode VALUE only
                        parts.Add($"{key}={UrlEnc(val)}");
                        break;

                    case 2: // encode KEY + VALUE
                        parts.Add($"{UrlEnc(key)}={UrlEnc(val)}");
                        break;

                    case 3: // encode VALUE only, riêng vnp_ReturnUrl không encode
                        if (string.Equals(key, "vnp_ReturnUrl", StringComparison.Ordinal))
                            parts.Add($"{key}={val}");
                        else
                            parts.Add($"{key}={UrlEnc(val)}");
                        break;

                    default:
                        // fallback an toàn = mode 1
                        parts.Add($"{key}={UrlEnc(val)}");
                        break;
                }
            }

            return string.Join("&", parts);
        }

        /// <summary>Tạo URL thanh toán. amount = VND (chưa nhân 100).</summary>
        public string CreatePaymentUrl(int orderId, decimal amount, string? orderInfo, string clientIp)
        {
            var baseUrl = (_cfg["VNPay:Url"] ?? "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html").Trim();
            var tmnCode = (_cfg["VNPay:TmnCode"] ?? throw new InvalidOperationException("Missing VNPay:TmnCode")).Trim();
            var hashSecret = (_cfg["VNPay:HashSecret"] ?? throw new InvalidOperationException("Missing VNPay:HashSecret")).Trim();
            var returnUrl = (_cfg["VNPay:ReturnUrl"] ?? throw new InvalidOperationException("Missing VNPay:ReturnUrl")).Trim();

            if (orderId <= 0) throw new ArgumentException("orderId không hợp lệ.");
            if (amount <= 0) throw new ArgumentException("amount phải > 0 VND.");

            // IPv4
            if (string.IsNullOrWhiteSpace(clientIp) || clientIp.Contains(":"))
                clientIp = "127.0.0.1";

            var nowVN = DateTime.UtcNow.AddHours(7);
            var createDate = nowVN.ToString("yyyyMMddHHmmss");
            var expireDate = nowVN.AddMinutes(15).ToString("yyyyMMddHHmmss");

            long vnd = Convert.ToInt64(Math.Round(amount, 0, MidpointRounding.AwayFromZero));
            var vnpAmount = (vnd * 100).ToString();

            var info = string.IsNullOrWhiteSpace(orderInfo) ? $"Thanh toan don #{orderId}" : orderInfo.Trim();

            // THAM SỐ GỐC (chưa thêm SecureHash/Type)
            var raw = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["vnp_Version"] = "2.1.0",
                ["vnp_Command"] = "pay",
                ["vnp_TmnCode"] = tmnCode,
                ["vnp_Amount"] = vnpAmount,
                ["vnp_CreateDate"] = createDate,
                ["vnp_ExpireDate"] = expireDate,
                ["vnp_CurrCode"] = "VND",
                ["vnp_IpAddr"] = clientIp,
                ["vnp_Locale"] = "vn",
                ["vnp_OrderInfo"] = info,
                ["vnp_OrderType"] = "other",
                ["vnp_ReturnUrl"] = returnUrl,
                ["vnp_TxnRef"] = orderId.ToString()
            };

            // 1) signData theo mode đã chọn
            var signData = BuildSignData(raw, SIGN_MODE);
            var secureHash = HmacSHA512(hashSecret, signData);

            // 2) URL cuối cùng = signData + hash type + hash
            var url = $"{baseUrl}?{signData}&vnp_SecureHashType=HmacSHA512&vnp_SecureHash={secureHash}";

            _log.LogInformation("VNPay SIGN_MODE: {Mode}", SIGN_MODE);
            _log.LogInformation("VNPay signData : {SignData}", signData);
            _log.LogInformation("VNPay hash     : {Hash}", secureHash);
            _log.LogInformation("VNPay URL      : {Url}", url);

            return url;
        }

        public bool ValidateSignature(Dictionary<string, string> queryParams, string inputHash)
        {
            var secret = (_cfg["VNPay:HashSecret"] ?? "").Trim();

            // BỎ 2 tham số hash
            var copy = new Dictionary<string, string>(queryParams, StringComparer.OrdinalIgnoreCase);
            copy.Remove("vnp_SecureHash");
            copy.Remove("vnp_SecureHashType");

            var signData = BuildSignData(copy, SIGN_MODE);
            var calcHash = HmacSHA512(secret, signData);

            return calcHash.Equals(inputHash, StringComparison.OrdinalIgnoreCase);
        }
    }
}
