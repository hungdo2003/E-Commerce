using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

        public VNPayController(AppDbContext db, VNPayService vnPayService)
        {
            _db = db;
            _vnPayService = vnPayService;
        }

        /// <summary>
        /// Test endpoint để debug VNPay URL (không cần auth)
        /// </summary>
        [HttpGet("test")]
        [AllowAnonymous]
        public IActionResult TestVNPay()
        {
            try
            {
                var ipAddress = HttpContext.Connection.RemoteIpAddress?
                     .MapToIPv4().ToString() ?? "127.0.0.1";

                string paymentUrl = _vnPayService.CreatePaymentUrl(
                    orderId: 99999,
                    amount: 100000,
                    orderInfo: "Test thanh toan",
                    ipAddress: ipAddress
                );

                return Ok(new
                {
                    Success = true,
                    PaymentUrl = paymentUrl,
                    Message = "URL test được tạo thành công. Kiểm tra URL này trên VNPay sandbox.",
                    Note = "Nếu không mở được, kiểm tra ReturnUrl trong appsettings.json phải là URL public (không phải localhost)"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Success = false, Message = $"Lỗi: {ex.Message}" });
            }
        }

        /// <summary>
        /// Tạo payment URL VNPay
        /// </summary>
        [HttpPost("create")]
        public async Task<IActionResult> CreatePayment([FromBody] VNPayRequestDto request)
        {
            try
            {
                // Lấy thông tin order
                var order = await _db.Orders.FindAsync(request.OrderId);
                if (order == null)
                {
                    return NotFound(new VNPayResponseDto
                    {
                        Success = false,
                        Message = "Không tìm thấy đơn hàng"
                    });
                }

                // Lấy IP address của client
                var ipAddress = HttpContext.Connection.RemoteIpAddress?
                     .MapToIPv4().ToString() ?? "127.0.0.1";



                // Tạo payment URL
                string paymentUrl = _vnPayService.CreatePaymentUrl(
                    orderId: order.Id,
                    amount: order.TotalAmount,
                    orderInfo: request.Description,
                    ipAddress: ipAddress
                );

                return Ok(new VNPayResponseDto
                {
                    Success = true,
                    PaymentUrl = paymentUrl,
                    Message = "Tạo link thanh toán thành công"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new VNPayResponseDto
                {
                    Success = false,
                    Message = $"Lỗi: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Callback từ VNPay sau khi thanh toán
        /// </summary>
        [HttpGet("return")]
        [AllowAnonymous]
        public async Task<IActionResult> VNPayReturn()
        {
            try
            {
                // Lấy tất cả query parameters
                var queryParams = Request.Query.ToDictionary(x => x.Key, x => x.Value.ToString());
                
                if (!queryParams.ContainsKey("vnp_SecureHash"))
                {
                    return BadRequest("Invalid callback data");
                }

                string vnpSecureHash = queryParams["vnp_SecureHash"];

                // Validate signature
                bool isValidSignature = _vnPayService.ValidateSignature(queryParams, vnpSecureHash);
                if (!isValidSignature)
                {
                    return BadRequest("Invalid signature");
                }

                // Lấy thông tin giao dịch
                string responseCode = queryParams.GetValueOrDefault("vnp_ResponseCode", "");
                string transactionStatus = queryParams.GetValueOrDefault("vnp_TransactionStatus", "");
                string txnRef = queryParams.GetValueOrDefault("vnp_TxnRef", "");
                string transactionNo = queryParams.GetValueOrDefault("vnp_TransactionNo", "");

                if (int.TryParse(txnRef, out int orderId))
                {
                    var order = await _db.Orders.FindAsync(orderId);
                    if (order != null)
                    {
                        // Kiểm tra trạng thái thanh toán
                        if (responseCode == "00" && transactionStatus == "00")
                        {
                            // Thanh toán thành công
                            order.Status = "Paid";
                            order.PaymentMethod = "VNPay";
                            order.PaymentTransactionId = transactionNo;
                            await _db.SaveChangesAsync();

                            return Ok(new
                            {
                                Success = true,
                                Message = "Thanh toán thành công",
                                OrderId = orderId,
                                TransactionId = transactionNo
                            });
                        }
                        else
                        {
                            // Thanh toán thất bại
                            order.Status = "Cancelled";
                            await _db.SaveChangesAsync();

                            return Ok(new
                            {
                                Success = false,
                                Message = "Thanh toán thất bại",
                                ResponseCode = responseCode
                            });
                        }
                    }
                }

                return BadRequest("Order not found");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }

        /// <summary>
        /// IPN (Instant Payment Notification) từ VNPay - webhook
        /// </summary>
        [HttpGet("ipn")]
        [AllowAnonymous]
        public async Task<IActionResult> VNPayIPN()
        {
            try
            {
                var queryParams = Request.Query.ToDictionary(x => x.Key, x => x.Value.ToString());
                
                if (!queryParams.ContainsKey("vnp_SecureHash"))
                {
                    return Ok(new { RspCode = "97", Message = "Invalid signature" });
                }

                string vnpSecureHash = queryParams["vnp_SecureHash"];
                bool isValidSignature = _vnPayService.ValidateSignature(queryParams, vnpSecureHash);

                if (!isValidSignature)
                {
                    return Ok(new { RspCode = "97", Message = "Invalid signature" });
                }

                string responseCode = queryParams.GetValueOrDefault("vnp_ResponseCode", "");
                string txnRef = queryParams.GetValueOrDefault("vnp_TxnRef", "");

                if (int.TryParse(txnRef, out int orderId))
                {
                    var order = await _db.Orders.FindAsync(orderId);
                    if (order != null)
                    {
                        if (responseCode == "00")
                        {
                            if (order.Status != "Paid")
                            {
                                order.Status = "Paid";
                                await _db.SaveChangesAsync();
                            }
                            return Ok(new { RspCode = "00", Message = "Confirm Success" });
                        }
                        else
                        {
                            order.Status = "Cancelled";
                            await _db.SaveChangesAsync();
                            return Ok(new { RspCode = "00", Message = "Confirm Success" });
                        }
                    }
                    return Ok(new { RspCode = "01", Message = "Order not found" });
                }

                return Ok(new { RspCode = "02", Message = "Invalid order id" });
            }
            catch (Exception ex)
            {
                return Ok(new { RspCode = "99", Message = $"Error: {ex.Message}" });
            }
        }
    }
}


