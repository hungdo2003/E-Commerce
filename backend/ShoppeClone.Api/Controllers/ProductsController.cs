using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShoppeClone.Api.Infrastructure;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly AppDbContext _db;
    public ProductsController(AppDbContext db) { _db = db; }

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery]int page=1, [FromQuery]int size=20, [FromQuery]string? q=null, [FromQuery]int? categoryId=null)
    {
        var query = _db.Products.AsNoTracking().Include(x=>x.Category).AsQueryable();
        if(!string.IsNullOrWhiteSpace(q)) query = query.Where(x => x.Name.Contains(q));
        if(categoryId.HasValue) query = query.Where(x => x.CategoryId == categoryId);
        var total = await query.CountAsync();
        var items = await query.OrderByDescending(x=>x.Id).Skip((page-1)*size).Take(size).Select(x=> new{
            x.Id, x.Name, x.Price, x.ThumbnailUrl, Category = x.Category.Name
        }).ToListAsync();
        return Ok(new { total, page, size, items });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var p = await _db.Products.AsNoTracking().FirstOrDefaultAsync(x=>x.Id==id);
        return p==null? NotFound(): Ok(p);
    }
}
