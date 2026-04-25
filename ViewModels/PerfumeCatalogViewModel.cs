using ALOud.DTOs.Brands;
using ALOud.DTOs.Families;
using ALOud.DTOs.Perfumes;

namespace ViewModels;

public class PerfumeCatalogViewModel
{
    public PaginatedList<PerfumeDto>? Perfumes { get; set; }
    public List<BrandSelectDto> Brands { get; set; } = new();
    public List<FamilySelectDto> Families { get; set; } = new();
    public string? SearchQuery { get; set; }
    public Guid? SelectedBrandId { get; set; }
    public Guid? SelectedFamilyId { get; set; }
    public string? SelectedGender { get; set; }

    public List<PerfumeDto> Items => Perfumes?.Items ?? new List<PerfumeDto>();
    public int PageIndex => Perfumes?.PageIndex ?? 1;
    public int TotalPages => Perfumes?.TotalPages ?? 0;
    public int TotalCount => Perfumes?.TotalCount ?? 0;
    public int PageSize => Perfumes?.PageSize ?? 12;
    public bool HasPreviousPage => Perfumes?.HasPreviousPage ?? false;
    public bool HasNextPage => Perfumes?.HasNextPage ?? false;
}
