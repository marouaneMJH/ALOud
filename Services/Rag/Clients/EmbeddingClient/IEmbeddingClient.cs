namespace ALOud.Services.Rag.Clients;


public interface IEmbeddingClient
{
    Task<float[]> CreateEmbeddingAsync(
        string input,
        CancellationToken cancellationToken = default
    );
}
