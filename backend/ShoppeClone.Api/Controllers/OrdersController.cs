using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShoppeClone.Api.Domain.Entities;
using ShoppeClone.Api.Infrastructure;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

namespace ShoppeClone.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly AppDbContext _db;
        public OrdersController(AppDbContext db) { _db = db; }

        // TẠM THỜI: Method không cần user id
        private bool TryGetUserId(out int uid)
        {
            // Tạm thời return user id mặc định để test
            uid = 1; // Hoặc lấy user id đầu tiên từ database
            return true;

            // HOẶC comment toàn bộ method và dùng trực tiếp:
            // uid = 1;
            // return true;
        }

        [HttpPost("create-from-cart")]
        public async Task<IActionResult> CreateFromCart()
        {
            try
            {
                var uid = 2; // UserId có giỏ hàng

                var cart = await _db.CartItems
                    .Include(x => x.Product)
                    .Where(x => x.UserId == uid)
                    .ToListAsync();

                if (!cart.Any())
                    return BadRequest(new { Message = "Cart empty" });

                // Kiểm tra tồn kho
                foreach (var item in cart)
                {
                    if (item.Product.Stock < item.Quantity)
                    {
                        return BadRequest(new
                        {
                            Message = $"Sản phẩm {item.Product.Name} chỉ còn {item.Product.Stock} sản phẩm"
                        });
                    }
                }

                // Tạo order với đầy đủ thông tin
                var order = new Order
                {
                    UserId = uid,
                    Status = "Pending",
                    CreatedAt = DateTime.UtcNow,
                    TotalAmount = 0,
                    PaymentMethod = "Cash"
                };

                foreach (var c in cart)
                {
                    order.Items.Add(new OrderItem
                    {
                        ProductId = c.ProductId,
                        Quantity = c.Quantity,
                        UnitPrice = c.Product.Price,
                        CreatedAt = DateTime.UtcNow
                    });
                    order.TotalAmount += c.Product.Price * c.Quantity;

                    // Cập nhật tồn kho
                    c.Product.Stock -= c.Quantity;
                }

                _db.Orders.Add(order);
                _db.CartItems.RemoveRange(cart);

                // THÊM LOG ĐỂ DEBUG
                Console.WriteLine($"Creating order for user {uid}");
                Console.WriteLine($"Total amount: {order.TotalAmount}");
                Console.WriteLine($"Cart items count: {cart.Count}");

                await _db.SaveChangesAsync();

                return Ok(new CreateOrderResponse
                {
                    OrderId = order.Id,
                    TotalAmount = order.TotalAmount,
                    Status = order.Status
                });
            }
            catch (Exception ex)
            {
                // LOG CHI TIẾT LỖI
                Console.WriteLine($"ERROR: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"INNER ERROR: {ex.InnerException.Message}");
                    return BadRequest(new { Message = $"Database Error: {ex.InnerException.Message}" });
                }
                return BadRequest(new { Message = $"Error: {ex.Message}" });
            }
        }



        [HttpGet]
        public async Task<IActionResult> MyOrders()
        {
            if (!TryGetUserId(out var uid))
                return Unauthorized("Invalid or missing user id claim");

            var data = await _db.Orders
                .Where(o => o.UserId == uid)
                .OrderByDescending(o => o.Id)
                .Select(o => new
                {
                    o.Id,
                    o.CreatedAt,
                    o.TotalAmount,
                    o.Status,
                    o.PaymentMethod,
                    o.PaymentDate
                })
                .ToListAsync();

            return Ok(data);
        }

        // THÊM API để lấy chi tiết đơn hàng
        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrder(int id)
        {
            if (!TryGetUserId(out var uid))
                return Unauthorized("Invalid or missing user id claim");

            var order = await _db.Orders
                .Include(o => o.Items)
                .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(o => o.Id == id && o.UserId == uid);

            if (order == null)
                return NotFound();

            return Ok(new
            {
                order.Id,
                order.CreatedAt,
                order.TotalAmount,
                order.Status,
                order.PaymentMethod,
                order.PaymentTransactionId,
                order.PaymentDate,
                Items = order.Items.Select(i => new
                {
                    i.ProductId,
                    ProductName = i.Product.Name,
                    i.Quantity,
                    i.UnitPrice,
                    SubTotal = i.Quantity * i.UnitPrice
                })
            });
        }

        [HttpGet("debug-db-info")]
        public async Task<IActionResult> DebugDbInfo()
        {
            var connectionString = _db.Database.GetConnectionString();
            var databaseName = await _db.Database.ExecuteSqlRawAsync("SELECT DB_NAME()");

            return Ok(new
            {
                ConnectionString = connectionString,
                CurrentDatabase = databaseName,
                Server = _db.Database.GetDbConnection().DataSource
            });
        }
    }

    // THÊM class response cho create order - ĐẶT NGOÀI CLASS CONTROLLER
    public class CreateOrderResponse
    {
        public int OrderId { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}