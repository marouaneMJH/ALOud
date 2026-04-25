using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ALOud.Models
{
    [Table("OrderShipmentItems")]
    public class OrderShipmentItem
    {
        [Key]
        public Guid Id { get; set; }

        // Foreign keys
        [Required]
        public Guid OrderShipmentId { get; set; }

        [Required]
        public Guid OrderItemId { get; set; }

        // Quantity information
        [Required]
        public int Quantity { get; set; }

        // Item condition and tracking
        [MaxLength(50)]
        public string Status { get; set; } = "Prepared"; // "Prepared", "Shipped", "Delivered", "Exception"

        [MaxLength(500)]
        public string? Notes { get; set; }

        // Timestamps
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Navigation properties
        [ForeignKey("OrderShipmentId")]
        public virtual OrderShipment OrderShipment { get; set; } = null!;

        [ForeignKey("OrderItemId")]
        public virtual OrderItem OrderItem { get; set; } = null!;
    }
}