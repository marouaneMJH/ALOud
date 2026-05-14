namespace ALOud.Services.Infrastructure.Rag.Models
{
    public class PerfumeRagSource
    {
        public Guid Id { get; init; }
        public string Name { get; init; } = string.Empty;
        public string Brand { get; init; } = string.Empty;

        public string? Intensity { get; init; }
        public string? Longevity { get; init; }
        public string? Sillage { get; init; }
        public string? GenderProfile { get; init; }

        public string? PriceRange { get; init; }
        public decimal Price { get; init; }

        public string? Description { get; init; }
        public string? ImageUrl { get; init; }

        public IReadOnlyList<string> Families { get; init; } = [];
        public IReadOnlyList<PerfumeNoteInfo> Notes { get; init; } = [];
        public IReadOnlyList<PerfumeAccordInfo> Accords { get; init; } = [];
        public IReadOnlyList<string> Tags { get; init; } = [];
        public IReadOnlyList<string> Seasons { get; init; } = [];
        public IReadOnlyList<string> Occasions { get; init; } = [];
    }

    public class PerfumeNoteInfo
    {
        public required string Name { get; init; }
        public required string Category { get; init; }
        public required string Level { get; init; }
    }

    public class PerfumeAccordInfo
    {
        public required string Name { get; init; }
        public required string Intensity { get; init; }
    }
}
