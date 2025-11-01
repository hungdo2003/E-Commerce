using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShoppeClone.Api.Infrastructure;

namespace ShoppeClone.Api.Controllers
{
    /// <summary>
    /// Mock Payment Controller - CHỈ DÙNG ĐỂ TEST
    /// Bypass VNPay và complete payment trực tiếp
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class MockPaymentController : ControllerBase
    {
        private readonly AppDbContext _db;

        public MockPaymentController(AppDbContext db)
        {
            _db = db;
        }

        /// <summary>
        /// Complete payment cho một order (TEST MODE)
        /// </summary>
        [HttpPost("complete/{orderId}")]
        public async Task<IActionResult> CompletePayment(int orderId)
        {
            try
            {
                var order = await _db.Orders.FindAsync(orderId);
                
                if (order == null)
                {
                    return NotFound(new { 
                        Success = false, 
                        Message = "Không tìm thấy đơn hàng" 
                    });
                }

                if (order.Status == "Paid")
                {
                    return BadRequest(new { 
                        Success = false, 
                        Message = "Đơn hàng đã được thanh toán rồi" 
                    });
                }

                // Update order status
                order.Status = "Paid";
                order.PaymentMethod = "VNPay (Mock - Test Mode)";
                order.PaymentTransactionId = $"MOCK_{DateTime.Now.Ticks}";
                
                await _db.SaveChangesAsync();

                return Ok(new
                {
                    Success = true,
                    Message = "✅ Thanh toán thành công (Test Mode)",
                    OrderId = orderId,
                    TransactionId = order.PaymentTransactionId,
                    Amount = order.TotalAmount,
                    Status = order.Status,
                    Note = "Đây là mock payment - chỉ dùng để test. Production sẽ dùng VNPay thật."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { 
                    Success = false, 
                    Message = $"Lỗi: {ex.Message}" 
                });
            }
        }

        /// <summary>
        /// Lấy danh sách orders để test
        /// </summary>
        [HttpGet("orders")]
        public async Task<IActionResult> GetOrders()
        {
            try
            {
                var userId = User.FindFirst("userId")?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized();
                }

                var orders = await _db.Orders
                    .Where(o => o.UserId == int.Parse(userId))
                    .OrderByDescending(o => o.CreatedAt)
                    .Select(o => new
                    {
                        o.Id,
                        o.TotalAmount,
                        o.Status,
                        o.CreatedAt,
                        o.PaymentMethod,
                        o.PaymentTransactionId
                    })
                    .ToListAsync();

                return Ok(new { Success = true, Orders = orders });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { 
                    Success = false, 
                    Message = $"Lỗi: {ex.Message}" 
                });
            }
        }
    }
}

