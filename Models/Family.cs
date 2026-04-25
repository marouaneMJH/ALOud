using System.ComponentModel.DataAnnotations;

namespace ALOud.Models
{
    public class Family
    {
        public Guid Id { get; set; }

        [Required]
        [MaxLength(150)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        // Navigation property
        public virtual ICollection<PerfumeFamily> PerfumeFamilies { get; set; } = new List<PerfumeFamily>();
    }
}
