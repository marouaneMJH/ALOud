using System.ComponentModel.DataAnnotations;

namespace ALOud.Models
{
    public class Note
    {
        public Guid Id { get; set; }

        [Required]
        [MaxLength(150)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? Category { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        // Navigation property
        public virtual ICollection<PerfumeNote> PerfumeNotes { get; set; } = new List<PerfumeNote>();
    }
}
