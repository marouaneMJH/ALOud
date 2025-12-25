
using System.ComponentModel.DataAnnotations;

namespace ALOud.DTOs;

public class CreateUserDto
{
    [Required(ErrorMessage = "Le prénom est requis")]
    [MaxLength(100, ErrorMessage = "Le prénom ne peut pas dépasser 100 caractères")]
    [Display(Name = "Prénom")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Le nom est requis")]
    [MaxLength(100, ErrorMessage = "Le nom ne peut pas dépasser 100 caractères")]
    [Display(Name = "Nom")]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "L'email est requis")]
    [EmailAddress(ErrorMessage = "L'email n'est pas valide")]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Le mot de passe est requis")]
    [MinLength(8, ErrorMessage = "Le mot de passe doit contenir au minimum 8 caractères")]
    [Display(Name = "Mot de passe")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "L'adresse est requise")]
    [MinLength(10, ErrorMessage = "L'adresse doit contenir au minimum 10 caractères")]
    [Display(Name = "Adresse")]
    public string Address { get; set; } = string.Empty;
}