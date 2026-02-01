public interface IChatOrchestratorService
{
    Task<string> HandleAsync(
        Guid userId,
        string message,
        CancellationToken cancellationToken = default
    );
}
