using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ALOud.Models
{
    [Table("Checkouts")]
    public class Checkout
    {
        [Key]
        public Guid Id { get; set; }

        // User information (nullable for guest checkout)
        public Guid? UserId { get; set; }

        [Required]
        [MaxLength(100)]
        public required string Email { get; set; }

        // Cart identification
        [Required]
        [MaxLength(100)]
        public required string CartId { get; set; }

        // Checkout status
        [Required]
        [MaxLength(50)]
        public required string Status { get; set; } // "InProgress", "Completed", "Abandoned", "Failed"

        // Checkout steps completion
        public bool ShippingAddressCompleted { get; set; } = false;
        public bool BillingAddressCompleted { get; set; } = false;
        public bool ShippingMethodCompleted { get; set; } = false;
        public bool PaymentMethodCompleted { get; set; } = false;

        // Financial information
        [Column(TypeName = "decimal(10,2)")]
        public decimal SubtotalAmount { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal TaxAmount { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal ShippingAmount { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal DiscountAmount { get; set; } = 0;

        [Column(TypeName = "decimal(10,2)")]
        public decimal TotalAmount { get; set; }

        // Tax information
        [Column(TypeName = "decimal(5,4)")]
        public decimal TaxRate { get; set; } = 0.08m; // 8% flat rate

        // Shipping information
        [MaxLength(100)]
        public string? ShippingMethod { get; set; }

        // Payment information
        [MaxLength(100)]
        public string? PaymentMethod { get; set; }

        [MaxLength(200)]
        public string? PaymentIntentId { get; set; } // For Stripe integration

        // Guest checkout handling
        public bool IsGuestCheckout { get; set; } = false;
        public bool GuestPromptedForSignup { get; set; } = false;

        // Timestamps
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public DateTime? AbandonedAt { get; set; }

        // Session management
        public DateTime ExpiresAt { get; set; } // 30 minutes from creation

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual User? User { get; set; }

        public virtual ICollection<CheckoutAddress> CheckoutAddresses { get; set; } = new List<CheckoutAddress>();
        public virtual ICollection<StockReservation> StockReservations { get; set; } = new List<StockReservation>();
    }
}