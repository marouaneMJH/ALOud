using System.ComponentModel.DataAnnotations;

namespace ALOud.DTOs
{
    public class StartCheckoutDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        public bool IsGuestCheckout { get; set; } = false;
    }

    public class UpdateCheckoutStepDto
    {
        [Required]
        public Guid CheckoutId { get; set; }

        public bool ShippingAddressCompleted { get; set; }
        public bool BillingAddressCompleted { get; set; }
        public bool ShippingMethodCompleted { get; set; }
        public bool PaymentMethodCompleted { get; set; }
    }

    public class CheckoutSummaryDto
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public bool IsGuestCheckout { get; set; }
        
        // Step completion
        public bool ShippingAddressCompleted { get; set; }
        public bool BillingAddressCompleted { get; set; }
        public bool ShippingMethodCompleted { get; set; }
        public bool PaymentMethodCompleted { get; set; }

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

        // Timestamps
        public DateTime CreatedAt { get; set; }
        public DateTime ExpiresAt { get; set; }

        // Progress calculation
        public int ProgressPercentage => CalculateProgress();
        public bool CanComplete => ShippingAddressCompleted && BillingAddressCompleted && 
                                 ShippingMethodCompleted && PaymentMethodCompleted;

        private int CalculateProgress()
        {
            int completed = 0;
            if (ShippingAddressCompleted) completed++;
            if (BillingAddressCompleted) completed++;
            if (ShippingMethodCompleted) completed++;
            if (PaymentMethodCompleted) completed++;
            return (completed * 100) / 4;
        }
    }

    public class CheckoutAddressDto
    {
        public Guid? Id { get; set; }
        public Guid CheckoutId { get; set; }
        public string AddressType { get; set; } = string.Empty; // "Shipping" or "Billing"
        
        [Required]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        public string LastName { get; set; } = string.Empty;

        public string? Company { get; set; }

        [Required]
        public string AddressLine1 { get; set; } = string.Empty;

        public string? AddressLine2 { get; set; }

        [Required]
        public string City { get; set; } = string.Empty;

        [Required]
        public string State { get; set; } = string.Empty;

        [Required]
        public string PostalCode { get; set; } = string.Empty;

        [Required]
        public string Country { get; set; } = string.Empty;

        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }

        // For copying from saved address
        public Guid? UserAddressId { get; set; }
    }

    public class CompleteCheckoutDto
    {
        [Required]
        public Guid CheckoutId { get; set; }

        [Required]
        public string PaymentMethod { get; set; } = string.Empty;

        public string? PaymentIntentId { get; set; }
        public bool SaveAddressForFuture { get; set; } = false;
    }
}