using ALOud.Services.Infrastructure.ExpertSystem.Domain;

public interface IHybridExpertSystemService
{
    Task<string> EvaluateAsync(
        Recommendation rec,
        CancellationToken cancellationToken = default);
}
