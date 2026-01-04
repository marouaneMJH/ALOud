using System;
using System.ComponentModel.DataAnnotations;

namespace ALOud.DTOs.Products;


public class CreateProductDto
{
    [Required]
    [MaxLength(200)]
    public string Name { get; set; }

    [MaxLength(2000)]
    public string Description { get; set; }

    [Range(0, 1_000_000)]
    public decimal Price { get; set; }

    [Range(0, int.MaxValue)]
    public int Stock { get; set; }

    [Url]
    public string ImageUrl { get; set; }

    [Required]
    public int CategoryId { get; set; }
}
