using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ALOud.Models
{
    [Table("OrderStatusHistory")]
    public class OrderStatusHistory
    {
        [Key]
        public Guid Id { get; set; }

        // Foreign key to Order
        [Required]
        public Guid OrderId { get; set; }

        // Status change information
        [MaxLength(50)]
        public string? PreviousStatus { get; set; }

        [Required]
        [MaxLength(50)]
        public required string NewStatus { get; set; }

        [Required]
        [MaxLength(100)]
        public required string ChangeReason { get; set; } // "Order placed", "Payment confirmed", "Shipped", etc.

        // Who made the change
        public Guid? ChangedByUserId { get; set; } // null for system changes

        [MaxLength(50)]
        public string ChangeSource { get; set; } = "System"; // "System", "Admin", "Customer", "API", "Webhook"

        // Additional context
        [MaxLength(1000)]
        public string? Notes { get; set; }

        [MaxLength(500)]
        public string? ExternalReference { get; set; } // Payment ID, tracking number, etc.

        // Notification tracking
        public bool CustomerNotified { get; set; } = false;
        public DateTime? NotificationSentAt { get; set; }

        [MaxLength(100)]
        public string? NotificationMethod { get; set; } // "Email", "SMS", "Push"

        // Timestamps
        public DateTime CreatedAt { get; set; }

        // Metadata for integrations
        [MaxLength(2000)]
        public string? Metadata { get; set; } // JSON for additional data

        // Navigation properties
        [ForeignKey("OrderId")]
        public virtual Order Order { get; set; } = null!;

        [ForeignKey("ChangedByUserId")]
        public virtual User? ChangedByUser { get; set; }
    }
}