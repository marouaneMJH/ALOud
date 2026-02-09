using ALOud.DTOs.ExpertSystem;
using ALOud.Services.Infrastructure.ExpertSystem.Domain;

public interface IExpertSystemService
{
    Recommendation Evaluate(UserProfileDto profile);
}
