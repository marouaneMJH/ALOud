using System.ComponentModel.DataAnnotations;
using ALOud.DTOs.Validation;

namespace ALOud.DTOs.Perfumes
{
    public class CreatePerfumeDto : IValidatableObject
    {
        [Required(ErrorMessage = "Perfume name is required")]
        [MaxLength(300, ErrorMessage = "Perfume name cannot exceed 300 characters")]
        public string Name { get; set; } = string.Empty;

        [MaxLength(100)]
        [AllowedValuesList("Light", "Moderate", "Strong", "Very Strong", ErrorMessage = "Invalid intensity value")]
        public string? Intensity { get; set; }

        [MaxLength(100)]
        [AllowedValuesList("1-2 hours", "3-4 hours", "5-6 hours", "7-8 hours", "8+ hours", ErrorMessage = "Invalid longevity value")]
        public string? Longevity { get; set; }

        [MaxLength(100)]
        [AllowedValuesList("Intimate", "Moderate", "Strong", "Enormous", ErrorMessage = "Invalid sillage value")]
        public string? Sillage { get; set; }

        [MaxLength(100)]
        [AllowedValuesList("Male", "Female", "Unisex", "Masculine", "Feminine", ErrorMessage = "Invalid gender profile value")]
        public string? GenderProfile { get; set; }

        [MaxLength(100)]
        [AllowedValuesList("Budget", "Mid-range", "Premium", "Luxury", ErrorMessage = "Invalid price range value")]
        public string? PriceRange { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Price must be a positive value")]
        public decimal Price { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Stock quantity must be zero or greater")]
        public int StockQuantity { get; set; }

        [MaxLength(2000, ErrorMessage = "Description cannot exceed 2000 characters")]
        public string? Description { get; set; }

        [MaxLength(500, ErrorMessage = "Image URL cannot exceed 500 characters")]
        [Url(ErrorMessage = "Image URL must be a valid URL")]
        public string? ImageUrl { get; set; }

        [Required(ErrorMessage = "Brand is required")]
        public Guid BrandId { get; set; }

        // Many-to-many selections
        public List<Guid> FamilyIds { get; set; } = new();
        public List<PerfumeNoteSelectionDto> NoteSelections { get; set; } = new();
        public List<PerfumeAccordSelectionDto> AccordSelections { get; set; } = new();
        public List<Guid> TagIds { get; set; } = new();
        public List<Guid> SeasonIds { get; set; } = new();
        public List<Guid> OccasionIds { get; set; } = new();

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            foreach (var error in ValidateDuplicateIds(FamilyIds, nameof(FamilyIds))) yield return error;
            foreach (var error in ValidateDuplicateIds(TagIds, nameof(TagIds))) yield return error;
            foreach (var error in ValidateDuplicateIds(SeasonIds, nameof(SeasonIds))) yield return error;
            foreach (var error in ValidateDuplicateIds(OccasionIds, nameof(OccasionIds))) yield return error;

            if (NoteSelections.Count != NoteSelections.Select(n => n.NoteId).Distinct().Count())
                yield return new ValidationResult("Duplicate note selections are not allowed", new[] { nameof(NoteSelections) });

            if (AccordSelections.Count != AccordSelections.Select(a => a.AccordId).Distinct().Count())
                yield return new ValidationResult("Duplicate accord selections are not allowed", new[] { nameof(AccordSelections) });
        }

        private static IEnumerable<ValidationResult> ValidateDuplicateIds(IEnumerable<Guid> ids, string fieldName)
        {
            var list = ids.ToList();
            if (list.Count != list.Distinct().Count())
                yield return new ValidationResult($"Duplicate {fieldName} entries are not allowed", new[] { fieldName });
        }
    }

    public class UpdatePerfumeDto : IValidatableObject
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Perfume name is required")]
        [MaxLength(300, ErrorMessage = "Perfume name cannot exceed 300 characters")]
        public string Name { get; set; } = string.Empty;

        [MaxLength(100)]
        [AllowedValuesList("Light", "Moderate", "Strong", "Very Strong", ErrorMessage = "Invalid intensity value")]
        public string? Intensity { get; set; }

        [MaxLength(100)]
        [AllowedValuesList("1-2 hours", "3-4 hours", "5-6 hours", "7-8 hours", "8+ hours", ErrorMessage = "Invalid longevity value")]
        public string? Longevity { get; set; }

        [MaxLength(100)]
        [AllowedValuesList("Intimate", "Moderate", "Strong", "Enormous", ErrorMessage = "Invalid sillage value")]
        public string? Sillage { get; set; }

        [MaxLength(100)]
        [AllowedValuesList("Male", "Female", "Unisex", "Masculine", "Feminine", ErrorMessage = "Invalid gender profile value")]
        public string? GenderProfile { get; set; }

        [MaxLength(100)]
        [AllowedValuesList("Budget", "Mid-range", "Premium", "Luxury", ErrorMessage = "Invalid price range value")]
        public string? PriceRange { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Price must be a positive value")]
        public decimal Price { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Stock quantity must be zero or greater")]
        public int StockQuantity { get; set; }

        [MaxLength(2000, ErrorMessage = "Description cannot exceed 2000 characters")]
        public string? Description { get; set; }

        [MaxLength(500, ErrorMessage = "Image URL cannot exceed 500 characters")]
        [Url(ErrorMessage = "Image URL must be a valid URL")]
        public string? ImageUrl { get; set; }

        [Required(ErrorMessage = "Brand is required")]
        public Guid BrandId { get; set; }

        // Many-to-many selections
        public List<Guid> FamilyIds { get; set; } = new();
        public List<PerfumeNoteSelectionDto> NoteSelections { get; set; } = new();
        public List<PerfumeAccordSelectionDto> AccordSelections { get; set; } = new();
        public List<Guid> TagIds { get; set; } = new();
        public List<Guid> SeasonIds { get; set; } = new();
        public List<Guid> OccasionIds { get; set; } = new();

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            foreach (var error in ValidateDuplicateIds(FamilyIds, nameof(FamilyIds))) yield return error;
            foreach (var error in ValidateDuplicateIds(TagIds, nameof(TagIds))) yield return error;
            foreach (var error in ValidateDuplicateIds(SeasonIds, nameof(SeasonIds))) yield return error;
            foreach (var error in ValidateDuplicateIds(OccasionIds, nameof(OccasionIds))) yield return error;

            if (NoteSelections.Count != NoteSelections.Select(n => n.NoteId).Distinct().Count())
                yield return new ValidationResult("Duplicate note selections are not allowed", new[] { nameof(NoteSelections) });

            if (AccordSelections.Count != AccordSelections.Select(a => a.AccordId).Distinct().Count())
                yield return new ValidationResult("Duplicate accord selections are not allowed", new[] { nameof(AccordSelections) });
        }

        private static IEnumerable<ValidationResult> ValidateDuplicateIds(IEnumerable<Guid> ids, string fieldName)
        {
            var list = ids.ToList();
            if (list.Count != list.Distinct().Count())
                yield return new ValidationResult($"Duplicate {fieldName} entries are not allowed", new[] { fieldName });
        }
    }

    public class PerfumeDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Intensity { get; set; }
        public string? Longevity { get; set; }
        public string? Sillage { get; set; }
        public string? GenderProfile { get; set; }
        public string? PriceRange { get; set; }
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        public Guid BrandId { get; set; }
        public string BrandName { get; set; } = string.Empty;
        public List<string> Families { get; set; } = new();
        public DateTime CreatedAt { get; set; }
    }

    public class PerfumeDetailsDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Intensity { get; set; }
        public string? Longevity { get; set; }
        public string? Sillage { get; set; }
        public string? GenderProfile { get; set; }
        public string? PriceRange { get; set; }
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid BrandId { get; set; }
        public string BrandName { get; set; } = string.Empty;

        // Related entities
        public List<string> Families { get; set; } = new();
        public List<PerfumeNoteDto> Notes { get; set; } = new();
        public List<PerfumeAccordDto> Accords { get; set; } = new();
        public List<string> Tags { get; set; } = new();
        public List<string> Seasons { get; set; } = new();
        public List<string> Occasions { get; set; } = new();
    }

    public class PerfumeNoteSelectionDto
    {
        public Guid NoteId { get; set; }
        [AllowedValuesList("Top", "Middle", "Base", ErrorMessage = "Invalid note level")]
        public string? NoteLevel { get; set; } // Top, Middle, Base
    }

    public class PerfumeAccordSelectionDto
    {
        public Guid AccordId { get; set; }
        [AllowedValuesList("Strong", "Medium", "Light", ErrorMessage = "Invalid accord intensity")]
        public string? Intensity { get; set; } // Strong, Medium, Light
    }

    public class PerfumeNoteDto
    {
        public Guid NoteId { get; set; }
        public string NoteName { get; set; } = string.Empty;
        public string? NoteLevel { get; set; }
    }

    public class PerfumeAccordDto
    {
        public Guid AccordId { get; set; }
        public string AccordName { get; set; } = string.Empty;
        public string? Intensity { get; set; }
    }
}
