
using System.ComponentModel.DataAnnotations;

namespace ALOud.DTOs;

public class CreateUserDto
{
    [Required]
    [MaxLength(100)]
    public string FirstName { get; set; }

    [Required]
    [MaxLength(100)]
    public string LastName { get; set; }

    [Required]
    [EmailAddress]
    public string Email { get; set; }

    [Required]
    [MinLength(8)]
    public string Password { get; set; }


    [Required]
    [MinLength(8)]

    public string Address;

}