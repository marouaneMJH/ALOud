using System.ComponentModel.DataAnnotations;

namespace ALOud.DTOs.Notes
{
    public class CreateNoteDto
    {
        [Required(ErrorMessage = "Note name is required")]
        [MaxLength(150, ErrorMessage = "Note name cannot exceed 150 characters")]
        public string Name { get; set; } = string.Empty;

        [MaxLength(100, ErrorMessage = "Category cannot exceed 100 characters")]
        public string? Category { get; set; }

        [MaxLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string? Description { get; set; }
    }

    public class UpdateNoteDto
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Note name is required")]
        [MaxLength(150, ErrorMessage = "Note name cannot exceed 150 characters")]
        public string Name { get; set; } = string.Empty;

        [MaxLength(100, ErrorMessage = "Category cannot exceed 100 characters")]
        public string? Category { get; set; }

        [MaxLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string? Description { get; set; }
    }

    public class NoteDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Category { get; set; }
        public string? Description { get; set; }
        public int PerfumeCount { get; set; }
    }

    public class NoteSelectDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Category { get; set; }
    }
}
