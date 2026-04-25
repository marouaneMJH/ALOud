using System.ComponentModel.DataAnnotations;

namespace ALOud.Models
{
    public class Occasion
    {
        public Guid Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        // Navigation property
        public virtual ICollection<PerfumeOccasion> PerfumeOccasions { get; set; } = new List<PerfumeOccasion>();
    }
}
