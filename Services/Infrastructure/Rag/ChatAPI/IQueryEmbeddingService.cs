namespace ALOud.Services.Infrastructure.Rag.ChatAPI;

public interface IQueryEmbeddingService
{
    Task<float[]> EmbedAsync(
        string query,
        CancellationToken cancellationToken = default
    );
}
