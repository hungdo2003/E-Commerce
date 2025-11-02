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

        [HttpPost("{orderId}/pay-with-vnpay")]
        public async Task<IActionResult> PayWithVnPay(int orderId)
        {
            // TẠM THỜI: Bỏ qua user check hoặc dùng user id cứng
            var uid = 1;

            var order = await _db.Orders
                .FirstOrDefaultAsync(o => o.Id == orderId); // Bỏ điều kiện && o.UserId == uid

            if (order == null)
                return NotFound("Order not found");

            if (order.Status != "Pending")
                return BadRequest("Order already processed");

            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";
            var paymentUrl = _vnPayService.CreatePaymentUrl(order, ipAddress);

            return Ok(new { paymentUrl });
        }

        [HttpGet("return")]
        [AllowAnonymous] // Cho phép VNPay gọi callback mà không cần auth
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

                            await _db.SaveChangesAsync();

                            // Redirect về frontend thành công
                            return Redirect($"http://localhost:3000/order-success/{orderId}");
                        }
                        else // Thanh toán thất bại
                        {
                            order.Status = "Failed";
                            order.PaymentMethod = "VNPay";
                            order.PaymentDate = DateTime.UtcNow;

                            await _db.SaveChangesAsync();

                            // Redirect về frontend thất bại
                            return Redirect($"http://localhost:3000/order-failed/{orderId}?error={vnp_ResponseCode}");
                        }
                    }
                }

                return BadRequest("Invalid order");
            }
            catch (Exception ex)
            {
                // Log lỗi
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
    }
}