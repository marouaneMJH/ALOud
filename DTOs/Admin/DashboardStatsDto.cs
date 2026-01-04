namespace ALOud.DTOs.Admin;

// Dashboard KPIs and statistics
public class DashboardStatsDto
{
    public int TotalProducts { get; set; }
    public int TotalCategories { get; set; }
    public int LowStockProducts { get; set; }
    public int OutOfStockProducts { get; set; }
    public decimal TotalInventoryValue { get; set; }
    public List<CategoryStatsDto> CategoryStats { get; set; } = new();
    public List<ProductStockAlertDto> LowStockAlerts { get; set; } = new();
}

public class CategoryStatsDto
{
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public int ProductCount { get; set; }
    public int TotalStock { get; set; }
}

public class ProductStockAlertDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Stock { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
}
