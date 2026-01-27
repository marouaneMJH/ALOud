using System.ComponentModel.DataAnnotations;

namespace ALOud.Models
{
    public class PerfumeNote
    {
        public Guid PerfumeId { get; set; }
        public virtual Perfume Perfume { get; set; } = null!;

        public Guid NoteId { get; set; }
        public virtual Note Note { get; set; } = null!;

        [MaxLength(50)]
        public string? NoteLevel { get; set; } // e.g., "Top", "Middle", "Base"
    }
}
