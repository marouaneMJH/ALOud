using ALOud.Services;
using System.Text.Json;

namespace ALOud.Services.Rag;

public sealed class RagToolDispatcher
{
    private readonly CartService _cartService;
    private readonly IProductService _productService;

    public RagToolDispatcher(
        CartService cartService,
        IProductService productService)
    {
        _cartService = cartService;
        _productService = productService;
    }

    public async Task<object?> DispatchAsync(string toolName, Dictionary<string, object> args)
    {
        return toolName switch
        {
            "search_products" => await HandleSearchAsync(args),
            "get_product_details" => await HandleGetProductDetailsAsync(args),
            "recommend_products" => await HandleRecommendProductsAsync(args),
            "get_cart" => await HandleGetCartAsync(),
            "add_to_cart" => await HandleAddAsync(args),
            "remove_from_cart" => await HandleRemoveAsync(args),
            "increase" => await HandleIncreaseAsync(args),
            "decrease" => await HandleDecreaseAsync(args),
            "analyze_cart" => await HandleAnalyzeCartAsync(),
            "compare_products" => await HandleCompareProductsAsync(args),
            _ => new { Error = $"Outil inconnu: {toolName}" }
        };
    }

    // OPTIMIZED: Limit to 5 results, minimal fields
    private async Task<object> HandleSearchAsync(Dictionary<string, object> args)
    {
        var query = args["query"].ToString() ?? string.Empty;
        var products = await _productService.SearchProductsAsync(query);

        if (!products.Any())
        {
            return new
            {
                Found = 0,
                Message = $"Aucun produit trouvé pour '{query}'",
                Suggestion = "Essayez avec d'autres mots-clés comme 'boisé', 'frais', ou une marque spécifique"
            };
        }

        // Return minimal data - only what's needed for display
        return new
        {
            Found = products.Count,
            Products = products.Take(5).Select(p =>
            {
                dynamic dp = p;
                return new
                {
                    Id = (int)dp.Id,
                    Name = (string)dp.Name,
                    Price = (decimal)dp.Price,
                    Image = (string)dp.ImageUrl
                };
            }).ToList()
        };
    }

    private async Task<object> HandleGetProductDetailsAsync(Dictionary<string, object> args)
    {
        var productId = GetInt32(args["productId"]);
        var product = await _productService.GetProductByIdAsync(productId);

        if (product == null)
            return new
            {
                Error = $"Produit #{productId} introuvable",
                Action = "Utilisez search_products pour trouver le bon ID"
            };

        // Truncate description to save tokens
        var shortDesc = product.Description?.Length > 100
            ? product.Description[..100] + "..."
            : product.Description;

        return new
        {
            product.Id,
            product.Name,
            Desc = shortDesc,
            product.Price,
            product.Stock,
            Dispo = product.Stock > 0 ? "En stock" : "Rupture",
            Img = product.ImageUrl
        };
    }

    private async Task<object> HandleRecommendProductsAsync(Dictionary<string, object> args)
    {
        var preferences = args["preferences"].ToString() ?? string.Empty;
        var limit = args.TryGetValue("limit", out var l) ? GetInt32(l) : 3;
        limit = Math.Min(limit, 5); // Cap at 5 to save tokens

        var products = await _productService.SearchProductsAsync(preferences);

        if (!products.Any())
        {
            // Fallback to popular products instead of random
            var allProducts = await _productService.GetAllProductsAsync();
            products = allProducts
                .OrderByDescending(p => p.Stock) // In-stock first
                .Take(limit)
                .Cast<object>()
                .ToList();
        }

        return new
        {
            BasedOn = preferences,
            Recommendations = products.Take(limit).Select(p =>
            {
                dynamic dp = p;
                return new
                {
                    Id = (int)dp.Id,
                    Name = (string)dp.Name,
                    Price = (decimal)dp.Price,
                    Img = (string)dp.ImageUrl
                };
            }).ToList()
        };
    }

    // OPTIMIZED: Minimal cart data
    private async Task<object> HandleGetCartAsync()
    {
        var cart = await _cartService.GetCartAsync();

        if (!cart.Any())
            return new { Empty = true, Message = "Votre panier est vide" };

        return new
        {
            Items = cart.Select(i => new
            {
                Id = i.ProductId,
                Name = i.ProductName,
                Qty = i.Quantity,
                Prix = i.Price
            }).ToList(),
            Total = cart.Sum(i => i.Price * i.Quantity)
        };
    }

    private async Task<object> HandleAddAsync(Dictionary<string, object> args)
    {
        var productId = GetInt32(args["productId"]);
        var quantity = args.TryGetValue("quantity", out var q) ? GetInt32(q) : 1;

        var product = await _productService.GetProductByIdAsync(productId);

        if (product == null)
            return new { Ok = false, Error = "Produit introuvable" };

        if (product.Stock < quantity)
        {
            return new
            {
                Ok = false,
                Error = $"Stock insuffisant ({product.Stock} dispo)",
                Stock = product.Stock
            };
        }

        for (int i = 0; i < quantity; i++)
        {
            await _cartService.AddToCartAsync(new ViewModels.CartItemVM
            {
                ProductId = product.Id,
                ProductName = product.Name,
                Price = product.Price,
                Quantity = 1
            });
        }

        return new
        {
            Ok = true,
            Added = $"{quantity}x {product.Name}",
            Prix = product.Price * quantity
        };
    }

    private async Task<object> HandleRemoveAsync(Dictionary<string, object> args)
    {
        var productId = GetInt32(args["productId"]);
        await _cartService.RemoveAsync(productId);
        return new { Ok = true, Removed = productId };
    }

    private async Task<object> HandleIncreaseAsync(Dictionary<string, object> args)
    {
        var productId = GetInt32(args["productId"]);
        await _cartService.IncreaseAsync(productId);
        return new { Ok = true };
    }

    private async Task<object> HandleDecreaseAsync(Dictionary<string, object> args)
    {
        var productId = GetInt32(args["productId"]);
        await _cartService.DecreaseAsync(productId);
        return new { Ok = true };
    }

    // NEW: Smart cart analysis
    private async Task<object> HandleAnalyzeCartAsync()
    {
        var cart = await _cartService.GetCartAsync();

        if (!cart.Any())
            return new { Empty = true, Conseil = "Découvrez nos parfums avec recommend_products!" };

        var total = cart.Sum(i => i.Price * i.Quantity);
        var itemCount = cart.Sum(i => i.Quantity);

        // Check stock for each item
        var stockWarnings = new List<string>();
        foreach (var item in cart)
        {
            var product = await _productService.GetProductByIdAsync(item.ProductId);
            if (product != null && product.Stock < item.Quantity)
            {
                stockWarnings.Add($"{item.ProductName}: seulement {product.Stock} en stock");
            }
        }

        // Smart suggestions
        var suggestions = new List<string>();
        if (total < 500)
            suggestions.Add("Ajoutez 1 article pour livraison gratuite (>500 MAD)");
        if (itemCount == 1)
            suggestions.Add("Découvrez nos coffrets pour économiser!");

        return new
        {
            Articles = itemCount,
            Total = $"{total:N0} MAD",
            Alertes = stockWarnings.Any() ? stockWarnings : null,
            Conseils = suggestions.Any() ? suggestions : null,
            PretPourCommande = !stockWarnings.Any()
        };
    }

    // NEW: Product comparison
    private async Task<object> HandleCompareProductsAsync(Dictionary<string, object> args)
    {
        var idsObj = args["productIds"];
        var ids = new List<int>();

        if (idsObj is JsonElement jsonElement && jsonElement.ValueKind == JsonValueKind.Array)
        {
            foreach (var el in jsonElement.EnumerateArray())
            {
                ids.Add(el.GetInt32());
            }
        }

        if (ids.Count < 2)
            return new { Error = "Fournissez au moins 2 IDs de produits" };

        var products = new List<object>();
        foreach (var id in ids.Take(3))
        {
            var product = await _productService.GetProductByIdAsync(id);
            if (product != null)
            {
                products.Add(new
                {
                    product.Id,
                    product.Name,
                    product.Price,
                    product.Stock,
                    Dispo = product.Stock > 0
                });
            }
        }

        if (products.Count < 2)
            return new { Error = "Produits introuvables" };

        // Find best value
        var sorted = products.OrderBy(p => ((dynamic)p).Price).ToList();
        var cheapest = ((dynamic)sorted.First()).Name;

        return new
        {
            Produits = products,
            MoinsCher = cheapest,
            Conseil = $"{cheapest} offre le meilleur rapport qualité-prix"
        };
    }

    private static int GetInt32(object value)
    {
        if (value is JsonElement element)
            return element.GetInt32();
        return Convert.ToInt32(value);
    }
}
