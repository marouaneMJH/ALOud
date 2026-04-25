using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace ALOud.DTOs.Validation;

/// <summary>
/// Validates that a string represents a properly formatted address
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public sealed class AddressAttribute : ValidationAttribute
{
    private readonly int _minLength;
    private readonly int _maxLength;
    private static readonly char[] AddressSeparators = { ' ', ',', '\t' };

    public AddressAttribute(int minLength = 10, int maxLength = 200)
    {
        _minLength = minLength;
        _maxLength = maxLength;
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is null or string { Length: 0 })
        {
            return new ValidationResult("Address is required");
        }

        var address = value.ToString()!;

        // Check length
        if (address.Length < _minLength)
        {
            return new ValidationResult($"Address must be at least {_minLength} characters long");
        }

        if (address.Length > _maxLength)
        {
            return new ValidationResult($"Address cannot exceed {_maxLength} characters");
        }

        // Check for at least one number (street number)
        if (!Regex.IsMatch(address, @"\d"))
        {
            return new ValidationResult("Address must contain at least one street number");
        }

        // Check for at least one letter
        if (!Regex.IsMatch(address, @"[a-zA-Z]"))
        {
            return new ValidationResult("Address must contain letters");
        }

        // Check for minimum word count (at least 3 words)
        var words = address.Split(AddressSeparators, StringSplitOptions.RemoveEmptyEntries);
        if (words.Length < 3)
        {
            return new ValidationResult("Address must contain at least 3 words (number, street, city)");
        }

        return ValidationResult.Success;
    }
}
