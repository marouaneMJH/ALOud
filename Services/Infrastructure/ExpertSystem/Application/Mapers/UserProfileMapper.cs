using ALOud.DTOs.ExpertSystem;
using ALOud.Services.Infrastructure.ExpertSystem.Domain;

namespace ALOud.Services.Infrastructure.ExpertSystem.Mappers;

public static class UserProfileMapper
{
    public static UserProfile ToDomain(UserProfileDto dto)
    {
        return new UserProfile
        {
            Climate = dto.Climate,
            Occasion = dto.Occasion,
            SkinType = dto.SkinType,
            Compliment = dto.Compliment,
            SeasonPreference = dto.SeasonPreference,
            Persona = dto.Persona,
            Sensitivity = dto.Sensitivity,
            WantsLongPerformance = dto.WantsLongPerformance
        };
    }
}
