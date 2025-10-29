using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using ShoppeClone.Api.Application.AI.Clients;

namespace ShoppeClone.Api.Application.AI.Clients
{
    public sealed class GeminiClient
    {
        private readonly HttpClient _http;
        private readonly GoogleAiOptions _opt;

        // ✅ Constructor cho Typed HttpClient
        public GeminiClient(HttpClient http, IOptions<GoogleAiOptions> opt)
        {
            _http = http;                  // đã được cấu hình BaseAddress trong Program.cs
            _opt = opt.Value;
        }

        public async Task<object> ListModelsAsync(CancellationToken ct)
        {
            var url = $"v1/models?key={_opt.ApiKey}"; // relative URL OK vì đã có BaseAddress
            using var res = await _http.GetAsync(url, ct);
            var json = await res.Content.ReadAsStringAsync(ct);
            res.EnsureSuccessStatusCode();
            return JsonSerializer.Deserialize<object>(json)!;
        }

        // public async Task<string> GenerateAsync(object payload, CancellationToken ct)
        // {
        //     var url = $"v1/models/gemini-2.5-flash:generateContent?key={_opt.ApiKey}";
        //     using var req = new HttpRequestMessage(HttpMethod.Post, url)
        //     {
        //         Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json")
        //     };

        //     using var res = await _http.SendAsync(req, ct);
        //     var body = await res.Content.ReadAsStringAsync(ct);
        //     res.EnsureSuccessStatusCode();

        //     using var doc = JsonDocument.Parse(body);
        //     var cands = doc.RootElement.GetProperty("candidates");
        //     if (cands.GetArrayLength() == 0) return "(empty)";

        //     var parts = cands[0].GetProperty("content").GetProperty("parts").EnumerateArray();
        //     var sb = new StringBuilder();
        //     foreach (var p in parts)
        //         if (p.TryGetProperty("text", out var t) && t.ValueKind == JsonValueKind.String)
        //             sb.Append(t.GetString());
        //     return sb.ToString();
        // }

        public async Task<string> GenerateAsync(object payload, CancellationToken ct)
{
 var model = "gemini-2.5-flash";
    var url = $"v1/models/{model}:generateContent?key={_opt.ApiKey}";

    using var req = new HttpRequestMessage(HttpMethod.Post, url)
    {
        Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json")
    };

    using var res = await _http.SendAsync(req, ct);
    var body = await res.Content.ReadAsStringAsync(ct);

    if (!res.IsSuccessStatusCode)
        throw new HttpRequestException($"Gemini error {(int)res.StatusCode}: {body}");

    using var doc = JsonDocument.Parse(body);
    var root = doc.RootElement;

    // Bị chặn an toàn?
    if (root.TryGetProperty("promptFeedback", out var pf) &&
        pf.TryGetProperty("blockReason", out var br) &&
        br.ValueKind == JsonValueKind.String &&
        !string.IsNullOrWhiteSpace(br.GetString()))
    {
        throw new InvalidOperationException($"Gemini blocked: {br.GetString()} | {body}");
    }

    // Chuẩn: candidates[0].content.parts[].text
    if (root.TryGetProperty("candidates", out var cands) &&
        cands.ValueKind == JsonValueKind.Array &&
        cands.GetArrayLength() > 0)
    {
        var first = cands[0];
        if (first.TryGetProperty("content", out var content) &&
            content.ValueKind == JsonValueKind.Object &&
            content.TryGetProperty("parts", out var parts) &&
            parts.ValueKind == JsonValueKind.Array)
        {
            var sb = new StringBuilder();
            foreach (var p in parts.EnumerateArray())
                if (p.TryGetProperty("text", out var t) && t.ValueKind == JsonValueKind.String)
                    sb.Append(t.GetString());
            var txt = sb.ToString();
            return string.IsNullOrWhiteSpace(txt) ? "(empty)" : txt;
        }
    }

    // Fallback: content.parts[].text ở root
    if (root.TryGetProperty("content", out var content2) &&
        content2.ValueKind == JsonValueKind.Object &&
        content2.TryGetProperty("parts", out var parts2) &&
        parts2.ValueKind == JsonValueKind.Array)
    {
        var sb = new StringBuilder();
        foreach (var p in parts2.EnumerateArray())
            if (p.TryGetProperty("text", out var t) && t.ValueKind == JsonValueKind.String)
                sb.Append(t.GetString());
        var txt = sb.ToString();
        return string.IsNullOrWhiteSpace(txt) ? "(empty)" : txt;
    }

    // Server trả lỗi dạng khác?
    if (root.TryGetProperty("error", out var err) &&
        err.TryGetProperty("message", out var msg) &&
        msg.ValueKind == JsonValueKind.String)
    {
        throw new InvalidOperationException($"Gemini error message: {msg.GetString()} | Raw: {body}");
    }

    throw new InvalidOperationException($"Gemini returned unexpected response. Raw: {body}");
}

    }
}
