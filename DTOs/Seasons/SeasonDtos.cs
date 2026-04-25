using System.ComponentModel.DataAnnotations;

namespace ALOud.DTOs.Seasons
{
    public class CreateSeasonDto
    {
        [Required(ErrorMessage = "Season name is required")]
        [MaxLength(50, ErrorMessage = "Season name cannot exceed 50 characters")]
        public string Name { get; set; } = string.Empty;
    }

    public class UpdateSeasonDto
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Season name is required")]
        [MaxLength(50, ErrorMessage = "Season name cannot exceed 50 characters")]
        public string Name { get; set; } = string.Empty;
    }

    public class SeasonDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int PerfumeCount { get; set; }
    }

    public class SeasonSelectDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
