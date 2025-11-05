using Microsoft.Extensions.Options;
using ShoppeClone.Api.Domain.Entities;
using ShoppeClone.Api.Infrastructure;
using System.Net;

namespace ShoppeClone.Api.Application.Payment
{
    public class MockVNPayService : IVNPayService
    {
        private readonly VNPayConfiguration _config;

        public MockVNPayService(IOptions<VNPayConfiguration> config)
        {
            _config = config.Value;
        }

        public VNPayPaymentResponse CreatePaymentUrl(VNPayPaymentRequest request, string ipAddress)
        {
            try
            {
                var transactionId = DateTime.Now.Ticks.ToString();

                // TẠO URL VỚI ĐẦY ĐỦ PARAMETERS CẦN THIẾT
                var baseUrl = $"{_config.ReturnUrl}?" +
                            $"vnp_Amount={(int)(request.Amount * 100)}" +
                            $"&vnp_BankCode=NCB" +
                            $"&vnp_TransactionNo=TEST{DateTime.Now:yyyyMMddHHmmss}" +
                            $"&vnp_OrderInfo={WebUtility.UrlEncode(request.OrderDescription)}" +
                            $"&vnp_PayDate={DateTime.Now:yyyyMMddHHmmss}" +
                            $"&vnp_ResponseCode=00" +
                            $"&vnp_TmnCode={_config.TmnCode}" +
                            $"&vnp_TxnRef={transactionId}" +
                            $"&vnp_SecureHash=mock_hash";

                return new VNPayPaymentResponse
                {
                    Success = true,
                    PaymentUrl = baseUrl,
                    TransactionId = transactionId,
                    Message = "Mock payment URL created successfully"
                };
            }
            catch (Exception ex)
            {
                return new VNPayPaymentResponse
                {
                    Success = false,
                    Message = $"Mock service error: {ex.Message}"
                };
            }
        }

        public bool ValidateSignature(Dictionary<string, string> queryParams)
        {
            // Trong mock, luôn return true
            return true;
        }

        public PaymentStatus GetPaymentStatus(string responseCode)
        {
            // Mock: luôn thành công cho testing
            return PaymentStatus.Success;
        }
    }
}