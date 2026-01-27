namespace ALOud.Models
{
    public class PerfumeFamily
    {
        public Guid PerfumeId { get; set; }
        public virtual Perfume Perfume { get; set; } = null!;

        public Guid FamilyId { get; set; }
        public virtual Family Family { get; set; } = null!;
    }
}
