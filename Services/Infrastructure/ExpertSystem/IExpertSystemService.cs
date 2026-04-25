using ALOud.DTOs.ExpertSystem;
using ALOud.Services.Infrastructure.ExpertSystem.Domain;

namespace ALOud.Services.Infrastructure.ExpertSystem
{
    public interface IExpertSystemService
    {
        Recommendation Evaluate(UserProfileDto profile);
    }
}
