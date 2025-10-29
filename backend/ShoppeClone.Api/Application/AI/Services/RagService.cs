using System.Security.Cryptography;
using System.Text;
using ShoppeClone.Api.Application.AI.Clients;
using ShoppeClone.Api.Application.AI.Dtos;
using ShoppeClone.Api.Application.AI.Interfaces;
using ShoppeClone.Api.Infrastructure.Repositories;

namespace ShoppeClone.Api.Application.AI.Services;

public interface IRagService
{
    Task<object> IndexAsync(KbUpsertRequest req, CancellationToken ct);
    Task<AiChatResponse> ChatAsync(RagChatRequest req, CancellationToken ct);
}

public sealed class RagService : IRagService
{
    private readonly IKbRepository _repo;
    private readonly IEmbeddingService _emb;
    private readonly GeminiClient _gemini;

    public RagService(IKbRepository repo, IEmbeddingService emb, GeminiClient gemini)
    { _repo = repo; _emb = emb; _gemini = gemini; }

    public async Task<object> IndexAsync(KbUpsertRequest req, CancellationToken ct)
    {
        if (req?.items == null || req.items.Count == 0) return new { inserted = 0 };

        var cleaned = req.items.Select(x => new {
            x.id, x.title, x.source, x.tags, text = (x.text ?? "").Trim()
        }).Where(x => x.text.Length > 0).ToList();

        var toInsert = new List<ShoppeClone.Api.Domain.Entities.KbChunk>();
        foreach (var it in cleaned)
        {
            var sha = Sha256(it.text);
            if (await _repo.ExistsShaAsync(sha, ct)) continue;
            var vec = await _emb.EmbedAsync(it.text, "RETRIEVAL_DOCUMENT", ct);
            toInsert.Add(new ShoppeClone.Api.Domain.Entities.KbChunk{
                Id = string.IsNullOrWhiteSpace(it.id) ? Guid.NewGuid().ToString("N") : it.id!,
                Title = it.title, Source = it.source, Tags = it.tags, Text = it.text,
                Embedding = vec, Sha256 = sha
            });
        }
        await _repo.AddRangeAsync(toInsert, ct);
        return new { inserted = toInsert.Count };

        static string Sha256(string s)
        {
            using var sha = SHA256.Create();
            return BitConverter.ToString(sha.ComputeHash(Encoding.UTF8.GetBytes(s))).Replace("-", "").ToLowerInvariant();
        }
    }

    public async Task<AiChatResponse> ChatAsync(RagChatRequest req, CancellationToken ct)
    {
        var lastUser = req.messages.LastOrDefault(m => (m.role ?? "user") != "assistant")?.content
                       ?? req.messages.LastOrDefault()?.content ?? "";

        var qvec = await _emb.EmbedAsync(lastUser, "RETRIEVAL_QUERY", ct);
        var cand = await _repo.CandidateAsync(req.tag, req.source, req.tenantId, 300, ct);

        var minScore = 0.38;
        var top = cand.Where(x => x.Embedding != null)
                      .Select(x => new { item = x, score = IEmbeddingService.Cosine(qvec, x.Embedding!) })
                      .Where(x => x.score >= minScore)
                      .OrderByDescending(x => x.score)
                      .Take(Math.Clamp(req.k, 3, 8))
                      .ToList();

                      if (!top.Any())
{
    // Có thể tuỳ biến câu này theo giọng CSKH bạn đang dùng
    return new AiChatResponse
    {
        reply = "Chưa đủ thông tin để trả lời chính xác. Bạn mô tả thêm giúp mình nhé!"
    };
}

        static string Trunc(string s, int max=1200) => s.Length <= max ? s : s.Substring(0, max) + " …";
        // var ctx = string.Join("\n---\n", top.Select((x,i) =>
        //     $"[C{i+1} | SCORE:{x.score:F3} | TITLE:{x.item.Title} | SOURCE:{x.item.Source}]\n{Trunc(x.item.Text)}"));
        var ctx = string.Join("\n---\n", top.Select(x =>
    $"{(x.item.Title ?? "(không tiêu đề)")} — {(x.item.Source ?? "-")}\n{Trunc(x.item.Text)}"));


        var system = req.system ?? "Bạn là trợ lý e-commerce. Chỉ dùng NGỮ CẢNH để trả lời.";
//         var userAugmented = $@"
// # Nhiệm vụ
// Trả lời ngắn gọn, **Markdown đẹp**, chỉ dùng NGỮ CẢNH.

// # Câu hỏi
// {lastUser}

// # NGỮ CẢNH (Top-K | C# | SCORE | TITLE — SOURCE)
// {ctx}

// # Định dạng
// - **TL;DR:** 1–2 câu.
// - 3–6 bullet **hành động**.
// - **Nguồn trích dẫn**: liệt kê C# dùng; nêu rõ thiếu gì nếu có.";

var userAugmented = $@"
# Vai trò
Bạn là trợ lý CSKH e-commerce, giao tiếp như người thật: lịch sự, thân thiện, rõ ràng.

# Mục tiêu
Trả lời đúng trọng tâm, dễ hiểu bằng tiếng Việt, bám sát **NGỮ CẢNH**, không bịa.

# Câu hỏi của khách
{lastUser}

# NGỮ CẢNH (chỉ để tham chiếu, không trích dẫn lại cho khách)
{ctx}

# Quy tắc
- Có dữ liệu trong NGỮ CẢNH → trả lời trực tiếp.
- Thiếu/mơ hồ → nói ngắn gọn “Chưa đủ thông tin.” và gợi ý 1–2 câu hỏi làm rõ.
- Không nhắc đến nguồn, không dùng mã hiệu C1/C2, không nêu điểm số hay tên file.
- Chỉ dùng số liệu/thời gian/phí có trong NGỮ CẢNH; không suy đoán.

# Định dạng
- **TL;DR:** 1 câu duy nhất (trả lời trực tiếp).
- 2–3 gạch đầu dòng: thân thiện, mang tính hỗ trợ, tránh giọng mệnh lệnh (tránh “hãy/đảm bảo/thiết lập…”).
- Tổng độ dài ≤ 120 từ.
";


    var payload = new
{
    contents = new object[]
    {
        new { role = "user", parts = new[] { new { text = system } } },
        new { role = "user", parts = new[] { new { text = userAugmented } } }
    },
   generationConfig = new {
    temperature = 0.15,     // giảm “bay bổng” để chính xác
    maxOutputTokens = 1024,  // khống chế độ dài câu trả lời
    topP = 0.9
}

};

        var reply = await _gemini.GenerateAsync(payload, ct);
        return new AiChatResponse { reply = reply };
    }
}
