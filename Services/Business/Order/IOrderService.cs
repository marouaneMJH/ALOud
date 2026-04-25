using ALOud.DTOs;
using ALOud.Models;

namespace ALOud.Services
{
    public interface IOrderService
    {
        // Order creation and management
        Task<OrderDto> CreateOrderFromCheckoutAsync(Guid checkoutId);
        Task<OrderDto?> GetOrderAsync(Guid orderId);
        Task<OrderDto?> GetOrderByNumberAsync(string orderNumber);
        Task<List<OrderSummaryDto>> GetUserOrdersAsync(Guid userId, int page = 1, int pageSize = 20);
        Task<List<OrderDto>> GetFullUserOrdersAsync(Guid userId, int page = 1, int pageSize = 20);
        Task<List<OrderSummaryDto>> SearchOrdersAsync(OrderSearchDto searchDto);
        
        // Order status management
        Task<bool> UpdateOrderStatusAsync(UpdateOrderStatusDto dto, Guid? updatedByUserId = null);
        Task<bool> CancelOrderAsync(CancelOrderDto dto, Guid? cancelledByUserId = null);
        Task<OrderTrackingDto?> GetOrderTrackingAsync(Guid orderId);
        Task<List<OrderStatusHistoryDto>> GetOrderStatusHistoryAsync(Guid orderId);
        
        // Order fulfillment
        Task<bool> MarkOrderAsPaidAsync(Guid orderId, string paymentIntentId, decimal paidAmount);
        Task<bool> MarkOrderAsProcessingAsync(Guid orderId, string? processingNotes = null);
        Task<bool> MarkOrderAsShippedAsync(Guid orderId, string trackingNumber, string carrier, string? shippingMethod = null);
        Task<bool> MarkOrderAsDeliveredAsync(Guid orderId, DateTime? deliveredAt = null, string? deliveryNotes = null);
        
        // Order modifications
        Task<bool> UpdateShippingAddressAsync(Guid orderId, CheckoutAddressDto shippingAddress);
        Task<bool> AddOrderNoteAsync(Guid orderId, string note, bool isInternal = false);
        
        // Business analytics
        Task<OrderStatisticsDto> GetOrderStatisticsAsync(DateTime? startDate = null, DateTime? endDate = null);
        Task<int> GetPendingOrdersCountAsync();
        Task<decimal> GetTotalRevenueAsync(DateTime? startDate = null, DateTime? endDate = null);
        
        // Customer service helpers
        Task<List<OrderSummaryDto>> GetOrdersByEmailAsync(string email);
        Task<bool> CanCancelOrderAsync(Guid orderId);
        Task<bool> CanRefundOrderAsync(Guid orderId);
        Task<decimal> GetRefundableAmountAsync(Guid orderId);
        
        // Integration helpers
        Task<bool> ProcessStockDeductionAsync(Guid orderId);
        Task<bool> RestoreStockAsync(Guid orderId, string reason);
        Task<List<Guid>> GetLowStockProductsFromOrdersAsync();
        
        // Order number generation
        string GenerateOrderNumber();
        
        // Validation helpers
        Task<bool> ValidateOrderOwnershipAsync(Guid orderId, Guid userId);
        Task<bool> IsValidOrderStatusTransitionAsync(string currentStatus, string newStatus);
    }
}