using System.ComponentModel.DataAnnotations;

namespace ALOud.DTOs.Validation;

/// <summary>
/// Validates that a password confirmation matches the original password
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public sealed class ComparePasswordAttribute : ValidationAttribute
{
    private readonly string _comparisonProperty;

    public ComparePasswordAttribute(string comparisonProperty)
    {
        _comparisonProperty = comparisonProperty;
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        var property = validationContext.ObjectType.GetProperty(_comparisonProperty);
        
        if (property == null)
        {
            return new ValidationResult($"Property {_comparisonProperty} not found");
        }

        var comparisonValue = property.GetValue(validationContext.ObjectInstance);

        if (value == null && comparisonValue == null)
        {
            return ValidationResult.Success;
        }

        if (value == null || comparisonValue == null)
        {
            return new ValidationResult("Passwords do not match");
        }

        if (!value.Equals(comparisonValue))
        {
            return new ValidationResult("Passwords do not match");
        }

        return ValidationResult.Success;
    }
}
