public class PerfumeRagSource
{
    public Guid Id { get; init; }
    public string Name { get; init; }
    public string Brand { get; init; }

    public string Intensity { get; init; }
    public string Longevity { get; init; }
    public string Sillage { get; init; }
    public string GenderProfile { get; init; }
    public string PriceRange { get; init; }

    public IReadOnlyList<string> Families { get; init; }
    public IReadOnlyList<PerfumeNoteInfo> Notes { get; init; }
    public IReadOnlyList<PerfumeAccordInfo> Accords { get; init; }
    public IReadOnlyList<string> Tags { get; init; }
    public IReadOnlyList<string> Seasons { get; init; }
    public IReadOnlyList<string> Occasions { get; init; }
}

public class PerfumeNoteInfo
{
    public string Name { get; init; }
    public string Category { get; init; }
    public string Level { get; init; }
}

public class PerfumeAccordInfo
{
    public string Name { get; init; }
    public string Intensity { get; init; }
}
