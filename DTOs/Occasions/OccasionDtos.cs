using System.ComponentModel.DataAnnotations;

namespace ALOud.DTOs.Occasions
{
    public class CreateOccasionDto
    {
        [Required(ErrorMessage = "Occasion name is required")]
        [MaxLength(100, ErrorMessage = "Occasion name cannot exceed 100 characters")]
        public string Name { get; set; } = string.Empty;
    }

    public class UpdateOccasionDto
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Occasion name is required")]
        [MaxLength(100, ErrorMessage = "Occasion name cannot exceed 100 characters")]
        public string Name { get; set; } = string.Empty;
    }

    public class OccasionDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int PerfumeCount { get; set; }
    }

    public class OccasionSelectDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
