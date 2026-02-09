public interface IQueryEmbeddingService
{
    Task<float[]> EmbedAsync(
        string query,
        CancellationToken cancellationToken = default
    );
}
