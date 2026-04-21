using ALOud.Models;

namespace ALOud.Services.External.Shipping
{
    public interface IShippingService
    {
        /// <summary>
        /// Create a shipment for an order
        /// </summary>
        Task<ShipmentResult> CreateShipmentAsync(ShipmentRequest request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get shipping rates for an order
        /// </summary>
        Task<ShippingRatesResult> GetShippingRatesAsync(ShippingRatesRequest request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Track a shipment by tracking number
        /// </summary>
        Task<TrackingResult> TrackShipmentAsync(string trackingNumber, string carrier, CancellationToken cancellationToken = default);

        /// <summary>
        /// Cancel a shipment
        /// </summary>
        Task<ShipmentResult> CancelShipmentAsync(string shipmentId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Update shipment status
        /// </summary>
        Task<TrackingResult> UpdateShipmentStatusAsync(string shipmentId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get shipping label URL
        /// </summary>
        Task<LabelResult> GetShippingLabelAsync(string shipmentId, CancellationToken cancellationToken = default);
    }

    public class ShipmentRequest
    {
        public required Order Order { get; set; }
        public required string ShippingMethod { get; set; }
        public required string Carrier { get; set; }
        public bool RequiresSignature { get; set; }
        public bool RequiresAdultSignature { get; set; }
        public decimal InsuranceAmount { get; set; }
        public string? SpecialInstructions { get; set; }
        public Dictionary<string, string> Metadata { get; set; } = new();
    }

    public class ShippingRatesRequest
    {
        public required ShippingAddress FromAddress { get; set; }
        public required ShippingAddress ToAddress { get; set; }
        public required List<PackageInfo> Packages { get; set; }
        public DateTime? ShipDate { get; set; }
        public Dictionary<string, string> Metadata { get; set; } = new();
    }

    public class ShippingAddress
    {
        public required string Name { get; set; }
        public string? Company { get; set; }
        public required string AddressLine1 { get; set; }
        public string? AddressLine2 { get; set; }
        public required string City { get; set; }
        public required string State { get; set; }
        public required string PostalCode { get; set; }
        public required string Country { get; set; }
        public string? PhoneNumber { get; set; }
    }

    public class PackageInfo
    {
        public decimal WeightPounds { get; set; }
        public decimal Length { get; set; }
        public decimal Width { get; set; }
        public decimal Height { get; set; }
        public string PackageType { get; set; } = "Box";
        public decimal Value { get; set; }
    }

    public class ShipmentResult
    {
        public bool IsSuccess { get; set; }
        public string? ShipmentId { get; set; }
        public string? TrackingNumber { get; set; }
        public string? Status { get; set; }
        public decimal ShippingCost { get; set; }
        public string? ShippingLabelUrl { get; set; }
        public DateTime? EstimatedDeliveryDate { get; set; }
        public string? ErrorCode { get; set; }
        public string? ErrorMessage { get; set; }
        public DateTime CreatedAt { get; set; }
        public Dictionary<string, string> Metadata { get; set; } = new();
    }

    public class ShippingRatesResult
    {
        public bool IsSuccess { get; set; }
        public List<ShippingRate> Rates { get; set; } = new();
        public string? ErrorCode { get; set; }
        public string? ErrorMessage { get; set; }
        public Dictionary<string, string> Metadata { get; set; } = new();
    }

    public class ShippingRate
    {
        public required string Carrier { get; set; }
        public required string ServiceName { get; set; }
        public required string ServiceCode { get; set; }
        public decimal Rate { get; set; }
        public DateTime? EstimatedDeliveryDate { get; set; }
        public int DeliveryDays { get; set; }
        public bool GuaranteedService { get; set; }
        public Dictionary<string, string> Metadata { get; set; } = new();
    }

    public class TrackingResult
    {
        public bool IsSuccess { get; set; }
        public required string Status { get; set; } // "Prepared", "Shipped", "In_Transit", "Out_For_Delivery", "Delivered", "Exception"
        public string? TrackingNumber { get; set; }
        public string? Carrier { get; set; }
        public DateTime? EstimatedDeliveryDate { get; set; }
        public DateTime? ActualDeliveryDate { get; set; }
        public string? DeliverySignature { get; set; }
        public List<TrackingEvent> TrackingEvents { get; set; } = new();
        public string? ErrorCode { get; set; }
        public string? ErrorMessage { get; set; }
        public Dictionary<string, string> Metadata { get; set; } = new();
    }

    public class TrackingEvent
    {
        public DateTime Timestamp { get; set; }
        public required string Status { get; set; }
        public required string Description { get; set; }
        public string? Location { get; set; }
        public Dictionary<string, string> Metadata { get; set; } = new();
    }

    public class LabelResult
    {
        public bool IsSuccess { get; set; }
        public string? LabelUrl { get; set; }
        public string? Format { get; set; } // "PDF", "PNG", "ZPL"
        public string? ErrorCode { get; set; }
        public string? ErrorMessage { get; set; }
        public Dictionary<string, string> Metadata { get; set; } = new();
    }
}