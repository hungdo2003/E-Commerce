using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShoppeClone.Api.Application.Payment;
using ShoppeClone.Api.Domain.Entities;
using ShoppeClone.Api.Infrastructure;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

namespace ShoppeClone.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VNPayController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly IVnPayService _vnPayService;

        public VNPayController(AppDbContext db, IVnPayService vnPayService)
        {
            _db = db;
            _vnPayService = vnPayService;
        }

        // TẠM THỜI: Method không cần user id
        private bool TryGetUserId(out int uid)
        {
            uid = 1; // User id mặc định
            return true;
        }

        [HttpPost("{orderId}/pay-with-vnpay")] // ✅ Đã sửa thành POST
        public async Task<IActionResult> PayWithVnPay(int orderId)
        {
            if (!TryGetUserId(out var uid)) // ✅ Biến 'uid' được khai báo ở đây
                return Unauthorized("Invalid or missing user id claim");

            var order = await _db.Orders
                .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == uid); // ✅ Giờ 'uid' đã tồn tại

            if (order == null)
                return NotFound("Order not found");

            if (order.Status != "Pending")
                return BadRequest("Order already processed");

            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";
            var paymentUrl = _vnPayService.CreatePaymentUrl(order, ipAddress);

            // Thêm log để debug
            Console.WriteLine($"=== VNPay Payment Request ===");
            Console.WriteLine($"OrderId: {orderId}, UserId: {uid}");
            Console.WriteLine($"Order found: {order != null}");
            if (order != null)
            {
                Console.WriteLine($"Order Status: {order.Status}, Amount: {order.TotalAmount}");
            }
            Console.WriteLine($"Payment URL generated: {!string.IsNullOrEmpty(paymentUrl)}");

            return Ok(new { paymentUrl });
        }

        [HttpGet("return")]
        [AllowAnonymous]
        public async Task<IActionResult> VnPayReturn()
        {
            try
            {
                var queryString = HttpContext.Request.QueryString.ToString();

                if (!_vnPayService.ValidateSignature(queryString))
                    return BadRequest("Invalid signature");

                var vnp_ResponseCode = HttpContext.Request.Query["vnp_ResponseCode"].ToString();
                var vnp_TxnRef = HttpContext.Request.Query["vnp_TxnRef"].ToString();

                if (int.TryParse(vnp_TxnRef, out int orderId))
                {
                    var order = await _db.Orders.FindAsync(orderId);
                    if (order != null)
                    {
                        if (vnp_ResponseCode == "00") // Thanh toán thành công
                        {
                            order.Status = "Paid";
                            order.PaymentMethod = "VNPay";
                            order.PaymentTransactionId = HttpContext.Request.Query["vnp_TransactionNo"].ToString();
                            order.PaymentDate = DateTime.UtcNow;
                            order.PaymentNote = $"VNPay - {HttpContext.Request.Query["vnp_BankCode"]}";

                            await _db.SaveChangesAsync();

                            return Redirect($"http://localhost:3000/order-success/{orderId}");
                        }
                        else // Thanh toán thất bại
                        {
                            order.Status = "Failed";
                            order.PaymentMethod = "VNPay";
                            order.PaymentDate = DateTime.UtcNow;
                            order.PaymentNote = $"VNPay Error: {vnp_ResponseCode}";

                            await _db.SaveChangesAsync();

                            return Redirect($"http://localhost:3000/order-failed/{orderId}?error={vnp_ResponseCode}");
                        }
                    }
                }

                return BadRequest("Invalid order");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"VNPay return error: {ex.Message}");
                return BadRequest("Payment processing error");
            }
        }

        [HttpGet("ipn")]
        [AllowAnonymous] // IPN URL cho VNPay
        public async Task<IActionResult> VnPayIPN()
        {
            try
            {
                var queryString = HttpContext.Request.QueryString.ToString();

                if (!_vnPayService.ValidateSignature(queryString))
                    return BadRequest("Invalid signature");

                var vnp_ResponseCode = HttpContext.Request.Query["vnp_ResponseCode"].ToString();
                var vnp_TxnRef = HttpContext.Request.Query["vnp_TxnRef"].ToString();

                if (int.TryParse(vnp_TxnRef, out int orderId))
                {
                    var order = await _db.Orders.FindAsync(orderId);
                    if (order != null && order.Status == "Pending")
                    {
                        if (vnp_ResponseCode == "00")
                        {
                            order.Status = "Paid";
                            order.PaymentMethod = "VNPay";
                            order.PaymentTransactionId = HttpContext.Request.Query["vnp_TransactionNo"].ToString();
                            order.PaymentDate = DateTime.UtcNow;

                            await _db.SaveChangesAsync();
                        }
                        else
                        {
                            order.Status = "Failed";
                            order.PaymentMethod = "VNPay";
                            order.PaymentDate = DateTime.UtcNow;

                            await _db.SaveChangesAsync();
                        }
                    }
                }

                return Ok(new { RspCode = "00", Message = "Confirm Success" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"VNPay IPN error: {ex.Message}");
                return Ok(new { RspCode = "99", Message = "Unknown error" });
            }
        }

        [HttpGet("mock-payment")]
        [AllowAnonymous]
        public async Task<IActionResult> MockPayment(int orderId, decimal amount)
        {
            try
            {
                Console.WriteLine($"=== MOCK PAYMENT STARTED ===");
                Console.WriteLine($"OrderId: {orderId}, Amount: {amount}");

                // Tìm order
                var order = await _db.Orders.FindAsync(orderId);
                if (order == null)
                {
                    Console.WriteLine("Order not found");
                    return NotFound("Order not found");
                }

                Console.WriteLine($"Found order: {order.Id}, Status: {order.Status}");

                // Mock thanh toán thành công
                // Tạo URL callback giống như VNPay thật
                var returnUrl = $"/api/VNPay/return?vnp_ResponseCode=00" +
                               $"&vnp_TxnRef={orderId}" +
                               $"&vnp_TransactionNo=MOCK{DateTime.Now.Ticks}" +
                               $"&vnp_Amount={(long)(amount * 100)}" +
                               $"&vnp_BankCode=NCB" +
                               $"&vnp_PayDate={DateTime.Now:yyyyMMddHHmmss}" +
                               $"&vnp_SecureHash=mock_hash";

                var fullReturnUrl = $"http://localhost:5080{returnUrl}";

                Console.WriteLine($"Redirecting to: {fullReturnUrl}");

                // Redirect đến callback URL (giống VNPay thật)
                return Redirect(fullReturnUrl);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Mock payment error: {ex.Message}");
                return BadRequest($"Mock payment error: {ex.Message}");
            }
        }
    }
}