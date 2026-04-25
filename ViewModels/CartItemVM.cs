namespace ViewModels
{
    public class CartItemVM
    {
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string BrandName { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public string ImageUrl { get; set; } = string.Empty;

        public decimal Total => Price * Quantity;
    }
}
