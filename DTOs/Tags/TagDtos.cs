using System.ComponentModel.DataAnnotations;

namespace ALOud.DTOs.Tags
{
    public class CreateTagDto
    {
        [Required(ErrorMessage = "Tag name is required")]
        [MaxLength(100, ErrorMessage = "Tag name cannot exceed 100 characters")]
        public string Name { get; set; } = string.Empty;
    }

    public class UpdateTagDto
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Tag name is required")]
        [MaxLength(100, ErrorMessage = "Tag name cannot exceed 100 characters")]
        public string Name { get; set; } = string.Empty;
    }

    public class TagDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int PerfumeCount { get; set; }
    }

    public class TagSelectDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
