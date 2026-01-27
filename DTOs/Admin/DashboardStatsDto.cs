namespace ALOud.DTOs.Admin;

// Dashboard KPIs and statistics for Perfume Catalog
public class DashboardStatsDto
{
    public int TotalPerfumes { get; set; }
    public int TotalBrands { get; set; }
    public int TotalFamilies { get; set; }
    public int TotalNotes { get; set; }
    public int TotalAccords { get; set; }
    public int TotalTags { get; set; }
    public int TotalSeasons { get; set; }
    public int TotalOccasions { get; set; }

    public List<BrandStatsDto> TopBrands { get; set; } = new();
    public List<FamilyStatsDto> TopFamilies { get; set; } = new();
    public List<RecentPerfumeDto> RecentPerfumes { get; set; } = new();
}

public class BrandStatsDto
{
    public Guid BrandId { get; set; }
    public string BrandName { get; set; } = string.Empty;
    public int PerfumeCount { get; set; }
}

public class FamilyStatsDto
{
    public Guid FamilyId { get; set; }
    public string FamilyName { get; set; } = string.Empty;
    public int PerfumeCount { get; set; }
}

public class RecentPerfumeDto
{
    public Guid PerfumeId { get; set; }
    public string PerfumeName { get; set; } = string.Empty;
    public string BrandName { get; set; } = string.Empty;
    public string? GenderProfile { get; set; }
    public string? ImageUrl { get; set; }
    public DateTime CreatedAt { get; set; }
}
