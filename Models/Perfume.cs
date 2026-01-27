using System.ComponentModel.DataAnnotations;

namespace ALOud.Models
{
    public class Perfume
    {
        public Guid Id { get; set; }

        [Required]
        [MaxLength(300)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? Intensity { get; set; }

        [MaxLength(100)]
        public string? Longevity { get; set; }

        [MaxLength(100)]
        public string? Sillage { get; set; }

        [MaxLength(100)]
        public string? GenderProfile { get; set; }

        [MaxLength(100)]
        public string? PriceRange { get; set; }

        // Foreign key to Brand
        public Guid BrandId { get; set; }
        public virtual Brand Brand { get; set; } = null!;

        // Navigation properties for many-to-many relationships
        public virtual ICollection<PerfumeFamily> PerfumeFamilies { get; set; } = new List<PerfumeFamily>();
        public virtual ICollection<PerfumeNote> PerfumeNotes { get; set; } = new List<PerfumeNote>();
        public virtual ICollection<PerfumeAccord> PerfumeAccords { get; set; } = new List<PerfumeAccord>();
        public virtual ICollection<PerfumeTag> PerfumeTags { get; set; } = new List<PerfumeTag>();
        public virtual ICollection<PerfumeSeason> PerfumeSeasons { get; set; } = new List<PerfumeSeason>();
        public virtual ICollection<PerfumeOccasion> PerfumeOccasions { get; set; } = new List<PerfumeOccasion>();
    }
}
