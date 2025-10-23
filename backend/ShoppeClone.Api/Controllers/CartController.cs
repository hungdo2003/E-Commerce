using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShoppeClone.Api.Infrastructure;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CartController : ControllerBase
{
    private readonly AppDbContext _db;
    public CartController(AppDbContext db) { _db = db; }

    private bool TryGetUserId(out int uid)
    {
        uid = 0;
        var uidStr =
            User.FindFirstValue(ClaimTypes.NameIdentifier) ??
            User.FindFirstValue(JwtRegisteredClaimNames.Sub);
        return int.TryParse(uidStr, out uid);
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        if (!TryGetUserId(out var userId))
            return Unauthorized("Invalid or missing user id claim");

        var items = await _db.CartItems
            .Include(x => x.Product)
            .Where(x => x.UserId == userId)
            .Select(x => new { x.Id, x.ProductId, x.Quantity, x.Product.Name, x.Product.Price, x.Product.ThumbnailUrl })
            .ToListAsync();
        return Ok(items);
    }

    [HttpPost]
    public async Task<IActionResult> Add(AddCartDto dto)
    {
        if (!TryGetUserId(out var userId))
            return Unauthorized("Invalid or missing user id claim");

        var item = await _db.CartItems
            .FirstOrDefaultAsync(x => x.UserId == userId && x.ProductId == dto.ProductId);

        if (item == null)
        {
            item = new ShoppeClone.Api.Domain.Entities.CartItem
            {
                UserId = userId,
                ProductId = dto.ProductId,
                Quantity = dto.Quantity
            };
            _db.CartItems.Add(item);
        }
        else
        {
            item.Quantity += dto.Quantity;
        }

        await _db.SaveChangesAsync();
        return Ok();
    }

    [HttpDelete("{productId}")]
    public async Task<IActionResult> Remove(int productId)
    {
        if (!TryGetUserId(out var userId))
            return Unauthorized("Invalid or missing user id claim");

        var item = await _db.CartItems
            .FirstOrDefaultAsync(x => x.UserId == userId && x.ProductId == productId);

        if (item == null) return NotFound();

        _db.CartItems.Remove(item);
        await _db.SaveChangesAsync();
        return Ok();
    }
}

public record AddCartDto(int ProductId, int Quantity);
