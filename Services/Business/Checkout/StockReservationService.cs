using ALOud.Data;
using ALOud.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ViewModels;

namespace ALOud.Services
{
    public class StockReservationService : IStockReservationService
    {
        private readonly ALOudDbContext _context;
        private readonly ILogger<StockReservationService> _logger;

        public StockReservationService(ALOudDbContext context, ILogger<StockReservationService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<StockReservation>> ReserveStockAsync(Guid checkoutId, string cartId, List<CartItemVM> cartItems)
        {
            _logger.LogInformation("Reserving stock for checkout: {CheckoutId}, items: {ItemCount}", checkoutId, cartItems.Count);

            try
            {
                // Validate stock availability first
                if (!await ValidateStockAvailabilityAsync(cartItems))
                {
                    throw new InvalidOperationException("Insufficient stock available for one or more items");
                }

                var reservations = new List<StockReservation>();
                var expirationTime = DateTime.UtcNow.AddMinutes(30);

                foreach (var item in cartItems)
                {
                    // Get perfume details for pricing
                    var perfume = await _context.Perfumes.FindAsync(item.ProductId);
                    if (perfume == null)
                    {
                        throw new ArgumentException($"Perfume {item.ProductId} not found");
                    }

                    var reservation = new StockReservation
                    {
                        Id = Guid.NewGuid(),
                        CheckoutId = checkoutId,
                        PerfumeId = item.ProductId,
                        Quantity = item.Quantity,
                        Status = "Reserved",
                        UnitPrice = perfume.Price, // Use current price
                        TotalPrice = perfume.Price * item.Quantity,
                        CartId = cartId,
                        ReservedAt = DateTime.UtcNow,
                        ExpiresAt = expirationTime,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    reservations.Add(reservation);
                    _context.StockReservations.Add(reservation);

                    _logger.LogInformation("Reserved {Quantity} units of perfume {PerfumeId} for checkout {CheckoutId}", 
                        item.Quantity, item.ProductId, checkoutId);
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("Successfully reserved stock for checkout: {CheckoutId}", checkoutId);
                return reservations;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reserving stock for checkout: {CheckoutId}", checkoutId);
                throw;
            }
        }

        public async Task<bool> ReleaseReservationsAsync(Guid checkoutId)
        {
            _logger.LogInformation("Releasing reservations for checkout: {CheckoutId}", checkoutId);

            try
            {
                var reservations = await _context.StockReservations
                    .Where(r => r.CheckoutId == checkoutId && r.Status == "Reserved")
                    .ToListAsync();

                foreach (var reservation in reservations)
                {
                    reservation.Status = "Released";
                    reservation.ReleasedAt = DateTime.UtcNow;
                    reservation.UpdatedAt = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("Released {Count} reservations for checkout: {CheckoutId}", reservations.Count, checkoutId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error releasing reservations for checkout: {CheckoutId}", checkoutId);
                throw;
            }
        }

        public async Task<bool> ConfirmReservationsAsync(Guid checkoutId)
        {
            _logger.LogInformation("Confirming reservations for checkout: {CheckoutId}", checkoutId);

            try
            {
                var reservations = await _context.StockReservations
                    .Where(r => r.CheckoutId == checkoutId && r.Status == "Reserved")
                    .ToListAsync();

                foreach (var reservation in reservations)
                {
                    reservation.Status = "Confirmed";
                    reservation.ConfirmedAt = DateTime.UtcNow;
                    reservation.UpdatedAt = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("Confirmed {Count} reservations for checkout: {CheckoutId}", reservations.Count, checkoutId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error confirming reservations for checkout: {CheckoutId}", checkoutId);
                throw;
            }
        }

        public async Task<bool> ExtendReservationsAsync(Guid checkoutId, int additionalMinutes = 15)
        {
            _logger.LogInformation("Extending reservations for checkout: {CheckoutId} by {Minutes} minutes", checkoutId, additionalMinutes);

            try
            {
                var reservations = await _context.StockReservations
                    .Where(r => r.CheckoutId == checkoutId && r.Status == "Reserved")
                    .ToListAsync();

                foreach (var reservation in reservations)
                {
                    reservation.ExpiresAt = reservation.ExpiresAt.AddMinutes(additionalMinutes);
                    reservation.UpdatedAt = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("Extended {Count} reservations for checkout: {CheckoutId}", reservations.Count, checkoutId);
                return reservations.Count > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extending reservations for checkout: {CheckoutId}", checkoutId);
                throw;
            }
        }

        public async Task<bool> ValidateStockAvailabilityAsync(List<CartItemVM> cartItems)
        {
            _logger.LogInformation("Validating stock availability for {ItemCount} items", cartItems.Count);

            try
            {
                foreach (var item in cartItems)
                {
                    var perfume = await _context.Perfumes.FindAsync(item.ProductId);
                    if (perfume == null)
                    {
                        _logger.LogWarning("Perfume {PerfumeId} not found", item.ProductId);
                        return false;
                    }

                    // Check available stock (assuming Stock field exists on Perfume)
                    // For now, we'll use a simple availability check
                    var currentReserved = await _context.StockReservations
                        .Where(r => r.PerfumeId == item.ProductId && 
                                   r.Status == "Reserved" && 
                                   r.ExpiresAt > DateTime.UtcNow)
                        .SumAsync(r => r.Quantity);

                    var availableStock = (perfume.StockQuantity) - currentReserved;

                    if (availableStock < item.Quantity)
                    {
                        _logger.LogWarning("Insufficient stock for perfume {PerfumeId}. Available: {Available}, Requested: {Requested}", 
                            item.ProductId, availableStock, item.Quantity);
                        return false;
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating stock availability");
                throw;
            }
        }

        public async Task<Dictionary<Guid, int>> GetAvailableStockAsync(List<Guid> perfumeIds)
        {
            try
            {
                var stockLevels = new Dictionary<Guid, int>();

                foreach (var perfumeId in perfumeIds)
                {
                    var perfume = await _context.Perfumes.FindAsync(perfumeId);
                    if (perfume == null) continue;

                    var currentReserved = await _context.StockReservations
                        .Where(r => r.PerfumeId == perfumeId && 
                                   r.Status == "Reserved" && 
                                   r.ExpiresAt > DateTime.UtcNow)
                        .SumAsync(r => r.Quantity);

                    var availableStock = (perfume.StockQuantity) - currentReserved;
                    stockLevels[perfumeId] = Math.Max(0, availableStock);
                }

                return stockLevels;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting available stock levels");
                throw;
            }
        }

        public async Task<List<StockReservation>> GetActiveReservationsAsync(Guid perfumeId)
        {
            try
            {
                return await _context.StockReservations
                    .Where(r => r.PerfumeId == perfumeId && 
                               r.Status == "Reserved" && 
                               r.ExpiresAt > DateTime.UtcNow)
                    .Include(r => r.Checkout)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active reservations for perfume: {PerfumeId}", perfumeId);
                throw;
            }
        }

        public async Task<List<StockReservation>> GetCheckoutReservationsAsync(Guid checkoutId)
        {
            try
            {
                return await _context.StockReservations
                    .Where(r => r.CheckoutId == checkoutId)
                    .Include(r => r.Perfume)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting reservations for checkout: {CheckoutId}", checkoutId);
                throw;
            }
        }

        public async Task<decimal> CalculateReservationTotalAsync(Guid checkoutId)
        {
            try
            {
                return await _context.StockReservations
                    .Where(r => r.CheckoutId == checkoutId && r.Status == "Reserved")
                    .SumAsync(r => r.TotalPrice);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating reservation total for checkout: {CheckoutId}", checkoutId);
                throw;
            }
        }

        public async Task<int> CleanupExpiredReservationsAsync()
        {
            _logger.LogInformation("Cleaning up expired reservations");

            try
            {
                var expiredReservations = await _context.StockReservations
                    .Where(r => r.Status == "Reserved" && r.ExpiresAt <= DateTime.UtcNow)
                    .ToListAsync();

                foreach (var reservation in expiredReservations)
                {
                    reservation.Status = "Expired";
                    reservation.ReleasedAt = DateTime.UtcNow;
                    reservation.UpdatedAt = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("Cleaned up {Count} expired reservations", expiredReservations.Count);
                return expiredReservations.Count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cleaning up expired reservations");
                throw;
            }
        }

        public async Task<bool> IsStockReservedAsync(Guid perfumeId, int quantity)
        {
            try
            {
                var availableStock = await GetAvailableStockAsync(new List<Guid> { perfumeId });
                return availableStock.ContainsKey(perfumeId) && availableStock[perfumeId] >= quantity;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking stock reservation for perfume: {PerfumeId}", perfumeId);
                throw;
            }
        }

        public async Task<bool> UpdateReservationStatusAsync(Guid reservationId, string status)
        {
            try
            {
                var reservation = await _context.StockReservations.FindAsync(reservationId);
                if (reservation == null) return false;

                reservation.Status = status;
                reservation.UpdatedAt = DateTime.UtcNow;

                if (status == "Released" || status == "Expired")
                {
                    reservation.ReleasedAt = DateTime.UtcNow;
                }
                else if (status == "Confirmed")
                {
                    reservation.ConfirmedAt = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating reservation status for: {ReservationId}", reservationId);
                throw;
            }
        }

        public async Task<List<StockReservation>> GetReservationsByStatusAsync(string status)
        {
            try
            {
                return await _context.StockReservations
                    .Where(r => r.Status == status)
                    .Include(r => r.Perfume)
                    .Include(r => r.Checkout)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting reservations by status: {Status}", status);
                throw;
            }
        }
    }
}