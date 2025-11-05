using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShoppeClone.Api.Infrastructure;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

[ApiController]
[Route("api/[controller]")]
// [Authorize]
public class CartController : ControllerBase
{
    private readonly AppDbContext _db;
    public CartController(AppDbContext db) { _db = db; }

    private bool TryGetUserId(out int uid)
    {
        // Tạm thời dùng user ID 2 cho testing
        uid = 2; // User "Khoa Nguyen"
        return true;
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

        // KIỂM TRA SẢN PHẨM TỒN TẠI VÀ CÒN HÀNG
        var product = await _db.Products.FindAsync(dto.ProductId);
        if (product == null)
            return BadRequest("Sản phẩm không tồn tại");

        if (product.Stock < dto.Quantity)
            return BadRequest($"Sản phẩm '{product.Name}' chỉ còn {product.Stock} sản phẩm trong kho");

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
            // KIỂM TRA TỔNG SỐ LƯỢNG SAU KHI CỘNG THÊM
            int newQuantity = item.Quantity + dto.Quantity;
            if (product.Stock < newQuantity)
                return BadRequest($"Số lượng vượt quá tồn kho. Chỉ còn {product.Stock} sản phẩm");

            item.Quantity = newQuantity;
        }

        await _db.SaveChangesAsync();
        return Ok(new { Message = "Đã thêm vào giỏ hàng" });
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
