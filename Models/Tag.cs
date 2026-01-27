using System.ComponentModel.DataAnnotations;

namespace ALOud.Models
{
    public class Tag
    {
        public Guid Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        // Navigation property
        public virtual ICollection<PerfumeTag> PerfumeTags { get; set; } = new List<PerfumeTag>();
    }
}
