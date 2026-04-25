using ALOud.Data;
using ALOud.DTOs;
using ALOud.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ViewModels;

namespace ALOud.Services
{
    public class CheckoutService : ICheckoutService
    {
        private readonly ALOudDbContext _context;
        private readonly IStockReservationService _stockService;
        private readonly ICartService _cartService;
        private readonly IUserService _userService;
        private readonly IOrderService _orderService;
        private readonly ILogger<CheckoutService> _logger;

        // Tax rate configuration (8% flat rate)
        private const decimal TAX_RATE = 0.08m;

        public CheckoutService(
            ALOudDbContext context,
            IStockReservationService stockService,
            ICartService cartService,
            IUserService userService,
            IOrderService orderService,
            ILogger<CheckoutService> logger)
        {
            _context = context;
            _stockService = stockService;
            _cartService = cartService;
            _userService = userService;
            _orderService = orderService;
            _logger = logger;
        }

        public async Task<CheckoutSummaryDto> StartCheckoutAsync(StartCheckoutDto dto, string cartId)
        {
            _logger.LogInformation("Starting checkout for cart: {CartId}, email: {Email}", cartId, dto.Email);

            try
            {
                // Check if there's already an active checkout for this cart
                var existingCheckout = await GetActiveCheckoutByCartIdAsync(cartId);
                if (existingCheckout != null)
                {
                    _logger.LogInformation("Found existing active checkout: {CheckoutId}", existingCheckout.Id);
                    return existingCheckout;
                }

                // Get cart items
                var cartItems = await _cartService.GetCartAsync();
                if (!cartItems.Any())
                {
                    throw new InvalidOperationException("Cannot start checkout with empty cart");
                }

                // Validate stock availability
                if (!await _stockService.ValidateStockAvailabilityAsync(cartItems))
                {
                    throw new InvalidOperationException("Insufficient stock for one or more items in cart");
                }

                // Determine if this is a guest checkout or authenticated user
                User? user = null;
                bool isGuestCheckout = dto.IsGuestCheckout;

                if (!string.IsNullOrEmpty(dto.Email))
                {
                    user = await _userService.GetByEmailAsync(dto.Email);
                    if (user != null)
                    {
                        isGuestCheckout = false;
                    }
                }

                // Create checkout record
                var checkout = new Checkout
                {
                    Id = Guid.NewGuid(),
                    UserId = user?.Id,
                    Email = dto.Email ?? "guest@aloud.ma", // Use provided email or fallback
                    CartId = cartId,
                    Status = "InProgress",
                    IsGuestCheckout = isGuestCheckout,
                    TaxRate = TAX_RATE,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(30)
                };

                _context.Checkouts.Add(checkout);
                await _context.SaveChangesAsync();

                // Reserve stock for checkout
                await _stockService.ReserveStockAsync(checkout.Id, cartId, cartItems);

                // Calculate initial totals
                var checkoutSummary = await RecalculateCheckoutAsync(checkout.Id);

                _logger.LogInformation("Checkout started successfully: {CheckoutId}", checkout.Id);
                return checkoutSummary;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting checkout for cart: {CartId}", cartId);
                throw;
            }
        }

        public async Task<CheckoutSummaryDto?> GetCheckoutAsync(Guid checkoutId)
        {
            try
            {
                var checkout = await _context.Checkouts
                    .Include(c => c.CheckoutAddresses)
                    .Include(c => c.StockReservations)
                    .FirstOrDefaultAsync(c => c.Id == checkoutId);

                return checkout != null ? MapToSummaryDto(checkout) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting checkout: {CheckoutId}", checkoutId);
                throw;
            }
        }

        public async Task<CheckoutSummaryDto?> GetActiveCheckoutByCartIdAsync(string cartId)
        {
            try
            {
                var checkout = await _context.Checkouts
                    .Include(c => c.CheckoutAddresses)
                    .Include(c => c.StockReservations)
                    .FirstOrDefaultAsync(c => c.CartId == cartId && 
                                            c.Status == "InProgress" && 
                                            c.ExpiresAt > DateTime.UtcNow);

                return checkout != null ? MapToSummaryDto(checkout) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active checkout for cart: {CartId}", cartId);
                throw;
            }
        }

        public async Task<CheckoutSummaryDto> UpdateCheckoutStepAsync(UpdateCheckoutStepDto dto)
        {
            _logger.LogInformation("Updating checkout steps for: {CheckoutId}", dto.CheckoutId);

            try
            {
                var checkout = await _context.Checkouts.FindAsync(dto.CheckoutId);
                if (checkout == null)
                {
                    throw new ArgumentException($"Checkout {dto.CheckoutId} not found");
                }

                checkout.ShippingAddressCompleted = dto.ShippingAddressCompleted;
                checkout.BillingAddressCompleted = dto.BillingAddressCompleted;
                checkout.ShippingMethodCompleted = dto.ShippingMethodCompleted;
                checkout.PaymentMethodCompleted = dto.PaymentMethodCompleted;
                checkout.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return await RecalculateCheckoutAsync(dto.CheckoutId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating checkout steps: {CheckoutId}", dto.CheckoutId);
                throw;
            }
        }

        public async Task<CheckoutSummaryDto> CompleteCheckoutAsync(CompleteCheckoutDto dto)
        {
            _logger.LogInformation("Completing checkout: {CheckoutId}", dto.CheckoutId);

            try
            {
                var checkout = await _context.Checkouts
                    .Include(c => c.CheckoutAddresses)
                    .Include(c => c.StockReservations)
                    .FirstOrDefaultAsync(c => c.Id == dto.CheckoutId);

                if (checkout == null)
                {
                    throw new ArgumentException($"Checkout {dto.CheckoutId} not found");
                }

                // Validate checkout is ready to complete
                if (!checkout.ShippingAddressCompleted || !checkout.BillingAddressCompleted ||
                    !checkout.ShippingMethodCompleted || !checkout.PaymentMethodCompleted)
                {
                    throw new InvalidOperationException("Checkout is not ready to complete - missing required steps");
                }

                // Update checkout status and payment info
                checkout.Status = "Completed";
                checkout.PaymentMethod = dto.PaymentMethod;
                checkout.PaymentIntentId = dto.PaymentIntentId;
                checkout.CompletedAt = DateTime.UtcNow;
                checkout.UpdatedAt = DateTime.UtcNow;

                // Confirm stock reservations
                await _stockService.ConfirmReservationsAsync(dto.CheckoutId);

                // Create order from completed checkout
                var order = await _orderService.CreateOrderFromCheckoutAsync(dto.CheckoutId);
                _logger.LogInformation("Created order {OrderId} from completed checkout {CheckoutId}", order.Id, dto.CheckoutId);

                await _context.SaveChangesAsync();

                _logger.LogInformation("Checkout completed successfully: {CheckoutId}", dto.CheckoutId);
                return MapToSummaryDto(checkout);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error completing checkout: {CheckoutId}", dto.CheckoutId);
                throw;
            }
        }

        public async Task<bool> AbandonCheckoutAsync(Guid checkoutId)
        {
            _logger.LogInformation("Abandoning checkout: {CheckoutId}", checkoutId);

            try
            {
                var checkout = await _context.Checkouts.FindAsync(checkoutId);
                if (checkout == null) return false;

                checkout.Status = "Abandoned";
                checkout.AbandonedAt = DateTime.UtcNow;
                checkout.UpdatedAt = DateTime.UtcNow;

                // Release stock reservations
                await _stockService.ReleaseReservationsAsync(checkoutId);

                await _context.SaveChangesAsync();

                _logger.LogInformation("Checkout abandoned successfully: {CheckoutId}", checkoutId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error abandoning checkout: {CheckoutId}", checkoutId);
                throw;
            }
        }

        public async Task<CheckoutAddress> SetShippingAddressAsync(Guid checkoutId, CheckoutAddressDto dto)
        {
            return await SetCheckoutAddressAsync(checkoutId, dto, "Shipping");
        }

        public async Task<CheckoutAddress> SetBillingAddressAsync(Guid checkoutId, CheckoutAddressDto dto)
        {
            return await SetCheckoutAddressAsync(checkoutId, dto, "Billing");
        }

        public async Task<CheckoutAddress?> GetCheckoutAddressAsync(Guid checkoutId, string addressType)
        {
            try
            {
                return await _context.CheckoutAddresses
                    .FirstOrDefaultAsync(a => a.CheckoutId == checkoutId && a.AddressType == addressType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting checkout address: {CheckoutId}, type: {AddressType}", checkoutId, addressType);
                throw;
            }
        }

        public async Task<bool> CopyShippingToBillingAsync(Guid checkoutId)
        {
            _logger.LogInformation("Copying shipping to billing address for checkout: {CheckoutId}", checkoutId);

            try
            {
                var shippingAddress = await GetCheckoutAddressAsync(checkoutId, "Shipping");
                if (shippingAddress == null)
                {
                    throw new InvalidOperationException("No shipping address found to copy");
                }

                // Remove existing billing address if any
                var existingBilling = await GetCheckoutAddressAsync(checkoutId, "Billing");
                if (existingBilling != null)
                {
                    _context.CheckoutAddresses.Remove(existingBilling);
                }

                // Create new billing address from shipping
                var billingAddress = new CheckoutAddress
                {
                    Id = Guid.NewGuid(),
                    CheckoutId = checkoutId,
                    AddressType = "Billing",
                    FirstName = shippingAddress.FirstName,
                    LastName = shippingAddress.LastName,
                    Company = shippingAddress.Company,
                    AddressLine1 = shippingAddress.AddressLine1,
                    AddressLine2 = shippingAddress.AddressLine2,
                    City = shippingAddress.City,
                    State = shippingAddress.State,
                    PostalCode = shippingAddress.PostalCode,
                    Country = shippingAddress.Country,
                    PhoneNumber = shippingAddress.PhoneNumber,
                    Email = shippingAddress.Email,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.CheckoutAddresses.Add(billingAddress);

                // Update checkout step completion
                var checkout = await _context.Checkouts.FindAsync(checkoutId);
                if (checkout != null)
                {
                    checkout.BillingAddressCompleted = true;
                    checkout.UpdatedAt = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error copying shipping to billing for checkout: {CheckoutId}", checkoutId);
                throw;
            }
        }

        public async Task<CheckoutSummaryDto> RecalculateCheckoutAsync(Guid checkoutId)
        {
            _logger.LogInformation("Recalculating checkout totals: {CheckoutId}", checkoutId);

            try
            {
                var checkout = await _context.Checkouts
                    .Include(c => c.CheckoutAddresses)
                    .Include(c => c.StockReservations)
                    .FirstOrDefaultAsync(c => c.Id == checkoutId);

                if (checkout == null)
                {
                    throw new ArgumentException($"Checkout {checkoutId} not found");
                }

                // Calculate subtotal from stock reservations
                checkout.SubtotalAmount = await CalculateSubtotalAsync(checkoutId);
                
                // Calculate tax based on billing/shipping address
                var billingAddress = checkout.CheckoutAddresses.FirstOrDefault(a => a.AddressType == "Billing");
                var shippingAddress = checkout.CheckoutAddresses.FirstOrDefault(a => a.AddressType == "Shipping");
                var taxState = billingAddress?.State ?? shippingAddress?.State ?? "";
                
                checkout.TaxAmount = await CalculateTaxAsync(checkout.SubtotalAmount, taxState);
                
                // Calculate shipping if method is set
                if (!string.IsNullOrEmpty(checkout.ShippingMethod))
                {
                    checkout.ShippingAmount = await CalculateShippingAsync(checkoutId, checkout.ShippingMethod);
                }

                // Calculate total
                checkout.TotalAmount = checkout.SubtotalAmount + checkout.TaxAmount + checkout.ShippingAmount - checkout.DiscountAmount;
                checkout.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return MapToSummaryDto(checkout);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recalculating checkout: {CheckoutId}", checkoutId);
                throw;
            }
        }

        public async Task<decimal> CalculateSubtotalAsync(Guid checkoutId)
        {
            try
            {
                return await _stockService.CalculateReservationTotalAsync(checkoutId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating subtotal for checkout: {CheckoutId}", checkoutId);
                throw;
            }
        }

        public async Task<decimal> CalculateTaxAsync(decimal subtotal, string state = "")
        {
            // Simple flat rate tax calculation for now
            // In a real implementation, you'd integrate with a tax service like TaxJar or Avalara
            try
            {
                return await Task.FromResult(subtotal * TAX_RATE);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating tax for subtotal: {Subtotal}", subtotal);
                throw;
            }
        }

        public async Task<decimal> CalculateShippingAsync(Guid checkoutId, string shippingMethod)
        {
            // Simple shipping calculation - in real implementation, integrate with shipping providers
            try
            {
                return await Task.FromResult(shippingMethod.ToLower() switch
                {
                    "standard" => 5.99m,
                    "express" => 12.99m,
                    "overnight" => 24.99m,
                    "free" => 0.00m,
                    _ => 5.99m
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating shipping for checkout: {CheckoutId}", checkoutId);
                throw;
            }
        }

        public async Task<CheckoutSummaryDto> SetShippingMethodAsync(Guid checkoutId, string shippingMethod)
        {
            _logger.LogInformation("Setting shipping method for checkout: {CheckoutId}, method: {Method}", checkoutId, shippingMethod);

            try
            {
                var checkout = await _context.Checkouts.FindAsync(checkoutId);
                if (checkout == null)
                {
                    throw new ArgumentException($"Checkout {checkoutId} not found");
                }

                checkout.ShippingMethod = shippingMethod;
                checkout.ShippingMethodCompleted = true;
                checkout.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return await RecalculateCheckoutAsync(checkoutId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting shipping method for checkout: {CheckoutId}", checkoutId);
                throw;
            }
        }

        public async Task<CheckoutSummaryDto> SetPaymentMethodAsync(Guid checkoutId, string paymentMethod, string? paymentIntentId = null)
        {
            _logger.LogInformation("Setting payment method for checkout: {CheckoutId}, method: {Method}", checkoutId, paymentMethod);

            try
            {
                var checkout = await _context.Checkouts.FindAsync(checkoutId);
                if (checkout == null)
                {
                    throw new ArgumentException($"Checkout {checkoutId} not found");
                }

                checkout.PaymentMethod = paymentMethod;
                checkout.PaymentIntentId = paymentIntentId;
                checkout.PaymentMethodCompleted = true;
                checkout.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return await RecalculateCheckoutAsync(checkoutId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting payment method for checkout: {CheckoutId}", checkoutId);
                throw;
            }
        }

        public async Task<List<string>> GetAvailableShippingMethodsAsync(Guid checkoutId)
        {
            // In real implementation, this would calculate available methods based on address, weight, etc.
            return await Task.FromResult(new List<string>
            {
                "Standard (5-7 business days)",
                "Express (2-3 business days)",
                "Overnight (1 business day)"
            });
        }

        public async Task<List<string>> GetAvailablePaymentMethodsAsync()
        {
            return await Task.FromResult(new List<string>
            {
                "Credit Card",
                "PayPal",
                "Apple Pay",
                "Google Pay"
            });
        }

        public async Task<bool> ExtendCheckoutSessionAsync(Guid checkoutId, int additionalMinutes = 15)
        {
            try
            {
                var checkout = await _context.Checkouts.FindAsync(checkoutId);
                if (checkout == null) return false;

                checkout.ExpiresAt = checkout.ExpiresAt.AddMinutes(additionalMinutes);
                checkout.UpdatedAt = DateTime.UtcNow;

                // Also extend stock reservations
                await _stockService.ExtendReservationsAsync(checkoutId, additionalMinutes);

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extending checkout session: {CheckoutId}", checkoutId);
                throw;
            }
        }

        public async Task<bool> IsCheckoutValidAsync(Guid checkoutId)
        {
            try
            {
                var checkout = await _context.Checkouts.FindAsync(checkoutId);
                return checkout != null && 
                       checkout.Status == "InProgress" && 
                       checkout.ExpiresAt > DateTime.UtcNow;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating checkout: {CheckoutId}", checkoutId);
                throw;
            }
        }

        public async Task<int> CleanupExpiredCheckoutsAsync()
        {
            _logger.LogInformation("Cleaning up expired checkouts");

            try
            {
                var expiredCheckouts = await _context.Checkouts
                    .Where(c => c.Status == "InProgress" && c.ExpiresAt <= DateTime.UtcNow)
                    .ToListAsync();

                foreach (var checkout in expiredCheckouts)
                {
                    checkout.Status = "Abandoned";
                    checkout.AbandonedAt = DateTime.UtcNow;
                    checkout.UpdatedAt = DateTime.UtcNow;

                    // Release stock reservations
                    await _stockService.ReleaseReservationsAsync(checkout.Id);
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("Cleaned up {Count} expired checkouts", expiredCheckouts.Count);
                return expiredCheckouts.Count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cleaning up expired checkouts");
                throw;
            }
        }

        public async Task<bool> PromptGuestForSignupAsync(Guid checkoutId)
        {
            try
            {
                var checkout = await _context.Checkouts.FindAsync(checkoutId);
                if (checkout == null) return false;

                checkout.GuestPromptedForSignup = true;
                checkout.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error prompting guest for signup: {CheckoutId}", checkoutId);
                throw;
            }
        }

        public async Task<User?> ConvertGuestToUserAsync(Guid checkoutId, CreateUserDto userDto)
        {
            _logger.LogInformation("Converting guest checkout to user: {CheckoutId}", checkoutId);

            try
            {
                var checkout = await _context.Checkouts.FindAsync(checkoutId);
                if (checkout == null || !checkout.IsGuestCheckout)
                {
                    return null;
                }

                // Create new user
                var user = await _userService.CreateUserAsync(userDto);

                // Update checkout to associate with new user
                checkout.UserId = user.Id;
                checkout.IsGuestCheckout = false;
                checkout.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Guest checkout converted to user: {UserId}", user.Id);
                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error converting guest to user: {CheckoutId}", checkoutId);
                throw;
            }
        }

        public async Task<bool> ValidateCartIntegrityAsync(Guid checkoutId, string cartId)
        {
            try
            {
                var checkout = await _context.Checkouts.FindAsync(checkoutId);
                if (checkout == null) return false;

                // Basic validation - ensure cart ID matches
                return checkout.CartId == cartId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating cart integrity: {CheckoutId}", checkoutId);
                throw;
            }
        }

        public async Task<List<StockReservation>> GetCheckoutStockReservationsAsync(Guid checkoutId)
        {
            try
            {
                return await _stockService.GetCheckoutReservationsAsync(checkoutId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting stock reservations for checkout: {CheckoutId}", checkoutId);
                throw;
            }
        }

        private async Task<CheckoutAddress> SetCheckoutAddressAsync(Guid checkoutId, CheckoutAddressDto dto, string addressType)
        {
            _logger.LogInformation("Setting {AddressType} address for checkout: {CheckoutId}", addressType, checkoutId);

            try
            {
                // Remove existing address of this type
                var existingAddress = await _context.CheckoutAddresses
                    .FirstOrDefaultAsync(a => a.CheckoutId == checkoutId && a.AddressType == addressType);

                if (existingAddress != null)
                {
                    _context.CheckoutAddresses.Remove(existingAddress);
                }

                // Create new address
                var address = new CheckoutAddress
                {
                    Id = Guid.NewGuid(),
                    CheckoutId = checkoutId,
                    AddressType = addressType,
                    FirstName = dto.FirstName,
                    LastName = dto.LastName,
                    Company = dto.Company,
                    AddressLine1 = dto.AddressLine1,
                    AddressLine2 = dto.AddressLine2,
                    City = dto.City,
                    State = dto.State,
                    PostalCode = dto.PostalCode,
                    Country = dto.Country,
                    PhoneNumber = dto.PhoneNumber,
                    Email = dto.Email,
                    UserAddressId = dto.UserAddressId,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.CheckoutAddresses.Add(address);

                // Update checkout step completion
                var checkout = await _context.Checkouts.FindAsync(checkoutId);
                if (checkout != null)
                {
                    if (addressType == "Shipping")
                        checkout.ShippingAddressCompleted = true;
                    else if (addressType == "Billing")
                        checkout.BillingAddressCompleted = true;
                    
                    checkout.UpdatedAt = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();
                return address;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting {AddressType} address for checkout: {CheckoutId}", addressType, checkoutId);
                throw;
            }
        }

        private static CheckoutSummaryDto MapToSummaryDto(Checkout checkout)
        {
            return new CheckoutSummaryDto
            {
                Id = checkout.Id,
                Email = checkout.Email,
                Status = checkout.Status,
                IsGuestCheckout = checkout.IsGuestCheckout,
                ShippingAddressCompleted = checkout.ShippingAddressCompleted,
                BillingAddressCompleted = checkout.BillingAddressCompleted,
                ShippingMethodCompleted = checkout.ShippingMethodCompleted,
                PaymentMethodCompleted = checkout.PaymentMethodCompleted,
                SubtotalAmount = checkout.SubtotalAmount,
                TaxAmount = checkout.TaxAmount,
                ShippingAmount = checkout.ShippingAmount,
                DiscountAmount = checkout.DiscountAmount,
                TotalAmount = checkout.TotalAmount,
                TaxRate = checkout.TaxRate,
                ShippingMethod = checkout.ShippingMethod,
                PaymentMethod = checkout.PaymentMethod,
                CreatedAt = checkout.CreatedAt,
                ExpiresAt = checkout.ExpiresAt
            };
        }
    }
}