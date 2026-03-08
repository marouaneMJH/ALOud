using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ALOud.Models
{
    [Table("Users")]
    [Index(nameof(Email), IsUnique = true)]

    public class User
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [MaxLength(20)]
        public string FirstName { get; set; }

        [Required]
        [MaxLength(20)]
        public string LastName { get; set; }

        [Required]
        [EmailAddress]
        [MaxLength(150)]
        public string Email { get; set; }

        [Required]
        public string PasswordHash { get; set; }

        public bool IsActive { get; set; } = true;
        public bool IsEmailVerified { get; set; } = false;

        [Required]
        [MinLength(10)]
        [MaxLength(100)]
        public string Address
        {
            get; set;
        } = string.Empty;



        public DateTime CreatedAt { get; set; }
    }
}
