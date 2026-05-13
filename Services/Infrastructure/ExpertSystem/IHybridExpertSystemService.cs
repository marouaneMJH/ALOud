using ALOud.DTOs.ExpertSystem;
using ALOud.Services.Infrastructure.ExpertSystem.Domain;

namespace ALOud.Services.Infrastructure.ExpertSystem
{
    public interface IHybridExpertSystemService
    {
        Task<HybridEvaluationResult> EvaluateAsync(
            Recommendation rec,
            CancellationToken cancellationToken = default);
    }
}
