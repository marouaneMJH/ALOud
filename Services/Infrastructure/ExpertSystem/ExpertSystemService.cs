using ALOud.DTOs.ExpertSystem;
using ALOud.Services.Infrastructure.ExpertSystem.Domain;
using ALOud.Services.Infrastructure.ExpertSystem.Mappers;

namespace ALOud.Services.Infrastructure.ExpertSystem
{
    public class ExpertSystemService : IExpertSystemService
    {
        private readonly ExpertSystemEngine _engine = new();

        public Recommendation Evaluate(UserProfileDto profile)
            => _engine.Run(UserProfileMapper.ToDomain(profile));
    }
}
