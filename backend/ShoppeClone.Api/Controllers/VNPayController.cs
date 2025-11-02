using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using ShoppeClone.Api.Application.Payment;
using ShoppeClone.Api.Infrastructure;

namespace ShoppeClone.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class VNPayController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly VNPayService _vnPayService;
        private readonly IConfiguration _cfg;

        public VNPayController(AppDbContext db, VNPayService vnPayService, IConfiguration cfg)
        {
            _db = db; _vnPayService = vnPayService; _cfg = cfg;
        }

        private string GetClientIp()
        {
            var ip = Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(ip)) ip = ip.Split(',')[0].Trim();
            if (string.IsNullOrWhiteSpace(ip))
                ip = HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString();
            return string.IsNullOrWhiteSpace(ip) ? "127.0.0.1" : ip;
        }

        // Debug tiện dụng – có thể xoá ở production
        [HttpGet("test")]
        [AllowAnonymous]
        public IActionResult TestVNPay()
        {
            var ip = GetClientIp();
            var url = _vnPayService.CreatePaymentUrl(99999, 100000, "Test thanh toan", ip);
            return Ok(new { success = true, paymentUrl = url, cfg = new { ClientIp = ip, ReturnUrl = _cfg["VNPay:ReturnUrl"] } });
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreatePayment([FromBody] VNPayRequestDto req)
        {
            var order = await _db.Orders.FindAsync(req.OrderId);
            if (order == null) return NotFound(new VNPayResponseDto { Success = false, Message = "Không tìm thấy đơn hàng" });

            var ip = GetClientIp();
            var url = _vnPayService.CreatePaymentUrl(order.Id, order.TotalAmount, req.Description, ip);

            return Ok(new VNPayResponseDto { Success = true, PaymentUrl = url, Message = "Tạo link thanh toán thành công" });
        }

        [HttpGet("return")]
        [AllowAnonymous]
        public async Task<IActionResult> VNPayReturn()
        {
            var q = Request.Query.ToDictionary(x => x.Key, x => x.Value.ToString());
            if (!q.TryGetValue("vnp_SecureHash", out var vnpHash)) return BadRequest("Invalid callback data");
            if (!_vnPayService.ValidateSignature(q, vnpHash)) return BadRequest("Invalid signature");

            var responseCode = q.GetValueOrDefault("vnp_ResponseCode", "");
            var transactionStatus = q.GetValueOrDefault("vnp_TransactionStatus", "");
            var txnRef = q.GetValueOrDefault("vnp_TxnRef", "");
            var transNo = q.GetValueOrDefault("vnp_TransactionNo", "");
            var amountStr = q.GetValueOrDefault("vnp_Amount", "");
            var tmn = q.GetValueOrDefault("vnp_TmnCode", "");

            if (!int.TryParse(txnRef, out var orderId)) return BadRequest("Order not found");
            var order = await _db.Orders.FindAsync(orderId);
            if (order == null) return BadRequest("Order not found");

            var expectedAmount = ((long)Math.Round(order.TotalAmount)) * 100;
            if (tmn != _cfg["VNPay:TmnCode"] || amountStr != expectedAmount.ToString())
                return BadRequest("Invalid amount or TmnCode");

            if (responseCode == "00" && transactionStatus == "00")
            {
                order.Status = "Paid";
                order.PaymentMethod = "VNPay";
                order.PaymentTransactionId = transNo;
                await _db.SaveChangesAsync();
                return Ok(new { Success = true, Message = "Thanh toán thành công", OrderId = orderId, TransactionId = transNo });
            }

            order.Status = "Cancelled";
            await _db.SaveChangesAsync();
            return Ok(new { Success = false, Message = "Thanh toán thất bại", ResponseCode = responseCode });
        }

        [HttpGet("ipn")]
        [AllowAnonymous]
        public async Task<IActionResult> VNPayIPN()
        {
            var q = Request.Query.ToDictionary(x => x.Key, x => x.Value.ToString());
            if (!q.TryGetValue("vnp_SecureHash", out var vnpHash)) return Ok(new { RspCode = "97", Message = "Invalid signature" });
            if (!_vnPayService.ValidateSignature(q, vnpHash)) return Ok(new { RspCode = "97", Message = "Invalid signature" });

            var responseCode = q.GetValueOrDefault("vnp_ResponseCode", "");
            var transactionStatus = q.GetValueOrDefault("vnp_TransactionStatus", "");
            var txnRef = q.GetValueOrDefault("vnp_TxnRef", "");
            var amountStr = q.GetValueOrDefault("vnp_Amount", "");
            var tmn = q.GetValueOrDefault("vnp_TmnCode", "");

            if (!int.TryParse(txnRef, out var orderId)) return Ok(new { RspCode = "02", Message = "Invalid order id" });

            var order = await _db.Orders.FindAsync(orderId);
            if (order == null) return Ok(new { RspCode = "01", Message = "Order not found" });

            var expectedAmount = ((long)Math.Round(order.TotalAmount)) * 100;
            if (tmn != _cfg["VNPay:TmnCode"] || amountStr != expectedAmount.ToString())
                return Ok(new { RspCode = "04", Message = "Invalid amount or TmnCode" });

            if (responseCode == "00" && transactionStatus == "00")
            {
                if (order.Status != "Paid")
                {
                    order.Status = "Paid";
                    await _db.SaveChangesAsync();
                }
                return Ok(new { RspCode = "00", Message = "Confirm Success" });
            }

            order.Status = "Cancelled";
            await _db.SaveChangesAsync();
            return Ok(new { RspCode = "00", Message = "Confirm Success" });
        }
    }

    // DTOs
    public class VNPayRequestDto
    {
        public int OrderId { get; set; }
        public string Description { get; set; } = string.Empty;
    }

    public class VNPayResponseDto
    {
        public bool Success { get; set; }
        public string PaymentUrl { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }
}
