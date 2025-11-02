using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace ShoppeClone.Api.Application.Payment
{
    public static class VnPayHelper
    {
        public static string UrlEncodeRFC3986(string value)
        {
            if (string.IsNullOrEmpty(value)) return string.Empty;

            var enc = HttpUtility.UrlEncode(value, Encoding.UTF8);
            return enc?.Replace("+", "%20").Replace("*", "%2A").Replace("%7E", "~") ?? string.Empty;
        }

        public static string HmacSHA512(string key, string data)
        {
            using var h = new HMACSHA512(Encoding.UTF8.GetBytes(key));
            var hash = h.ComputeHash(Encoding.UTF8.GetBytes(data));
            return BitConverter.ToString(hash).Replace("-", "").ToUpperInvariant();
        }

        public static string BuildSignedUrl(string baseUrl, string tmnCode, string hashSecret,
            string returnUrl, long amountVnd, string ipAddr, string orderRef, string orderInfo,
            string locale = "vn", string currCode = "VND")
        {
            var now = DateTime.UtcNow.AddHours(7).ToString("yyyyMMddHHmmss");

            var raw = new SortedDictionary<string, string>(StringComparer.Ordinal)
            {
                ["vnp_Version"] = "2.1.0",
                ["vnp_Command"] = "pay",
                ["vnp_TmnCode"] = tmnCode,
                ["vnp_Amount"] = (amountVnd * 100).ToString(), // VND x100
                ["vnp_CurrCode"] = currCode,
                ["vnp_TxnRef"] = orderRef,
                ["vnp_OrderInfo"] = orderInfo,
                ["vnp_ReturnUrl"] = returnUrl,
                ["vnp_IpAddr"] = ipAddr,
                ["vnp_CreateDate"] = now,
                ["vnp_Locale"] = locale
            };

            var pairs = raw.Select(kv =>
                $"{UrlEncodeRFC3986(kv.Key)}={UrlEncodeRFC3986(kv.Value)}");
            var signData = string.Join("&", pairs);
            var secureHash = HmacSHA512(hashSecret, signData);
            return $"{baseUrl}?{signData}&vnp_SecureHash={secureHash}";
        }

        public static (bool ok, string calcHash, string signData) VerifyReturn(IDictionary<string, string> query, string hashSecret)
        {
            var dict = new Dictionary<string, string>(query, StringComparer.OrdinalIgnoreCase);
            var received = dict.GetValueOrDefault("vnp_SecureHash") ?? "";
            dict.Remove("vnp_SecureHash");
            dict.Remove("vnp_SecureHashType");

            var sorted = new SortedDictionary<string, string>(dict, StringComparer.Ordinal);
            var pairs = sorted.Select(kv =>
                $"{UrlEncodeRFC3986(kv.Key)}={UrlEncodeRFC3986(kv.Value)}");
            var signData = string.Join("&", pairs);
            var calc = HmacSHA512(hashSecret, signData);
            return (string.Equals(calc, received, StringComparison.OrdinalIgnoreCase), calc, signData);
        }
    }
}