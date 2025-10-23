using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

[ApiController]
[Route("api/[controller]")]
public class ChatController : ControllerBase
{
    private readonly IHttpClientFactory _http; private readonly IConfiguration _cfg;
    public ChatController(IHttpClientFactory http, IConfiguration cfg){ _http=http; _cfg=cfg; }

    public record ChatDto(string message, string? productContext);

    [HttpPost("ask")]
    public async Task<IActionResult> Ask(ChatDto dto)
    {
        var endpoint = _cfg["Chat:ProviderEndpoint"]!;
        var apiKey = _cfg["Chat:ApiKey"]!;
        var model = _cfg["Chat:Model"]!;

        var reqBody = new {
            model,
            messages = new object[]{
                new { role = "system", content = "Bạn là trợ lý tư vấn mua sắm, gợi ý sản phẩm phù hợp." },
                new { role = "user", content = dto.productContext==null? dto.message : $"[context:{dto.productContext}]\n{dto.message}" }
            },
            temperature = 0.3
        };
        var http = _http.CreateClient();
        var req = new HttpRequestMessage(HttpMethod.Post, endpoint);
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        req.Content = new StringContent(JsonSerializer.Serialize(reqBody), Encoding.UTF8, "application/json");
        var res = await http.SendAsync(req);
        var body = await res.Content.ReadAsStringAsync();
        return Content(body, "application/json");
    }
}
