using System.ComponentModel.DataAnnotations;
using ALOud.Services.Infrastructure.ExpertSystem.Domain;

namespace ALOud.DTOs.ExpertSystem
{
    public class UserProfileDto
    {
        [Required]
        public EClimate Climate { get; set; }
        public EOccasion Occasion { get; set; }
        public ESkinType SkinType { get; set; }
        public EComplimentDesire Compliment { get; set; }
        public ESeasonPreference SeasonPreference { get; set; }
        public EPersona Persona { get; set; }
        public ESensitivity Sensitivity { get; set; }

        public bool WantsLongPerformance { get; set; }

    }

}
