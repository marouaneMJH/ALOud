using ALOud.DTOs.ExpertSystem;
using ALOud.Services.Infrastructure.ExpertSystem.Domain;

public interface IHybridExpertSystemService
{
    string Evaluate(Recommendation rec);
}
