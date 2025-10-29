namespace ShoppeClone.Api.Application.AI.Interfaces
{
    public interface IEmbeddingService
    {
        Task<float[]> EmbedAsync(string text, string taskType = "RETRIEVAL_DOCUMENT", CancellationToken ct = default);

        static double Cosine(float[] a, float[] b)
        {
            if (a == null || b == null || a.Length == 0 || b.Length == 0) return 0.0;

            int n = Math.Min(a.Length, b.Length);
            double dot = 0, na = 0, nb = 0;
            for (int i = 0; i < n; i++)
            {
                dot += a[i] * b[i];
                na  += a[i] * a[i];
                nb  += b[i] * b[i];
            }
            return (na == 0 || nb == 0) ? 0.0 : dot / (Math.Sqrt(na) * Math.Sqrt(nb));
        }
    }
}
