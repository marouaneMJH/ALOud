using System.ComponentModel.DataAnnotations;

namespace ALOud.Models
{
    public class PerfumeAccord
    {
        public Guid PerfumeId { get; set; }
        public virtual Perfume Perfume { get; set; } = null!;

        public Guid AccordId { get; set; }
        public virtual Accord Accord { get; set; } = null!;

        [MaxLength(50)]
        public string? Intensity { get; set; } // e.g., "Strong", "Medium", "Light"
    }
}
