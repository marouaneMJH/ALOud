using System.ComponentModel.DataAnnotations;

namespace ALOud.DTOs.Perfumes
{
    public class CreatePerfumeDto
    {
        [Required(ErrorMessage = "Perfume name is required")]
        [MaxLength(300, ErrorMessage = "Perfume name cannot exceed 300 characters")]
        public string Name { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? Intensity { get; set; }

        [MaxLength(100)]
        public string? Longevity { get; set; }

        [MaxLength(100)]
        public string? Sillage { get; set; }

        [MaxLength(100)]
        public string? GenderProfile { get; set; }

        [MaxLength(100)]
        public string? PriceRange { get; set; }

        [Required(ErrorMessage = "Brand is required")]
        public Guid BrandId { get; set; }

        // Many-to-many selections
        public List<Guid> FamilyIds { get; set; } = new();
        public List<PerfumeNoteSelectionDto> NoteSelections { get; set; } = new();
        public List<PerfumeAccordSelectionDto> AccordSelections { get; set; } = new();
        public List<Guid> TagIds { get; set; } = new();
        public List<Guid> SeasonIds { get; set; } = new();
        public List<Guid> OccasionIds { get; set; } = new();
    }

    public class UpdatePerfumeDto
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Perfume name is required")]
        [MaxLength(300, ErrorMessage = "Perfume name cannot exceed 300 characters")]
        public string Name { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? Intensity { get; set; }

        [MaxLength(100)]
        public string? Longevity { get; set; }

        [MaxLength(100)]
        public string? Sillage { get; set; }

        [MaxLength(100)]
        public string? GenderProfile { get; set; }

        [MaxLength(100)]
        public string? PriceRange { get; set; }

        [Required(ErrorMessage = "Brand is required")]
        public Guid BrandId { get; set; }

        // Many-to-many selections
        public List<Guid> FamilyIds { get; set; } = new();
        public List<PerfumeNoteSelectionDto> NoteSelections { get; set; } = new();
        public List<PerfumeAccordSelectionDto> AccordSelections { get; set; } = new();
        public List<Guid> TagIds { get; set; } = new();
        public List<Guid> SeasonIds { get; set; } = new();
        public List<Guid> OccasionIds { get; set; } = new();
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
        public string? NoteLevel { get; set; } // Top, Middle, Base
    }

    public class PerfumeAccordSelectionDto
    {
        public Guid AccordId { get; set; }
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
