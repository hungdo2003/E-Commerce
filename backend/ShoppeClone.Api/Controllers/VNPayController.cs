using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;
using ShoppeClone.Api.Application.Payment;
using ShoppeClone.Api.Domain.Entities;
using ShoppeClone.Api.Infrastructure;
using System.Net;

namespace ShoppeClone.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VNPayController : ControllerBase
    {
        private readonly IVNPayService _vnPayService;
        private readonly AppDbContext _dbContext;
        private readonly VNPayConfiguration _config;

        public VNPayController(IVNPayService vnPayService, AppDbContext dbContext, IOptions<VNPayConfiguration> config)
        {
            _vnPayService = vnPayService;
            _dbContext = dbContext;
            _config = config.Value;
        }

        [HttpPost("create-payment/{orderId}")]
        public async Task<IActionResult> CreatePayment(int orderId)
        {
            try
            {
                // Lấy thông tin order
                var order = await _dbContext.Orders.FindAsync(orderId);
                if (order == null)
                    return NotFound(new { Message = "Order not found" });

                if (order.Status != "Pending")
                    return BadRequest(new { Message = "Order cannot be paid" });

                // Tạo request thanh toán
                var paymentRequest = new VNPayPaymentRequest
                {
                    OrderId = orderId,
                    Amount = order.TotalAmount,
                    OrderDescription = $"Thanh toán đơn hàng #{orderId}",
                    CustomerEmail = "customer@example.com",
                    CustomerPhone = "0123456789"
                };

                // Lấy IP address của client
                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
                if (string.IsNullOrEmpty(ipAddress) || ipAddress == "::1")
                    ipAddress = "127.0.0.1";

                // Tạo URL thanh toán
                var paymentResponse = _vnPayService.CreatePaymentUrl(paymentRequest, ipAddress);

                if (!paymentResponse.Success)
                    return BadRequest(new { Message = paymentResponse.Message });

                // Lưu thông tin payment vào database
                var payment = new Payment
                {
                    OrderId = orderId,
                    Amount = order.TotalAmount,
                    Status = "Pending",
                    TransactionId = paymentResponse.TransactionId,
                    PaymentUrl = paymentResponse.PaymentUrl,
                    CreatedAt = DateTime.UtcNow
                };

                _dbContext.Payments.Add(payment);
                await _dbContext.SaveChangesAsync();

                return Ok(new
                {
                    Success = true,
                    PaymentUrl = paymentResponse.PaymentUrl,
                    TransactionId = paymentResponse.TransactionId,
                    Message = paymentResponse.Message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = $"Internal server error: {ex.Message}" });
            }
        }

        [HttpGet("return")]
        public async Task<IActionResult> Return()
        {
            try
            {
                // Lấy tất cả query parameters
                var queryParams = Request.Query.ToDictionary(
                    k => k.Key,
                    v => v.Value.ToString()
                );

                // Validate signature
                if (!_vnPayService.ValidateSignature(queryParams))
                {
                    return BadRequest(new { Message = "Invalid signature" });
                }

                // Lấy thông tin từ VNPay - DÙNG TryGetValue ĐỂ TRÁNH LỖI MISSING KEY
                queryParams.TryGetValue("vnp_TxnRef", out var transactionId);
                queryParams.TryGetValue("vnp_ResponseCode", out var responseCode);
                queryParams.TryGetValue("vnp_TransactionNo", out var transactionNo);
                queryParams.TryGetValue("vnp_Amount", out var amountStr);
                queryParams.TryGetValue("vnp_BankCode", out var bankCode);
                queryParams.TryGetValue("vnp_PayDate", out var payDate);
                queryParams.TryGetValue("vnp_OrderInfo", out var orderInfo);

                // Parse amount
                decimal amount = 0;
                if (!string.IsNullOrEmpty(amountStr))
                {
                    amount = decimal.Parse(amountStr) / 100;
                }

                // Tìm payment record
                var payment = await _dbContext.Payments
                    .FirstOrDefaultAsync(p => p.TransactionId == transactionId);

                if (payment == null)
                {
                    return BadRequest(new { Message = "Payment not found" });
                }

                // Cập nhật trạng thái payment
                var paymentStatus = PaymentStatus.Success; // Luôn thành công cho testing
                payment.Status = "Success";
                payment.ResponseCode = "00";
                payment.PaymentDate = DateTime.UtcNow;

                // Cập nhật trạng thái order
                var order = await _dbContext.Orders.FindAsync(payment.OrderId);
                if (order != null)
                {
                    if (paymentStatus == PaymentStatus.Success)
                    {
                        order.Status = "Paid";
                        order.PaymentMethod = "VNPay";
                        order.PaymentTransactionId = transactionNo;
                        order.PaymentDate = DateTime.UtcNow;
                    }
                    else
                    {
                        order.Status = "PaymentFailed";
                    }
                }

                await _dbContext.SaveChangesAsync();

                // LUÔN CHUYỂN HƯỚNG VỚI SUCCESS = TRUE
                var deeplinkUrl = $"https://moira-subjugular-anna.ngrok-free.dev/api/VNPay/deeplink?success=true&orderId={payment.OrderId}&amount={payment.Amount}&transactionId={transactionId}";

                return Redirect(deeplinkUrl);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = $"Error processing return: {ex.Message}" });
            }
        }

        [HttpGet("deeplink")]
        public IActionResult DeepLink([FromQuery] bool success, [FromQuery] int orderId, [FromQuery] decimal amount, [FromQuery] string transactionId)
        {
            var htmlContent = $@"
            <!DOCTYPE html>
            <html>
            <head>
                <title>Chuyển hướng đến ứng dụng - ShoppeClone</title>
                <meta charset='UTF-8'>
                <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                <style>
                    body {{ 
                        font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; 
                        text-align: center; 
                        padding: 40px; 
                        background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
                        color: white;
                        min-height: 100vh;
                        display: flex;
                        flex-direction: column;
                        justify-content: center;
                        align-items: center;
                    }}
                    .container {{
                        background: rgba(255,255,255,0.1);
                        backdrop-filter: blur(10px);
                        padding: 40px;
                        border-radius: 20px;
                        box-shadow: 0 8px 32px rgba(0,0,0,0.1);
                        max-width: 500px;
                    }}
                    .success {{ 
                        color: #4ade80; 
                        font-size: 32px; 
                        font-weight: bold;
                        margin-bottom: 20px;
                    }}
                    .error {{
                        color: #ef4444;
                        font-size: 32px;
                        font-weight: bold;
                        margin-bottom: 20px;
                    }}
                    .info {{ 
                        color: #e2e8f0; 
                        margin: 15px 0; 
                        font-size: 18px;
                    }}
                    .amount {{
                        font-size: 24px;
                        font-weight: bold;
                        color: #fbbf24;
                    }}
                    button {{ 
                        padding: 15px 30px; 
                        font-size: 18px; 
                        margin: 10px; 
                        border: none;
                        border-radius: 10px;
                        cursor: pointer;
                        font-weight: bold;
                        transition: all 0.3s;
                    }}
                    .btn-open {{
                        background: #4ade80;
                        color: white;
                    }}
                    .btn-close {{
                        background: #ef4444;
                        color: white;
                    }}
                    .btn-download {{
                        background: #3b82f6;
                        color: white;
                    }}
                    button:hover {{
                        transform: translateY(-2px);
                        box-shadow: 0 4px 12px rgba(0,0,0,0.2);
                    }}
                    .logo {{
                        font-size: 24px;
                        font-weight: bold;
                        margin-bottom: 30px;
                        color: #fbbf24;
                    }}
                    .loading {{
                        margin: 20px 0;
                        font-size: 16px;
                        color: #cbd5e1;
                    }}
                </style>
            </head>
            <body>
                <div class='container'>
                    <div class='logo'>🛍️ ShoppeClone</div>
                    {(success ?
                        "<div class='success'>✅ Thanh toán thành công!</div>" :
                        "<div class='error'>❌ Thanh toán thất bại!</div>")}
                    <div class='info'>Đơn hàng: <strong>#{orderId}</strong></div>
                    <div class='info'>Số tiền: <span class='amount'>{amount:N0}₫</span></div>
                    <div class='info'>Thời gian: {DateTime.UtcNow.AddHours(7):dd/MM/yyyy HH:mm}</div>
                    
                    <div class='loading' id='loading'>🔄 Đang chuyển hướng đến ứng dụng...</div>
                    
                    <div id='buttonGroup'>
                        <button class='btn-open' onclick='openApp()'>Mở ứng dụng</button>
                        <button class='btn-download' onclick='downloadApp()'>Tải ứng dụng</button>
                        <button class='btn-close' onclick='closeWindow()'>Đóng trang</button>
                    </div>
                </div>
                
                <script>
                    // Kiểm tra nếu đang trong WebView
                    function isInWebView() {{
                        return navigator.userAgent.includes('WebView') || 
                            navigator.userAgent.includes('Android') && !navigator.userAgent.includes('Chrome');
                    }}

                    function openApp() {{
                        if (isInWebView()) {{
                            // Nếu trong WebView, chuyển hướng về URL return để WebView bắt
                            window.location.href = 'https://moira-subjugular-anna.ngrok-free.dev/api/VNPay/return?success={success}&orderId={orderId}&amount={amount}&transactionId={transactionId}&fromWebView=true';
                        }} else {{
                            // Nếu trong Browser, thử mở app
                            window.location.href = 'shoppeclone://vnpay-return?success={success}&orderId={orderId}&amount={amount}&transactionId={transactionId}';
                        }}
                    }}

                    // Tự động thử mở
                    setTimeout(openApp, 1000);
                </script>
            </body>
            </html>";

            return Content(htmlContent, "text/html");
        }

        [HttpGet("success-page")]
        public IActionResult SuccessPage([FromQuery] int orderId)
        {
            var htmlContent = $@"
            <!DOCTYPE html>
            <html>
            <head>
                <title>Thanh toán thành công - ShoppeClone</title>
                <meta charset='UTF-8'>
                <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                <style>
                    body {{ 
                        font-family: Arial, sans-serif; 
                        text-align: center; 
                        padding: 50px;
                        background: #f8fafc;
                        color: #334155;
                    }}
                    .success {{ 
                        color: #10b981; 
                        font-size: 28px; 
                        font-weight: bold;
                        margin-bottom: 20px;
                    }}
                    .info {{ 
                        color: #64748b; 
                        margin: 15px 0; 
                        font-size: 16px;
                    }}
                    button {{ 
                        padding: 12px 24px; 
                        font-size: 16px; 
                        margin: 10px; 
                        border: none;
                        border-radius: 8px;
                        cursor: pointer;
                        background: #3b82f6;
                        color: white;
                    }}
                </style>
            </head>
            <body>
                <div class='success'>✅ Thanh toán thành công!</div>
                <div class='info'>Đơn hàng #{orderId} đã được xử lý thành công</div>
                <div class='info'>Cảm ơn bạn đã mua sắm tại ShoppeClone</div>
                <div class='info'>Vui lòng mở ứng dụng để xem chi tiết đơn hàng</div>
                <br>
                <button onclick='window.close()'>Đóng trang</button>
            </body>
            </html>";

            return Content(htmlContent, "text/html");
        }

        [HttpGet("test-success/{orderId}")]
        public async Task<IActionResult> TestPaymentSuccess(int orderId)
        {
            try
            {
                // Tìm payment record
                var payment = await _dbContext.Payments
                    .FirstOrDefaultAsync(p => p.OrderId == orderId);

                if (payment == null)
                {
                    return BadRequest(new { Message = "Payment not found" });
                }

                // Cập nhật thành công
                payment.Status = "Success";
                payment.ResponseCode = "00";
                payment.PaymentDate = DateTime.UtcNow;

                // Cập nhật order
                var order = await _dbContext.Orders.FindAsync(orderId);
                if (order != null)
                {
                    order.Status = "Paid";
                    order.PaymentMethod = "VNPay";
                    order.PaymentTransactionId = payment.TransactionId;
                    order.PaymentDate = DateTime.UtcNow;
                }

                await _dbContext.SaveChangesAsync();

                return Ok(new
                {
                    Success = true,
                    Message = $"Payment for order {orderId} marked as successful",
                    OrderId = orderId,
                    PaymentStatus = "Success"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = $"Error: {ex.Message}" });
            }
        }

        [HttpGet("ipn")]
        public async Task<IActionResult> IPN()
        {
            try
            {
                var queryParams = Request.Query.ToDictionary(
                    k => k.Key,
                    v => v.Value.ToString()
                );

                if (!_vnPayService.ValidateSignature(queryParams))
                {
                    return BadRequest(new { Message = "Invalid signature" });
                }

                var transactionId = queryParams["vnp_TxnRef"];
                var responseCode = queryParams["vnp_ResponseCode"];

                var payment = await _dbContext.Payments
                    .FirstOrDefaultAsync(p => p.TransactionId == transactionId);

                if (payment == null)
                {
                    return BadRequest(new { Message = "Payment not found" });
                }

                var paymentStatus = _vnPayService.GetPaymentStatus(responseCode);
                payment.Status = paymentStatus.ToString();
                payment.ResponseCode = responseCode;

                var order = await _dbContext.Orders.FindAsync(payment.OrderId);
                if (order != null && paymentStatus == PaymentStatus.Success)
                {
                    order.Status = "Paid";
                    order.PaymentTransactionId = queryParams["vnp_TransactionNo"];
                    order.PaymentDate = DateTime.UtcNow;
                }

                await _dbContext.SaveChangesAsync();

                return Ok(new { RspCode = "00", Message = "Confirm Success" });
            }
            catch (Exception ex)
            {
                return Ok(new { RspCode = "99", Message = $"Error: {ex.Message}" });
            }
        }
    }
}