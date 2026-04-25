using ALOud.DTOs;
using ALOud.Models;

namespace ALOud.Services
{
    public interface ICheckoutService
    {
        // Checkout lifecycle management
        Task<CheckoutSummaryDto> StartCheckoutAsync(StartCheckoutDto dto, string cartId);
        Task<CheckoutSummaryDto?> GetCheckoutAsync(Guid checkoutId);
        Task<CheckoutSummaryDto?> GetActiveCheckoutByCartIdAsync(string cartId);
        Task<CheckoutSummaryDto> UpdateCheckoutStepAsync(UpdateCheckoutStepDto dto);
        Task<CheckoutSummaryDto> CompleteCheckoutAsync(CompleteCheckoutDto dto);
        Task<bool> AbandonCheckoutAsync(Guid checkoutId);
        
        // Address management during checkout
        Task<CheckoutAddress> SetShippingAddressAsync(Guid checkoutId, CheckoutAddressDto dto);
        Task<CheckoutAddress> SetBillingAddressAsync(Guid checkoutId, CheckoutAddressDto dto);
        Task<CheckoutAddress?> GetCheckoutAddressAsync(Guid checkoutId, string addressType);
        Task<bool> CopyShippingToBillingAsync(Guid checkoutId);
        
        // Financial calculations
        Task<CheckoutSummaryDto> RecalculateCheckoutAsync(Guid checkoutId);
        Task<decimal> CalculateSubtotalAsync(Guid checkoutId);
        Task<decimal> CalculateTaxAsync(decimal subtotal, string state = "");
        Task<decimal> CalculateShippingAsync(Guid checkoutId, string shippingMethod);
        
        // Shipping and payment methods
        Task<CheckoutSummaryDto> SetShippingMethodAsync(Guid checkoutId, string shippingMethod);
        Task<CheckoutSummaryDto> SetPaymentMethodAsync(Guid checkoutId, string paymentMethod, string? paymentIntentId = null);
        Task<List<string>> GetAvailableShippingMethodsAsync(Guid checkoutId);
        Task<List<string>> GetAvailablePaymentMethodsAsync();
        
        // Session management
        Task<bool> ExtendCheckoutSessionAsync(Guid checkoutId, int additionalMinutes = 15);
        Task<bool> IsCheckoutValidAsync(Guid checkoutId);
        Task<int> CleanupExpiredCheckoutsAsync();
        
        // Guest checkout helpers
        Task<bool> PromptGuestForSignupAsync(Guid checkoutId);
        Task<User?> ConvertGuestToUserAsync(Guid checkoutId, CreateUserDto userDto);
        
        // Integration helpers
        Task<bool> ValidateCartIntegrityAsync(Guid checkoutId, string cartId);
        Task<List<StockReservation>> GetCheckoutStockReservationsAsync(Guid checkoutId);
    }
}