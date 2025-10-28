using Microsoft.EntityFrameworkCore;
using ShoppeClone.Api.Infrastructure;           // AppDbContext
using ShoppeClone.Api.Domain.Entities;          // KbChunk

namespace ShoppeClone.Api.Infrastructure.Repositories
{
    public interface IKbRepository
    {
        Task<bool> ExistsShaAsync(string sha, CancellationToken ct);
        Task AddRangeAsync(IEnumerable<KbChunk> chunks, CancellationToken ct);
        Task<List<KbChunk>> CandidateAsync(string? tag, string? source, string? tenantId, int limit, CancellationToken ct);
    }

    public sealed class KbRepository : IKbRepository
    {
        private readonly AppDbContext _db;
        public KbRepository(AppDbContext db) { _db = db; }

        public Task<bool> ExistsShaAsync(string sha, CancellationToken ct)
            => _db.Set<KbChunk>().AnyAsync(x => x.Sha256 == sha, ct);

        public async Task AddRangeAsync(IEnumerable<KbChunk> chunks, CancellationToken ct)
        {
            await _db.AddRangeAsync(chunks, ct);
            await _db.SaveChangesAsync(ct);
        }

        public Task<List<KbChunk>> CandidateAsync(string? tag, string? source, string? tenantId, int limit, CancellationToken ct)
        {
            // Dùng Set<KbChunk>() để KHÔNG cần property KbChunks trong AppDbContext
            var q = _db.Set<KbChunk>().AsNoTracking();

            if (!string.IsNullOrWhiteSpace(tag))
                q = q.Where(x => (x.Tags ?? string.Empty).Contains(tag));

            if (!string.IsNullOrWhiteSpace(source))
                q = q.Where(x => x.Source == source);

            // BỎ lọc TenantId vì entity hiện tại không có thuộc tính này
            // (giữ nguyên tham số để không phải đổi IRagService/RagService)

            int take = limit <= 0 ? 100 : Math.Clamp(limit, 50, 500);

            return q.OrderByDescending(x => x.CreatedAt)
                    .Select(x => new KbChunk
                    {
                        Id = x.Id,
                        Title = x.Title,
                        Source = x.Source,
                        Tags = x.Tags,
                        Text = x.Text,
                        Embedding = x.Embedding
                    })
                    .Take(take)
                    .ToListAsync(ct);
        }
    }
}
