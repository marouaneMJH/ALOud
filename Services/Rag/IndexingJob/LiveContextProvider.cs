using ALOud.Services.Cart;
using ALOud.Services.Rag.Models;

public sealed class CartLiveContextProvider : ILiveContextProvider
{
    private readonly ICartContextBuilder _cartContextBuilder;

    public CartLiveContextProvider(ICartContextBuilder cartContextBuilder)
    {
        _cartContextBuilder = cartContextBuilder;
    }

    public async Task<LiveContextPayload?> BuildAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var cart = await _cartContextBuilder.BuildAsync(userId);

        return new LiveContextPayload
        {
            Name = "cart",
            Data = cart
        };
    }
}
