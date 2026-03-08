using System.ComponentModel.DataAnnotations;

namespace ALOud.DTOs.Validation;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public sealed class AllowedValuesListAttribute : ValidationAttribute
{
    private readonly HashSet<string> _allowed;

    public AllowedValuesListAttribute(params string[] allowedValues)
    {
        _allowed = allowedValues
            .Where(v => !string.IsNullOrWhiteSpace(v))
            .Select(v => v.Trim())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is null)
            return ValidationResult.Success;

        var text = value.ToString();
        if (string.IsNullOrWhiteSpace(text))
            return ValidationResult.Success;

        return _allowed.Contains(text.Trim())
            ? ValidationResult.Success
            : new ValidationResult(ErrorMessage ?? $"Invalid value for {validationContext.MemberName}.");
    }
}
