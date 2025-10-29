namespace ShoppeClone.Api.Application.AI.Dtos;

public sealed class AiMessage { public string role { get; set; } = "user"; public string content { get; set; } = ""; }
public sealed class AiChatRequest { public string? system { get; set; } public List<AiMessage> messages { get; set; } = new(); }
public sealed class AiChatResponse { public string reply { get; set; } = ""; public bool error { get; set; } public int? status { get; set; } public string? detail { get; set; } }

public sealed class KbUpsertItem { public string? id { get; set; } public string? title { get; set; } public string? source { get; set; } public string? tags { get; set; } public string? text { get; set; } }
public sealed class KbUpsertRequest { public List<KbUpsertItem> items { get; set; } = new(); }

public sealed class RagChatRequest
{
    public string? system { get; set; }
    public List<AiMessage> messages { get; set; } = new();
    public int k { get; set; } = 5;
    public string? tag { get; set; }
    public string? source { get; set; }
    public string? tenantId { get; set; }
}
