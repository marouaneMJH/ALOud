using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ALOud.Models
{
    [Table("StockReservations")]
    public class StockReservation
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid CheckoutId { get; set; }

        [Required]
        public Guid PerfumeId { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }

        [Required]
        [MaxLength(50)]
        public required string Status { get; set; } // "Reserved", "Released", "Confirmed", "Expired"

        // Pricing information (captured at time of reservation)
        [Column(TypeName = "decimal(10,2)")]
        public decimal UnitPrice { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal TotalPrice { get; set; }

        // Timestamps
        public DateTime ReservedAt { get; set; }
        public DateTime ExpiresAt { get; set; } // 30 minutes from ReservedAt
        public DateTime? ReleasedAt { get; set; }
        public DateTime? ConfirmedAt { get; set; }

        // Source tracking
        [Required]
        [MaxLength(100)]
        public required string CartId { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Navigation properties
        [ForeignKey("CheckoutId")]
        public virtual Checkout Checkout { get; set; } = null!;

        [ForeignKey("PerfumeId")]
        public virtual Perfume Perfume { get; set; } = null!;
    }
}