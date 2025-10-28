using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ShoppeClone.Api.Domain.Entities;

[Index(nameof(Sha256), IsUnique = true)]
public class KbChunk
{
    [Key] public string Id { get; set; } = Guid.NewGuid().ToString("N");
    [MaxLength(300)] public string? Title { get; set; }
    [MaxLength(500)] public string? Source { get; set; }
    [MaxLength(200)] public string? Tags { get; set; }
    public string Text { get; set; } = "";
    public float[]? Embedding { get; set; }   // 768-d
    [MaxLength(64)] public string? Sha256 { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? TenantId { get; set; }    
}
