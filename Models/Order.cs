using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ALOud.Models
{
    [Table("Orders")]
    public class Order
    {
        [Key]
        public Guid Id { get; set; }

        // Order identification
        [Required]
        [MaxLength(30)]
        public required string OrderNumber { get; set; } // ALO-20260403-ABC12345

        // Link to original checkout
        [Required]
        public Guid CheckoutId { get; set; }

        // User information (nullable for guest orders)
        public Guid? UserId { get; set; }

        [Required]
        [EmailAddress]
        [MaxLength(255)]
        public required string CustomerEmail { get; set; }

        // Order status management
        [Required]
        [MaxLength(50)]
        public required string Status { get; set; } // "Pending", "Paid", "Processing", "Shipped", "Delivered", "Cancelled", "Refunded"

        // Financial information (copied from completed checkout)
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
        public decimal TaxRate { get; set; }

        // Shipping information
        [MaxLength(100)]
        public string? ShippingMethod { get; set; }

        // Payment information
        [MaxLength(100)]
        public string? PaymentMethod { get; set; }

        [MaxLength(200)]
        public string? PaymentIntentId { get; set; }

        [MaxLength(100)]
        public string? PaymentStatus { get; set; } // "Pending", "Paid", "Failed", "Refunded", "Partially_Refunded"

        // Shipping address (denormalized for order history)
        [Required]
        [MaxLength(100)]
        public required string ShippingFirstName { get; set; }

        [Required]
        [MaxLength(100)]
        public required string ShippingLastName { get; set; }

        [MaxLength(100)]
        public string? ShippingCompany { get; set; }

        [Required]
        [MaxLength(200)]
        public required string ShippingAddressLine1 { get; set; }

        [MaxLength(200)]
        public string? ShippingAddressLine2 { get; set; }

        [Required]
        [MaxLength(100)]
        public required string ShippingCity { get; set; }

        [Required]
        [MaxLength(100)]
        public required string ShippingState { get; set; }

        [Required]
        [MaxLength(20)]
        public required string ShippingPostalCode { get; set; }

        [Required]
        [MaxLength(3)]
        public required string ShippingCountry { get; set; }

        [MaxLength(20)]
        public string? ShippingPhoneNumber { get; set; }

        // Billing address (denormalized for order history)
        [Required]
        [MaxLength(100)]
        public required string BillingFirstName { get; set; }

        [Required]
        [MaxLength(100)]
        public required string BillingLastName { get; set; }

        [MaxLength(100)]
        public string? BillingCompany { get; set; }

        [Required]
        [MaxLength(200)]
        public required string BillingAddressLine1 { get; set; }

        [MaxLength(200)]
        public string? BillingAddressLine2 { get; set; }

        [Required]
        [MaxLength(100)]
        public required string BillingCity { get; set; }

        [Required]
        [MaxLength(100)]
        public required string BillingState { get; set; }

        [Required]
        [MaxLength(20)]
        public required string BillingPostalCode { get; set; }

        [Required]
        [MaxLength(3)]
        public required string BillingCountry { get; set; }

        [MaxLength(20)]
        public string? BillingPhoneNumber { get; set; }

        // Guest order handling
        public bool IsGuestOrder { get; set; } = false;

        // Special instructions and notes
        [MaxLength(1000)]
        public string? CustomerNotes { get; set; }

        [MaxLength(1000)]
        public string? InternalNotes { get; set; }

        // Timestamps
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? PaidAt { get; set; }
        public DateTime? ProcessedAt { get; set; }
        public DateTime? ShippedAt { get; set; }
        public DateTime? DeliveredAt { get; set; }
        public DateTime? CancelledAt { get; set; }

        // Fulfillment tracking
        [MaxLength(100)]
        public string? TrackingNumber { get; set; }

        [MaxLength(100)]
        public string? ShippingCarrier { get; set; }

        [MaxLength(500)]
        public string? TrackingUrl { get; set; }

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual User? User { get; set; }

        [ForeignKey("CheckoutId")]
        public virtual Checkout Checkout { get; set; } = null!;

        public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
        public virtual ICollection<OrderStatusHistory> StatusHistory { get; set; } = new List<OrderStatusHistory>();
        public virtual ICollection<OrderShipment> Shipments { get; set; } = new List<OrderShipment>();
        public virtual ICollection<OrderPayment> Payments { get; set; } = new List<OrderPayment>();

        // Computed properties
        [NotMapped]
        public int TotalItems => OrderItems.Sum(oi => oi.Quantity);

        [NotMapped]
        public string FullShippingAddress => $"{ShippingAddressLine1}{(!string.IsNullOrEmpty(ShippingAddressLine2) ? $", {ShippingAddressLine2}" : "")}, {ShippingCity}, {ShippingState} {ShippingPostalCode}, {ShippingCountry}";

        [NotMapped]
        public string CustomerFullName => $"{ShippingFirstName} {ShippingLastName}";

        [NotMapped]
        public bool IsCompleted => Status == "Delivered";

        [NotMapped]
        public bool IsCancellable => Status is "Pending" or "Paid" or "Processing";

        [NotMapped]
        public bool IsRefundable => Status is "Paid" or "Processing" or "Shipped" or "Delivered";
    }
}