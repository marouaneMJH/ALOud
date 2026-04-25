namespace ALOud.Models
{
    public class PerfumeTag
    {
        public Guid PerfumeId { get; set; }
        public virtual Perfume Perfume { get; set; } = null!;

        public Guid TagId { get; set; }
        public virtual Tag Tag { get; set; } = null!;
    }
}
