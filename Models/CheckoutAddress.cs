using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ALOud.Models
{
    [Table("CheckoutAddresses")]
    public class CheckoutAddress
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid CheckoutId { get; set; }

        [Required]
        [MaxLength(50)]
        public required string AddressType { get; set; } // "Shipping" or "Billing"

        // Address fields
        [Required]
        [MaxLength(100)]
        public required string FirstName { get; set; }

        [Required]
        [MaxLength(100)]
        public required string LastName { get; set; }

        [MaxLength(100)]
        public string? Company { get; set; }

        [Required]
        [MaxLength(200)]
        public required string AddressLine1 { get; set; }

        [MaxLength(200)]
        public string? AddressLine2 { get; set; }

        [Required]
        [MaxLength(100)]
        public required string City { get; set; }

        [Required]
        [MaxLength(100)]
        public required string State { get; set; }

        [Required]
        [MaxLength(20)]
        public required string PostalCode { get; set; }

        [Required]
        [MaxLength(100)]
        public required string Country { get; set; }

        [MaxLength(20)]
        public string? PhoneNumber { get; set; }

        // For billing addresses
        [MaxLength(100)]
        public string? Email { get; set; }

        // Source tracking
        public Guid? UserAddressId { get; set; } // If copied from saved address

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Navigation properties
        [ForeignKey("CheckoutId")]
        public virtual Checkout Checkout { get; set; } = null!;

        [ForeignKey("UserAddressId")]
        public virtual UserAddress? UserAddress { get; set; }
    }
}