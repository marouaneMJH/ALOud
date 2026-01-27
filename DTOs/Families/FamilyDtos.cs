using System.ComponentModel.DataAnnotations;

namespace ALOud.DTOs.Families
{
    public class CreateFamilyDto
    {
        [Required(ErrorMessage = "Family name is required")]
        [MaxLength(150, ErrorMessage = "Family name cannot exceed 150 characters")]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string? Description { get; set; }
    }

    public class UpdateFamilyDto
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Family name is required")]
        [MaxLength(150, ErrorMessage = "Family name cannot exceed 150 characters")]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string? Description { get; set; }
    }

    public class FamilyDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int PerfumeCount { get; set; }
    }

    public class FamilySelectDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
