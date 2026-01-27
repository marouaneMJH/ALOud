using System.ComponentModel.DataAnnotations;

namespace ALOud.DTOs.Accords
{
    public class CreateAccordDto
    {
        [Required(ErrorMessage = "Accord name is required")]
        [MaxLength(150, ErrorMessage = "Accord name cannot exceed 150 characters")]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string? Description { get; set; }
    }

    public class UpdateAccordDto
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Accord name is required")]
        [MaxLength(150, ErrorMessage = "Accord name cannot exceed 150 characters")]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string? Description { get; set; }
    }

    public class AccordDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int PerfumeCount { get; set; }
    }

    public class AccordSelectDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
