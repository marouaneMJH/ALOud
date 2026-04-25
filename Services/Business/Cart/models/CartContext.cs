namespace ALOud.Models.Cart;

public sealed class CartContext
{
    public IReadOnlyList<CartItemContext> Items { get; init; } = [];
    public int TotalQuantity { get; init; }
    public decimal TotalPrice { get; init; }
}

public sealed class CartItemContext
{
    public Guid ProductId { get; init; }
    public string ProductName { get; init; } = string.Empty;
    public int Quantity { get; init; }
    public decimal UnitPrice { get; init; }
    public decimal Total { get; init; }
}
