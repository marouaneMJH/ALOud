using System.ComponentModel.DataAnnotations;

namespace ALOud.DTOs.Categories;

public class UpdateCategoryDto
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Category name is required")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 100 characters")]
    public string Name { get; set; } = string.Empty;
}
