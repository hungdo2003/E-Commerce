using Microsoft.AspNetCore.Mvc;
using ShoppeClone.Api.Application.AI.Clients;
using ShoppeClone.Api.Application.AI.Dtos;
using ShoppeClone.Api.Application.AI.Services;
using Microsoft.AspNetCore.Authorization;

namespace ShoppeClone.Api.Controllers;

[ApiController]
[Route("api/ai")]
public sealed class AiController : ControllerBase
{
    private readonly GeminiClient _gemini;
    private readonly IRagService _rag;

    public AiController(GeminiClient gemini, IRagService rag)
    { _gemini = gemini; _rag = rag; }

    [HttpGet("models")]
    public async Task<IActionResult> Models(CancellationToken ct)
        => Ok(await _gemini.ListModelsAsync(ct));

    [Consumes("application/json")]
    // [Authorize]
    [HttpPost("kb/index")]
    public async Task<IActionResult> Index([FromBody] KbUpsertRequest req, CancellationToken ct)
        => Ok(await _rag.IndexAsync(req, ct));
    
    [Consumes("application/json")]
    // [Authorize]
    [HttpPost("chat-rag")]
    public async Task<ActionResult<AiChatResponse>> ChatRag([FromBody] RagChatRequest req, CancellationToken ct)
        => Ok(await _rag.ChatAsync(req, ct));

[HttpPost("chat")]
public async Task<ActionResult<AiChatResponse>> Chat([FromBody] AiChatRequest req, CancellationToken ct)
{
    if (req?.messages == null || req.messages.Count == 0)
        return BadRequest(new AiChatResponse { error = true, detail = "Missing 'messages'." });

    // CSKH-friendly default system prompt (chỉ dùng khi system trống/placeholder)
    const string DefaultSystemCskh = @"
Bạn là trợ lý CSKH e-commerce, giao tiếp như người thật: lịch sự, thân thiện, rõ ràng.
Quy tắc:
- Trả lời ngắn gọn, đúng trọng tâm, bám sát câu hỏi; không bịa.
- Nếu thiếu dữ liệu: nói “Chưa đủ thông tin.” và gợi ý 1–2 câu hỏi làm rõ.
- Không dùng giọng mệnh lệnh; ưu tiên câu ngắn, dễ hiểu.
Định dạng:
- **TL;DR:** 1 câu trực tiếp.
- 1–2 gạch đầu dòng ngắn, thân thiện.
";

    var contents = new List<object>();

    // chỉ chèn system nếu hợp lệ; nếu trống/placeholder thì dùng CSKH default
    var sys = (req.system ?? "").Trim();
    if (string.IsNullOrWhiteSpace(sys) || string.Equals(sys, "string", StringComparison.OrdinalIgnoreCase))
    {
        sys = DefaultSystemCskh.Trim();
    }
    contents.Add(new { role = "user", parts = new[] { new { text = sys } } });

    // map các tin nhắn còn lại
    contents.AddRange(req.messages.Select(m => new
    {
        role = (m.role == "assistant") ? "model" : "user",
        parts = new[] { new { text = m.content ?? string.Empty } }
    }));

    var payload = new
    {
        contents,
        generationConfig = new
        {
            temperature = 0.3,
            maxOutputTokens = 512,
            topP = 0.9
        }
    };

    var reply = await _gemini.GenerateAsync(payload, ct);
    return Ok(new AiChatResponse { reply = reply });
}




}