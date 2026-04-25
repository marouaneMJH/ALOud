using ALOud.Services.Infrastructure.ExpertSystem.Domain;

namespace ALOud.Services.Infrastructure.ExpertSystem
{
    public interface IHybridExpertSystemService
    {
        Task<string> EvaluateAsync(
            Recommendation rec,
            CancellationToken cancellationToken = default);
    }
}
