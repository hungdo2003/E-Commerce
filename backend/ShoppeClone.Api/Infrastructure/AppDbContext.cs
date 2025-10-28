using Microsoft.EntityFrameworkCore;
using ShoppeClone.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion; 

namespace ShoppeClone.Api.Infrastructure
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        public DbSet<User> Users => Set<User>();
        public DbSet<Category> Categories => Set<Category>();
        public DbSet<Product> Products => Set<Product>();
        public DbSet<CartItem> CartItems => Set<CartItem>();
        public DbSet<Order> Orders => Set<Order>();
        public DbSet<OrderItem> OrderItems => Set<OrderItem>();
        public DbSet<KbChunk> KbChunks => Set<KbChunk>();

        protected override void OnModelCreating(ModelBuilder b)
        {
            b.Entity<User>().HasIndex(x => x.Email).IsUnique();
            b.Entity<CartItem>().HasIndex(x => new { x.UserId, x.ProductId }).IsUnique();
            b.Entity<OrderItem>().HasOne(x => x.Product).WithMany().HasForeignKey(x => x.ProductId);
            b.Entity<OrderItem>().HasOne(x => x.Order).WithMany(o => o.Items).HasForeignKey(x => x.OrderId);
            
// ===== KbChunk
            b.Entity<KbChunk>()
             .HasIndex(x => x.Sha256)
             .IsUnique();

            // Dùng converter với kiểu nullable và expression trỏ tới hàm static
            var floatArrayToBytes = new ValueConverter<float[]?, byte[]?>(
                v => FloatArrayToBytes(v),
                v => BytesToFloatArray(v)
            );

            b.Entity<KbChunk>(e =>
            {
                e.Property(p => p.Embedding)
                 .HasConversion(floatArrayToBytes)    // <- hết lỗi CS0834 & CS8620
                 .HasColumnType("varbinary(max)");

                e.Property(p => p.Text)
                 .HasColumnType("nvarchar(max)");
            });

            base.OnModelCreating(b);
        }

        // ===== Helpers cho converter (được phép gọi trong expression tree)
        private static byte[]? FloatArrayToBytes(float[]? v)
        {
            if (v == null || v.Length == 0) return Array.Empty<byte>();
            var bytes = new byte[v.Length * sizeof(float)];
            Buffer.BlockCopy(v, 0, bytes, 0, bytes.Length);
            return bytes;
        }

        private static float[]? BytesToFloatArray(byte[]? v)
        {
            if (v == null || v.Length == 0) return Array.Empty<float>();
            var floats = new float[v.Length / sizeof(float)];
            Buffer.BlockCopy(v, 0, floats, 0, v.Length);
            return floats;
        }
    }
}
