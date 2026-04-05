using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ALOud.Models
{
    [Table("OrderPayments")]
    public class OrderPayment
    {
        [Key]
        public Guid Id { get; set; }

        // Foreign key to Order
        [Required]
        public Guid OrderId { get; set; }

        // Payment identification
        [Required]
        [MaxLength(100)]
        public required string PaymentReference { get; set; } // Internal payment reference

        [MaxLength(200)]
        public string? ExternalPaymentId { get; set; } // Stripe Payment Intent ID, PayPal Transaction ID

        [MaxLength(200)]
        public string? TransactionId { get; set; } // Bank/processor transaction ID

        // Payment information
        [Required]
        [MaxLength(50)]
        public required string PaymentMethod { get; set; } // "Credit_Card", "PayPal", "Bank_Transfer", "Cash"

        [MaxLength(100)]
        public string? PaymentProvider { get; set; } // "Stripe", "PayPal", "Square"

        [Required]
        [MaxLength(50)]
        public required string PaymentType { get; set; } // "Payment", "Refund", "Partial_Refund", "Chargeback"

        [Required]
        [MaxLength(50)]
        public required string Status { get; set; } // "Pending", "Processing", "Succeeded", "Failed", "Cancelled", "Refunded"

        // Financial details
        [Column(TypeName = "decimal(10,2)")]
        public decimal Amount { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal ProcessingFee { get; set; } = 0;

        [Column(TypeName = "decimal(10,2)")]
        public decimal NetAmount { get; set; } // Amount - ProcessingFee

        [Required]
        [MaxLength(3)]
        public required string Currency { get; set; } = "MAD";

        // Credit card information (if applicable)
        [MaxLength(20)]
        public string? CardLast4 { get; set; }

        [MaxLength(50)]
        public string? CardBrand { get; set; } // "Visa", "Mastercard", "Amex"

        [MaxLength(10)]
        public string? CardExpMonth { get; set; }

        [MaxLength(10)]
        public string? CardExpYear { get; set; }

        [MaxLength(200)]
        public string? CardFingerprint { get; set; }

        // Authorization information
        [MaxLength(100)]
        public string? AuthorizationCode { get; set; }

        [MaxLength(100)]
        public string? AvsResult { get; set; } // Address Verification System

        [MaxLength(100)]
        public string? CvvResult { get; set; } // CVV verification

        // Risk and fraud information
        [MaxLength(50)]
        public string? RiskLevel { get; set; } // "Low", "Medium", "High"

        [Column(TypeName = "decimal(5,2)")]
        public decimal? RiskScore { get; set; }

        [MaxLength(500)]
        public string? FraudDetails { get; set; }

        // Timestamps
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? ProcessedAt { get; set; }
        public DateTime? AuthorizedAt { get; set; }
        public DateTime? CapturedAt { get; set; }
        public DateTime? FailedAt { get; set; }
        public DateTime? RefundedAt { get; set; }

        // Failure information
        [MaxLength(100)]
        public string? FailureCode { get; set; }

        [MaxLength(500)]
        public string? FailureMessage { get; set; }

        // Refund information
        [Column(TypeName = "decimal(10,2)")]
        public decimal RefundedAmount { get; set; } = 0;

        [MaxLength(500)]
        public string? RefundReason { get; set; }

        public Guid? RefundedByUserId { get; set; }

        // Webhook and integration tracking
        [MaxLength(200)]
        public string? WebhookEventId { get; set; }

        public DateTime? WebhookProcessedAt { get; set; }

        // Additional metadata
        [MaxLength(2000)]
        public string? Metadata { get; set; } // JSON for additional provider-specific data

        [MaxLength(500)]
        public string? CustomerNote { get; set; }

        [MaxLength(500)]
        public string? InternalNote { get; set; }

        // Navigation properties
        [ForeignKey("OrderId")]
        public virtual Order Order { get; set; } = null!;

        [ForeignKey("RefundedByUserId")]
        public virtual User? RefundedByUser { get; set; }

        // Computed properties
        [NotMapped]
        public bool IsSuccessful => Status == "Succeeded";

        [NotMapped]
        public bool IsFailed => Status is "Failed" or "Cancelled";

        [NotMapped]
        public bool IsRefunded => RefundedAmount > 0;

        [NotMapped]
        public bool IsFullyRefunded => RefundedAmount >= Amount;

        [NotMapped]
        public decimal RefundableAmount => Amount - RefundedAmount;

        [NotMapped]
        public bool IsProcessing => Status is "Pending" or "Processing";
    }
}