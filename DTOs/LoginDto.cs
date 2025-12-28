
using System.ComponentModel.DataAnnotations;

namespace ALOud.DTOs;

public class LoginDto
{
    [Required(ErrorMessage = "L'email est requis")]
    [EmailAddress(ErrorMessage = "L'email n'est pas valide")]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Le mot de passe est requis")]
    [Display(Name = "Mot de passe")]
    public string Password { get; set; } = string.Empty;

}
