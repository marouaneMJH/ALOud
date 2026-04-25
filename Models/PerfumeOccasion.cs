namespace ALOud.Models
{
    public class PerfumeOccasion
    {
        public Guid PerfumeId { get; set; }
        public virtual Perfume Perfume { get; set; } = null!;

        public Guid OccasionId { get; set; }
        public virtual Occasion Occasion { get; set; } = null!;
    }
}
