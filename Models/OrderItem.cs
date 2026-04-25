using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ALOud.Models
{
    [Table("OrderItems")]
    public class OrderItem
    {
        [Key]
        public Guid Id { get; set; }

        // Foreign key to Order
        [Required]
        public Guid OrderId { get; set; }

        // Product information (denormalized for order history)
        [Required]
        public Guid ProductId { get; set; }

        [Required]
        [MaxLength(200)]
        public required string ProductName { get; set; }

        [Required]
        [MaxLength(100)]
        public required string BrandName { get; set; }

        [MaxLength(50)]
        public string? ProductSku { get; set; }

        // Product details at time of order (prices can change)
        [Column(TypeName = "decimal(10,2)")]
        public decimal UnitPrice { get; set; }

        [Required]
        public int Quantity { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal LineTotal { get; set; } // UnitPrice * Quantity

        // Product attributes at time of order
        [MaxLength(20)]
        public string? Size { get; set; } // "30ml", "50ml", "100ml"

        [MaxLength(500)]
        public string? ProductDescription { get; set; }

        [MaxLength(500)]
        public string? ProductImageUrl { get; set; }

        // Item status for partial fulfillment
        [MaxLength(50)]
        public string Status { get; set; } = "Ordered"; // "Ordered", "Processing", "Shipped", "Delivered", "Cancelled", "Refunded"

        public int ShippedQuantity { get; set; } = 0;
        public int DeliveredQuantity { get; set; } = 0;
        public int CancelledQuantity { get; set; } = 0;
        public int RefundedQuantity { get; set; } = 0;

        // Timestamps for item lifecycle
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? ProcessedAt { get; set; }
        public DateTime? ShippedAt { get; set; }
        public DateTime? DeliveredAt { get; set; }
        public DateTime? CancelledAt { get; set; }

        // Notes and special handling
        [MaxLength(500)]
        public string? ItemNotes { get; set; }

        // Navigation properties
        [ForeignKey("OrderId")]
        public virtual Order Order { get; set; } = null!;

        [ForeignKey("ProductId")]
        public virtual Perfume Product { get; set; } = null!;

        // Computed properties
        [NotMapped]
        public int PendingQuantity => Quantity - ShippedQuantity - CancelledQuantity;

        [NotMapped]
        public bool IsFullyFulfilled => DeliveredQuantity >= Quantity;

        [NotMapped]
        public bool IsPartiallyCancelled => CancelledQuantity > 0 && CancelledQuantity < Quantity;

        [NotMapped]
        public bool IsFullyCancelled => CancelledQuantity >= Quantity;

        [NotMapped]
        public decimal RefundableAmount => (Quantity - RefundedQuantity) * UnitPrice;
    }
}