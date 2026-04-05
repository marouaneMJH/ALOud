using ALOud.Models;
using ViewModels;

namespace ALOud.Services
{
    public interface IStockReservationService
    {
        // Stock reservation management
        Task<List<StockReservation>> ReserveStockAsync(Guid checkoutId, string cartId, List<CartItemVM> cartItems);
        Task<bool> ReleaseReservationsAsync(Guid checkoutId);
        Task<bool> ConfirmReservationsAsync(Guid checkoutId);
        Task<bool> ExtendReservationsAsync(Guid checkoutId, int additionalMinutes = 15);
        
        // Stock validation
        Task<bool> ValidateStockAvailabilityAsync(List<CartItemVM> cartItems);
        Task<Dictionary<Guid, int>> GetAvailableStockAsync(List<Guid> perfumeIds);
        Task<List<StockReservation>> GetActiveReservationsAsync(Guid perfumeId);
        
        // Reservation queries
        Task<List<StockReservation>> GetCheckoutReservationsAsync(Guid checkoutId);
        Task<decimal> CalculateReservationTotalAsync(Guid checkoutId);
        
        // Cleanup operations
        Task<int> CleanupExpiredReservationsAsync();
        Task<bool> IsStockReservedAsync(Guid perfumeId, int quantity);
        
        // Status management
        Task<bool> UpdateReservationStatusAsync(Guid reservationId, string status);
        Task<List<StockReservation>> GetReservationsByStatusAsync(string status);
    }
}