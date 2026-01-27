using System.ComponentModel.DataAnnotations;

namespace ALOud.Models
{
    public class Brand
    {
        public Guid Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        // Navigation property
        public virtual ICollection<Perfume> Perfumes { get; set; } = new List<Perfume>();
    }
}
