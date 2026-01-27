namespace ALOud.Models
{
    public class PerfumeSeason
    {
        public Guid PerfumeId { get; set; }
        public virtual Perfume Perfume { get; set; } = null!;

        public Guid SeasonId { get; set; }
        public virtual Season Season { get; set; } = null!;
    }
}
