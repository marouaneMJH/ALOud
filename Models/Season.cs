using System.ComponentModel.DataAnnotations;

namespace ALOud.Models
{
    public class Season
    {
        public Guid Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Name { get; set; } = string.Empty;

        // Navigation property
        public virtual ICollection<PerfumeSeason> PerfumeSeasons { get; set; } = new List<PerfumeSeason>();
    }
}
