using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShoppeClone.Api.Domain.Entities;
using ShoppeClone.Api.Infrastructure;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly AppDbContext _db;
    public OrdersController(AppDbContext db) { _db = db; }

    // Helper: lấy UserId an toàn từ claims
    private bool TryGetUserId(out int uid)
    {
        uid = 0;
        var uidStr =
            User.FindFirstValue(ClaimTypes.NameIdentifier) ??
            User.FindFirstValue(JwtRegisteredClaimNames.Sub);
        return int.TryParse(uidStr, out uid);
    }

    [HttpPost("create-from-cart")]
    public async Task<IActionResult> CreateFromCart()
    {
        if (!TryGetUserId(out var uid))
            return Unauthorized("Invalid or missing user id claim");

        var cart = await _db.CartItems
            .Include(x => x.Product)
            .Where(x => x.UserId == uid)
            .ToListAsync();

        if (!cart.Any()) return BadRequest("Cart empty");

        var order = new Order { UserId = uid };
        foreach (var c in cart)
        {
            order.Items.Add(new OrderItem
            {
                ProductId = c.ProductId,
                Quantity = c.Quantity,
                UnitPrice = c.Product.Price
            });
            order.TotalAmount += c.Product.Price * c.Quantity;
            c.Product.Stock -= c.Quantity;
        }

        _db.Orders.Add(order);
        _db.CartItems.RemoveRange(cart);
        await _db.SaveChangesAsync();

        return Ok(new { orderId = order.Id, order.TotalAmount });
    }

    [HttpGet]
    public async Task<IActionResult> MyOrders()
    {
        if (!TryGetUserId(out var uid))
            return Unauthorized("Invalid or missing user id claim");

        var data = await _db.Orders
            .Where(o => o.UserId == uid)
            .OrderByDescending(o => o.Id)
            .Select(o => new { o.Id, o.CreatedAt, o.TotalAmount, o.Status })
            .ToListAsync();

        return Ok(data);
    }
}
