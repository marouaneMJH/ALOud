using ALOud.DTOs.ExpertSystem;
using ALOud.Services.Infrastructure.ExpertSystem.Domain;
using ALOud.Services.Infrastructure.ExpertSystem.Mappers;


public class ExpertSystemService : IExpertSystemService
{
    private readonly ExpertSystemEngine _engine = new();

    public Recommendation Evaluate(UserProfileDto profile)
        => _engine.Run(UserProfileMapper.ToDomain(profile));
}
