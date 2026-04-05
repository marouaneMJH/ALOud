using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ALOud.Models
{
    [Table("OrderShipments")]
    public class OrderShipment
    {
        [Key]
        public Guid Id { get; set; }

        // Foreign key to Order
        [Required]
        public Guid OrderId { get; set; }

        // Shipment identification
        [Required]
        [MaxLength(50)]
        public required string ShipmentNumber { get; set; } // Internal shipment ID

        [MaxLength(100)]
        public string? TrackingNumber { get; set; }

        [Required]
        [MaxLength(100)]
        public required string ShippingCarrier { get; set; } // "UPS", "FedEx", "DHL", "USPS"

        [Required]
        [MaxLength(100)]
        public required string ShippingMethod { get; set; } // "Standard", "Express", "Overnight"

        [MaxLength(500)]
        public string? TrackingUrl { get; set; }

        // Shipment status
        [Required]
        [MaxLength(50)]
        public required string Status { get; set; } // "Prepared", "Shipped", "In_Transit", "Out_For_Delivery", "Delivered", "Exception"

        // Shipping costs
        [Column(TypeName = "decimal(10,2)")]
        public decimal ShippingCost { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal InsuranceAmount { get; set; } = 0;

        // Package information
        [Column(TypeName = "decimal(8,2)")]
        public decimal WeightPounds { get; set; }

        [MaxLength(50)]
        public string? PackageType { get; set; } // "Box", "Envelope", "Tube"

        [MaxLength(100)]
        public string? Dimensions { get; set; } // "12x8x6 inches"

        // Shipping addresses (copied from order at time of shipment)
        [Required]
        [MaxLength(100)]
        public required string ShipToName { get; set; }

        [Required]
        [MaxLength(100)]
        public required string ShipToCompany { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public required string ShipToAddressLine1 { get; set; }

        [MaxLength(200)]
        public string? ShipToAddressLine2 { get; set; }

        [Required]
        [MaxLength(100)]
        public required string ShipToCity { get; set; }

        [Required]
        [MaxLength(100)]
        public required string ShipToState { get; set; }

        [Required]
        [MaxLength(20)]
        public required string ShipToPostalCode { get; set; }

        [Required]
        [MaxLength(3)]
        public required string ShipToCountry { get; set; }

        [MaxLength(20)]
        public string? ShipToPhoneNumber { get; set; }

        // Return/origin address
        [Required]
        [MaxLength(200)]
        public required string ShipFromAddressLine1 { get; set; }

        [MaxLength(200)]
        public string? ShipFromAddressLine2 { get; set; }

        [Required]
        [MaxLength(100)]
        public required string ShipFromCity { get; set; }

        [Required]
        [MaxLength(100)]
        public required string ShipFromState { get; set; }

        [Required]
        [MaxLength(20)]
        public required string ShipFromPostalCode { get; set; }

        [Required]
        [MaxLength(3)]
        public required string ShipFromCountry { get; set; }

        // Delivery information
        public DateTime? EstimatedDeliveryDate { get; set; }
        public DateTime? ActualDeliveryDate { get; set; }
        public DateTime? DeliveryAttemptDate { get; set; }

        [MaxLength(200)]
        public string? DeliverySignature { get; set; }

        [MaxLength(500)]
        public string? DeliveryNotes { get; set; }

        // Timestamps
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? ShippedAt { get; set; }
        public DateTime? DeliveredAt { get; set; }

        // Special handling
        [MaxLength(500)]
        public string? SpecialInstructions { get; set; }

        public bool RequiresSignature { get; set; } = false;
        public bool RequiresAdultSignature { get; set; } = false;
        public bool IsInsured { get; set; } = false;

        // Label and documentation
        [MaxLength(500)]
        public string? ShippingLabelUrl { get; set; }

        [MaxLength(500)]
        public string? CommercialInvoiceUrl { get; set; }

        // Navigation properties
        [ForeignKey("OrderId")]
        public virtual Order Order { get; set; } = null!;

        public virtual ICollection<OrderShipmentItem> ShipmentItems { get; set; } = new List<OrderShipmentItem>();

        // Computed properties
        [NotMapped]
        public bool IsDelivered => Status == "Delivered";

        [NotMapped]
        public bool IsInTransit => Status is "Shipped" or "In_Transit" or "Out_For_Delivery";

        [NotMapped]
        public int DaysInTransit => ShippedAt.HasValue ? (DateTime.UtcNow - ShippedAt.Value).Days : 0;

        [NotMapped]
        public string FullShipToAddress => $"{ShipToAddressLine1}{(!string.IsNullOrEmpty(ShipToAddressLine2) ? $", {ShipToAddressLine2}" : "")}, {ShipToCity}, {ShipToState} {ShipToPostalCode}, {ShipToCountry}";
    }
}