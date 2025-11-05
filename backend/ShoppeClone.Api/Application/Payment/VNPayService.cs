using Microsoft.Extensions.Options;
using System.Globalization;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using ShoppeClone.Api.Domain.Entities;
using ShoppeClone.Api.Infrastructure;

namespace ShoppeClone.Api.Application.Payment
{
    public interface IVNPayService
    {
        VNPayPaymentResponse CreatePaymentUrl(VNPayPaymentRequest request, string ipAddress);
        bool ValidateSignature(Dictionary<string, string> queryParams);
        PaymentStatus GetPaymentStatus(string responseCode);
    }

    public class VNPayService : IVNPayService
    {
        private readonly VNPayConfiguration _config;

        public VNPayService(IOptions<VNPayConfiguration> config)
        {
            _config = config.Value;
        }

        public VNPayPaymentResponse CreatePaymentUrl(VNPayPaymentRequest request, string ipAddress)
        {
            try
            {
                // Tạo transaction ID
                string transactionId = DateTime.Now.Ticks.ToString();

                // Tạo các tham số bắt buộc
                var vnpParams = new SortedList<string, string>
                {
                    ["vnp_Version"] = _config.Version,
                    ["vnp_Command"] = _config.Command,
                    ["vnp_TmnCode"] = _config.TmnCode,
                    ["vnp_Amount"] = ((int)(request.Amount * 100)).ToString(),
                    ["vnp_CreateDate"] = DateTime.Now.ToString("yyyyMMddHHmmss"),
                    ["vnp_CurrCode"] = _config.CurrCode,
                    ["vnp_IpAddr"] = ipAddress,
                    ["vnp_Locale"] = _config.Locale,
                    ["vnp_OrderInfo"] = WebUtility.UrlEncode(request.OrderDescription),
                    ["vnp_OrderType"] = "other",
                    ["vnp_ReturnUrl"] = _config.ReturnUrl,
                    ["vnp_TxnRef"] = transactionId
                };

                // Tạo URL
                var queryString = string.Join("&", vnpParams.Select(kvp =>
                    $"{WebUtility.UrlEncode(kvp.Key)}={WebUtility.UrlEncode(kvp.Value)}"));

                // Tạo secure hash
                var signData = $"{queryString}";
                var secureHash = CreateSHA256Hash(_config.HashSecret + signData);

                var paymentUrl = $"{_config.Url}?{queryString}&vnp_SecureHashType=SHA256&vnp_SecureHash={secureHash}";

                return new VNPayPaymentResponse
                {
                    Success = true,
                    PaymentUrl = paymentUrl,
                    TransactionId = transactionId,
                    Message = "Tạo URL thanh toán thành công"
                };
            }
            catch (Exception ex)
            {
                return new VNPayPaymentResponse
                {
                    Success = false,
                    Message = $"Lỗi tạo URL thanh toán: {ex.Message}"
                };
            }
        }

        public bool ValidateSignature(Dictionary<string, string> queryParams)
        {
            try
            {
                // Lấy secure hash từ query params
                if (!queryParams.TryGetValue("vnp_SecureHash", out var receivedHash))
                    return false;

                // Tạo lại secure hash để so sánh
                var vnpParams = queryParams
                    .Where(kvp => !kvp.Key.Equals("vnp_SecureHash", StringComparison.OrdinalIgnoreCase) &&
                                 !kvp.Key.Equals("vnp_SecureHashType", StringComparison.OrdinalIgnoreCase))
                    .OrderBy(kvp => kvp.Key)
                    .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

                var signData = string.Join("&", vnpParams.Select(kvp =>
                    $"{WebUtility.UrlEncode(kvp.Key)}={WebUtility.UrlEncode(kvp.Value)}"));

                var calculatedHash = CreateSHA256Hash(_config.HashSecret + signData);

                return receivedHash.Equals(calculatedHash, StringComparison.OrdinalIgnoreCase);
            }
            catch
            {
                return false;
            }
        }

        public PaymentStatus GetPaymentStatus(string responseCode)
        {
            return responseCode switch
            {
                "00" => PaymentStatus.Success,
                "07" => PaymentStatus.Failed, // Trừ tiền thành công nhưng GD bị nghi ngờ
                "09" => PaymentStatus.Failed, // GD không thành công do: Thẻ/Tài khoản của khách hàng chưa đăng ký dịch vụ InternetBanking
                "10" => PaymentStatus.Failed, // GD không thành công do: Khách hàng xác thực thông tin thẻ/tài khoản không đúng quá 3 lần
                "11" => PaymentStatus.Failed, // GD không thành công do: Đã hết hạn chờ thanh toán. Xin quý khách vui lòng thực hiện lại GD.
                "12" => PaymentStatus.Failed, // GD không thành công do: Thẻ/Tài khoản của khách hàng bị khóa.
                "13" => PaymentStatus.Failed, // GD không thành công do: Quý khách nhập sai mật khẩu xác thực giao dịch (OTP).
                "24" => PaymentStatus.Failed, // GD không thành công do: Khách hàng hủy GD
                "51" => PaymentStatus.Failed, // GD không thành công do: Tài khoản của quý khách không đủ số dư để thực hiện GD.
                "65" => PaymentStatus.Failed, // GD không thành công do: Tài khoản của Quý khách đã vượt quá hạn mức giao dịch trong ngày.
                "75" => PaymentStatus.Failed, // Ngân hàng thanh toán đang bảo trì.
                "79" => PaymentStatus.Failed, // KH nhập sai mật khẩu thanh toán quá số lần quy định.
                "99" => PaymentStatus.Failed, // Các lỗi khác
                _ => PaymentStatus.Failed
            };
        }

        private string CreateSHA256Hash(string input)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(input);
            var hash = sha256.ComputeHash(bytes);
            return BitConverter.ToString(hash).Replace("-", "").ToLower();
        }
    }

    public enum PaymentStatus
    {
        Pending,
        Success,
        Failed
    }
}