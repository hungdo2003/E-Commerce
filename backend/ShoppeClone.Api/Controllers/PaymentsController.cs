using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShoppeClone.Api.Infrastructure;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

[ApiController]
[Route("api/[controller]/zp")]
public class PaymentsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IHttpClientFactory _http;
    private readonly IConfiguration _cfg;

    public PaymentsController(AppDbContext db, IHttpClientFactory http, IConfiguration cfg)
    {
        _db = db;
        _http = http;
        _cfg = cfg;
    }

    private bool TryGetUserId(out int uid)
    {
        uid = 0;
        var uidStr =
            User.FindFirstValue(ClaimTypes.NameIdentifier) ??
            User.FindFirstValue(JwtRegisteredClaimNames.Sub);
        return int.TryParse(uidStr, out uid);
    }

    public record CreatePaymentDto(int OrderId, string Description);

    [HttpPost("create")]
    [Authorize]
    public async Task<IActionResult> Create(CreatePaymentDto dto)
    {
        if (!TryGetUserId(out var uid))
            return Unauthorized("Invalid or missing user id claim");

        var order = await _db.Orders.FirstOrDefaultAsync(o => o.Id == dto.OrderId && o.UserId == uid);
        if (order == null) return NotFound();

        var appId = _cfg.GetValue<int>("ZaloPay:AppId");
        var key1 = _cfg["ZaloPay:Key1"]!;
        var cbUrl = _cfg["ZaloPay:CallbackUrl"]!;

        var transSuffix = RandomNumberGenerator.GetInt32(0, 1_000_000);
        var transId = $"{DateTime.UtcNow:yyMMdd}_{transSuffix:000000}";
        var amount = (long)Math.Round(order.TotalAmount * 1000, MidpointRounding.AwayFromZero);

        var embedObject = new { redirecturl = "zalopay://app", orderId = order.Id };
        var embedData = JsonSerializer.Serialize(embedObject);
        var itemData = JsonSerializer.Serialize(new[]
        {
            new { id = order.Id, name = "Order", price = amount, quantity = 1 }
        });

        var payloadObj = new
        {
            app_id = appId,
            app_user = uid.ToString(),
            app_time = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            amount,
            app_trans_id = transId,
            embed_data = embedData,
            item = itemData,
            description = dto.Description,
            callback_url = cbUrl
        };

        string data = $"{payloadObj.app_id}|{payloadObj.app_trans_id}|{payloadObj.app_user}|{payloadObj.amount}|{payloadObj.app_time}|{payloadObj.embed_data}|{payloadObj.item}";
        string mac = HmacSHA256(key1, data);

        var req = new HttpRequestMessage(HttpMethod.Post, _cfg["ZaloPay:CreateOrderUrl"]);
        req.Content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            { "app_id", payloadObj.app_id.ToString() },
            { "app_trans_id", payloadObj.app_trans_id },
            { "app_user", payloadObj.app_user },
            { "app_time", payloadObj.app_time.ToString() },
            { "amount", payloadObj.amount.ToString() },
            { "embed_data", payloadObj.embed_data },
            { "item", payloadObj.item },
            { "description", dto.Description },
            { "callback_url", cbUrl },
            { "mac", mac }
        });

        var client = _http.CreateClient();
        var res = await client.SendAsync(req);
        var body = await res.Content.ReadAsStringAsync();
        return Content(body, "application/json");
    }

    [HttpPost("callback")]
    public async Task<IActionResult> Callback([FromForm] ZaloCallback cb)
    {
        var key2 = _cfg["ZaloPay:Key2"]!;
        var mac = HmacSHA256(key2, cb.data);
        if (mac != cb.mac) return BadRequest();

        var data = JsonDocument.Parse(cb.data);
        var embedRaw = data.RootElement.GetProperty("embed_data").GetString();
        if (string.IsNullOrEmpty(embedRaw)) return BadRequest();

        var orderId = JsonDocument.Parse(embedRaw).RootElement.GetProperty("orderId").GetInt32();
        var trans = data.RootElement.GetProperty("app_trans_id").GetString();

        var order = await _db.Orders.FirstOrDefaultAsync(o => o.Id == orderId);
        if (order != null)
        {
            order.Status = "Paid";
            order.PaymentTransactionId = trans;
            await _db.SaveChangesAsync();
        }

        return Ok(new { return_code = 1, return_message = "success" });
    }

    static string HmacSHA256(string key, string data)
    {
        var keyBytes = Encoding.UTF8.GetBytes(key);
        using var h = new HMACSHA256(keyBytes);
        var hash = h.ComputeHash(Encoding.UTF8.GetBytes(data));
        return BitConverter.ToString(hash).Replace("-", "").ToLower();
    }

    public class ZaloCallback
    {
        public string data { get; set; } = string.Empty;
        public string mac { get; set; } = string.Empty;
    }
}
