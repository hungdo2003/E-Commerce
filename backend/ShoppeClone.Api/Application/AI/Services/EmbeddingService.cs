using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using ShoppeClone.Api.Application.AI.Clients;
using ShoppeClone.Api.Application.AI.Interfaces;

namespace ShoppeClone.Api.Application.AI.Services;

public sealed class EmbeddingService : IEmbeddingService
{
    private readonly HttpClient _http;
    private readonly GoogleAiOptions _opt;

    public EmbeddingService(IHttpClientFactory f, IOptions<GoogleAiOptions> opt)
    {
        _http = f.CreateClient("googleai");   // dùng named client đã cấu hình
        _opt = opt.Value;
    }

    public async Task<float[]> EmbedAsync(string text, string taskType = "RETRIEVAL_DOCUMENT", CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(_opt.ApiKey))
            throw new InvalidOperationException("Missing GoogleAI:ApiKey");

        var url = $"https://generativelanguage.googleapis.com/v1/models/text-embedding-004:embedContent?key={_opt.ApiKey}";

        var payload = new
        {
            content = new { parts = new[] { new { text } } },
            task_type = taskType // "RETRIEVAL_DOCUMENT" hoặc "RETRIEVAL_QUERY"
        };

        using var req = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json")
        };

        using var res = await _http.SendAsync(req, ct);
        var body = await res.Content.ReadAsStringAsync(ct);
        if (!res.IsSuccessStatusCode)
            throw new HttpRequestException($"Embeddings error {(int)res.StatusCode}: {body}");

        using var json = JsonDocument.Parse(body);
        if (!json.RootElement.TryGetProperty("embedding", out var emb) ||
            !emb.TryGetProperty("values", out var vals))
            throw new InvalidOperationException("Embedding response missing 'embedding.values'.");

        return vals.EnumerateArray().Select(v => v.GetSingle()).ToArray();
    }
}
