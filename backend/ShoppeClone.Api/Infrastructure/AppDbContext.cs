using Microsoft.EntityFrameworkCore;
using ShoppeClone.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.EntityFrameworkCore.ChangeTracking;

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

        // THÊM DÒNG NÀY
        public DbSet<Payment> Payments => Set<Payment>();

        protected override void OnModelCreating(ModelBuilder b)
        {
            b.Entity<User>().HasIndex(x => x.Email).IsUnique();
            b.Entity<CartItem>().HasIndex(x => new { x.UserId, x.ProductId }).IsUnique();
            b.Entity<OrderItem>().HasOne(x => x.Product).WithMany().HasForeignKey(x => x.ProductId);
            b.Entity<OrderItem>().HasOne(x => x.Order).WithMany(o => o.Items).HasForeignKey(x => x.OrderId);

            // THÊM CONFIGURATION CHO PAYMENT
            b.Entity<Payment>()
                .HasOne(p => p.Order)
                .WithMany()
                .HasForeignKey(p => p.OrderId);

            b.Entity<Payment>()
                .Property(p => p.Amount)
                .HasPrecision(18, 2);

            // FIX DECIMAL PRECISION WARNINGS
            b.Entity<Order>()
                .Property(o => o.TotalAmount)
                .HasPrecision(18, 2);

            b.Entity<OrderItem>()
                .Property(oi => oi.UnitPrice)
                .HasPrecision(18, 2);

            b.Entity<Product>()
                .Property(p => p.Price)
                .HasPrecision(18, 2);

            // FIX EMBEDDING VALUE COMPARER WARNING - SỬA PHẦN NÀY
            b.Entity<KbChunk>()
                .Property(k => k.Embedding)
                .HasConversion(
                    v => FloatArrayToBytes(v),
                    v => BytesToFloatArray(v)
                )
                .Metadata.SetValueComparer(new ValueComparer<float[]>(
                    (c1, c2) => CompareFloatArrays(c1, c2),
                    c => ComputeFloatArrayHashCode(c),
                    c => CloneFloatArray(c)
                ));

            b.Entity<KbChunk>()
                .HasIndex(x => x.Sha256)
                .IsUnique();

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

        // THÊM CÁC PHƯƠNG THỨC HELPER MỚI - không dùng null-conditional operator
        private static bool CompareFloatArrays(float[]? c1, float[]? c2)
        {
            if (c1 == null && c2 == null) return true;
            if (c1 == null || c2 == null) return false;
            if (c1.Length != c2.Length) return false;

            for (int i = 0; i < c1.Length; i++)
            {
                if (c1[i] != c2[i]) return false;
            }
            return true;
        }

        private static int ComputeFloatArrayHashCode(float[]? c)
        {
            if (c == null) return 0;

            unchecked
            {
                int hash = 17;
                foreach (var value in c)
                {
                    hash = hash * 31 + value.GetHashCode();
                }
                return hash;
            }
        }

        private static float[]? CloneFloatArray(float[]? c)
        {
            if (c == null) return null;
            return (float[])c.Clone();
        }
    }
}