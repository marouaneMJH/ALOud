using System.ComponentModel.DataAnnotations;

namespace ALOud.DTOs.Brands
{
    public class CreateBrandDto
    {
        [Required(ErrorMessage = "Brand name is required")]
        [MaxLength(200, ErrorMessage = "Brand name cannot exceed 200 characters")]
        public string Name { get; set; } = string.Empty;
    }

    public class UpdateBrandDto
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Brand name is required")]
        [MaxLength(200, ErrorMessage = "Brand name cannot exceed 200 characters")]
        public string Name { get; set; } = string.Empty;
    }

    public class BrandDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int PerfumeCount { get; set; }
    }

    public class BrandSelectDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
