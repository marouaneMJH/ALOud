using System.ComponentModel.DataAnnotations;

namespace ALOud.DTOs
{
    public class OrderDto
    {
        public Guid Id { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public Guid? UserId { get; set; }
        public string CustomerEmail { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public bool IsGuestOrder { get; set; }

        // Financial information
        public decimal SubtotalAmount { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal ShippingAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal TaxRate { get; set; }

        // Methods
        public string? ShippingMethod { get; set; }
        public string? PaymentMethod { get; set; }
        public string? PaymentStatus { get; set; }

        // Shipping information
        public string ShippingFirstName { get; set; } = string.Empty;
        public string ShippingLastName { get; set; } = string.Empty;
        public string? ShippingCompany { get; set; }
        public string ShippingAddressLine1 { get; set; } = string.Empty;
        public string? ShippingAddressLine2 { get; set; }
        public string ShippingCity { get; set; } = string.Empty;
        public string ShippingState { get; set; } = string.Empty;
        public string ShippingPostalCode { get; set; } = string.Empty;
        public string ShippingCountry { get; set; } = string.Empty;
        public string? ShippingPhoneNumber { get; set; }

        // Tracking information
        public string? TrackingNumber { get; set; }
        public string? ShippingCarrier { get; set; }
        public string? TrackingUrl { get; set; }

        // Timestamps
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? PaidAt { get; set; }
        public DateTime? ProcessedAt { get; set; }
        public DateTime? ShippedAt { get; set; }
        public DateTime? DeliveredAt { get; set; }
        public DateTime? CancelledAt { get; set; }

        // Order items
        public List<OrderItemDto> Items { get; set; } = new List<OrderItemDto>();

        // Computed properties
        public int TotalItems => Items.Sum(i => i.Quantity);
        public string FullShippingAddress => $"{ShippingAddressLine1}{(!string.IsNullOrEmpty(ShippingAddressLine2) ? $", {ShippingAddressLine2}" : "")}, {ShippingCity}, {ShippingState} {ShippingPostalCode}, {ShippingCountry}";
        public string CustomerFullName => $"{ShippingFirstName} {ShippingLastName}";
        public bool IsCompleted => Status == "Delivered";
        public bool IsCancellable => Status is "Pending" or "Paid" or "Processing";
        public bool IsRefundable => Status is "Paid" or "Processing" or "Shipped" or "Delivered";
    }

    public class OrderItemDto
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string BrandName { get; set; } = string.Empty;
        public string? ProductSku { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public decimal LineTotal { get; set; }
        public string? Size { get; set; }
        public string? ProductImageUrl { get; set; }
        public string Status { get; set; } = string.Empty;
        public int ShippedQuantity { get; set; }
        public int DeliveredQuantity { get; set; }
        public int CancelledQuantity { get; set; }

        // Computed properties
        public int PendingQuantity => Quantity - ShippedQuantity - CancelledQuantity;
        public bool IsFullyFulfilled => DeliveredQuantity >= Quantity;
        public bool IsPartiallyCancelled => CancelledQuantity > 0 && CancelledQuantity < Quantity;
        public bool IsFullyCancelled => CancelledQuantity >= Quantity;
    }

    public class OrderSummaryDto
    {
        public Guid Id { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public int TotalItems { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ShippedAt { get; set; }
        public DateTime? DeliveredAt { get; set; }
        public string? TrackingNumber { get; set; }
        public bool IsCancellable { get; set; }
        public bool IsRefundable { get; set; }
    }

    public class CreateOrderDto
    {
        [Required]
        public Guid CheckoutId { get; set; }
    }

    public class UpdateOrderStatusDto
    {
        [Required]
        public Guid OrderId { get; set; }

        [Required]
        [MaxLength(50)]
        public required string NewStatus { get; set; }

        [Required]
        [MaxLength(100)]
        public required string ChangeReason { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }

        [MaxLength(500)]
        public string? ExternalReference { get; set; }

        public bool NotifyCustomer { get; set; } = true;
    }

    public class CancelOrderDto
    {
        [Required]
        public Guid OrderId { get; set; }

        [Required]
        [MaxLength(500)]
        public required string Reason { get; set; }

        public bool RefundPayment { get; set; } = true;
        public bool NotifyCustomer { get; set; } = true;

        [MaxLength(1000)]
        public string? CustomerNote { get; set; }
    }

    public class OrderTrackingDto
    {
        public Guid OrderId { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? TrackingNumber { get; set; }
        public string? ShippingCarrier { get; set; }
        public string? TrackingUrl { get; set; }
        public DateTime? ShippedAt { get; set; }
        public DateTime? EstimatedDeliveryDate { get; set; }
        public DateTime? DeliveredAt { get; set; }
        public List<OrderStatusHistoryDto> StatusHistory { get; set; } = new List<OrderStatusHistoryDto>();
    }

    public class OrderStatusHistoryDto
    {
        public Guid Id { get; set; }
        public string? PreviousStatus { get; set; }
        public string NewStatus { get; set; } = string.Empty;
        public string ChangeReason { get; set; } = string.Empty;
        public string ChangeSource { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public bool CustomerNotified { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class OrderSearchDto
    {
        public string? OrderNumber { get; set; }
        public string? CustomerEmail { get; set; }
        public string? Status { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public decimal? MinAmount { get; set; }
        public decimal? MaxAmount { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public string SortBy { get; set; } = "CreatedAt";
        public string SortDirection { get; set; } = "DESC";
    }

    public class OrderStatisticsDto
    {
        public int TotalOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public int PendingOrders { get; set; }
        public int ProcessingOrders { get; set; }
        public int ShippedOrders { get; set; }
        public int DeliveredOrders { get; set; }
        public int CancelledOrders { get; set; }
        public decimal AverageOrderValue { get; set; }
        public Dictionary<string, int> OrdersByStatus { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, decimal> RevenueByMonth { get; set; } = new Dictionary<string, decimal>();
    }

    public class ShipOrderDto
    {
        [Required]
        [MaxLength(100)]
        public required string TrackingNumber { get; set; }

        [Required]
        [MaxLength(100)]
        public required string Carrier { get; set; }

        [MaxLength(100)]
        public string? ShippingMethod { get; set; }
    }
}