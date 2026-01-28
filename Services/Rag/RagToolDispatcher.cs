using ALOud.Services.Perfume;
using ALOud.Data;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace ALOud.Services.Rag;

public sealed class RagToolDispatcher
{
    private readonly CartService _cartService;
    private readonly ALOudDbContext _db;

    public RagToolDispatcher(
        CartService cartService,
        ALOudDbContext db)
    {
        _cartService = cartService;
        _db = db;
    }

    public async Task<object?> DispatchAsync(string toolName, Dictionary<string, object> args)
    {
        return toolName switch
        {
            "search_perfumes" => await HandleSearchPerfumesAsync(args),
            "get_perfume_details" => await HandleGetPerfumeDetailsAsync(args),
            "recommend_perfumes" => await HandleRecommendPerfumesAsync(args),
            "get_cart" => await HandleGetCartAsync(),
            "add_to_cart" => await HandleAddAsync(args),
            "remove_from_cart" => await HandleRemoveAsync(args),
            "increase" => await HandleIncreaseAsync(args),
            "decrease" => await HandleDecreaseAsync(args),
            "analyze_cart" => await HandleAnalyzeCartAsync(),
            "compare_perfumes" => await HandleComparePerfumesAsync(args),
            "get_brands" => await HandleGetBrandsAsync(),
            "get_families" => await HandleGetFamiliesAsync(),
            _ => new { Error = $"Outil inconnu: {toolName}" }
        };
    }

    // Search perfumes by name, brand, notes, or family
    private async Task<object> HandleSearchPerfumesAsync(Dictionary<string, object> args)
    {
        var query = args["query"].ToString()?.ToLower() ?? string.Empty;

        var perfumes = await _db.Perfumes
            .Include(p => p.Brand)
            .Where(p => p.Name.ToLower().Contains(query) ||
                        p.Brand.Name.ToLower().Contains(query) ||
                        (p.GenderProfile != null && p.GenderProfile.ToLower().Contains(query)))
            .Take(5)
            .Select(p => new
            {
                Id = p.Id,
                Name = p.Name,
                Brand = p.Brand.Name,
                Gender = p.GenderProfile,
                Image = p.ImageUrl
            })
            .ToListAsync();

        if (!perfumes.Any())
        {
            return new
            {
                Found = 0,
                Message = $"Aucun parfum trouvé pour '{query}'",
                Suggestion = "Essayez avec des mots-clés comme 'boisé', 'floral', 'Dior', ou 'homme'"
            };
        }

        return new
        {
            Found = perfumes.Count,
            Perfumes = perfumes
        };
    }

    private async Task<object> HandleGetPerfumeDetailsAsync(Dictionary<string, object> args)
    {
        var perfumeId = GetGuid(args["perfumeId"]);

        var perfume = await _db.Perfumes
            .Include(p => p.Brand)
            .Include(p => p.PerfumeFamilies).ThenInclude(pf => pf.Family)
            .Include(p => p.PerfumeNotes).ThenInclude(pn => pn.Note)
            .Include(p => p.PerfumeAccords).ThenInclude(pa => pa.Accord)
            .FirstOrDefaultAsync(p => p.Id == perfumeId);

        if (perfume == null)
            return new
            {
                Error = "Parfum introuvable",
                Action = "Utilisez search_perfumes pour trouver le bon ID"
            };

        return new
        {
            perfume.Id,
            perfume.Name,
            Brand = perfume.Brand.Name,
            Gender = perfume.GenderProfile,
            Intensity = perfume.Intensity,
            Longevity = perfume.Longevity,
            Sillage = perfume.Sillage,
            PriceRange = perfume.PriceRange,
            Families = perfume.PerfumeFamilies.Select(pf => pf.Family.Name).ToList(),
            Notes = perfume.PerfumeNotes.Select(pn => new { pn.Note.Name, NoteLevel = pn.NoteLevel }).ToList(),
            Accords = perfume.PerfumeAccords.Select(pa => pa.Accord.Name).ToList(),
            Image = perfume.ImageUrl
        };
    }

    private async Task<object> HandleRecommendPerfumesAsync(Dictionary<string, object> args)
    {
        var preferences = args["preferences"].ToString()?.ToLower() ?? string.Empty;
        var limit = args.TryGetValue("limit", out var l) ? GetInt32(l) : 3;
        limit = Math.Min(limit, 5);

        // Try to match by gender, family, or notes
        var perfumes = await _db.Perfumes
            .Include(p => p.Brand)
            .Where(p => p.Name.ToLower().Contains(preferences) ||
                        p.Brand.Name.ToLower().Contains(preferences) ||
                        (p.GenderProfile != null && p.GenderProfile.ToLower().Contains(preferences)) ||
                        p.PerfumeFamilies.Any(pf => pf.Family.Name.ToLower().Contains(preferences)) ||
                        p.PerfumeNotes.Any(pn => pn.Note.Name.ToLower().Contains(preferences)))
            .Take(limit)
            .Select(p => new
            {
                Id = p.Id,
                Name = p.Name,
                Brand = p.Brand.Name,
                Gender = p.GenderProfile,
                Image = p.ImageUrl
            })
            .ToListAsync();

        if (!perfumes.Any())
        {
            // Fallback to recent perfumes
            perfumes = await _db.Perfumes
                .Include(p => p.Brand)
                .OrderByDescending(p => p.CreatedAt)
                .Take(limit)
                .Select(p => new
                {
                    Id = p.Id,
                    Name = p.Name,
                    Brand = p.Brand.Name,
                    Gender = p.GenderProfile,
                    Image = p.ImageUrl
                })
                .ToListAsync();
        }

        return new
        {
            BasedOn = preferences,
            Recommendations = perfumes
        };
    }

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
        var perfumeId = GetGuid(args["perfumeId"]);
        var quantity = args.TryGetValue("quantity", out var q) ? GetInt32(q) : 1;

        var perfume = await _db.Perfumes
            .Include(p => p.Brand)
            .FirstOrDefaultAsync(p => p.Id == perfumeId);

        if (perfume == null)
            return new { Ok = false, Error = "Parfum introuvable" };

        // For perfumes, we don't track stock - always available
        for (int i = 0; i < quantity; i++)
        {
            await _cartService.AddToCartAsync(new ViewModels.CartItemVM
            {
                ProductId = perfumeId,
                ProductName = perfume.Name,
                BrandName = perfume.Brand.Name,
                Price = perfume.Price,
                Quantity = 1,
                ImageUrl = perfume.ImageUrl ?? ""
            });
        }

        return new
        {
            Ok = true,
            Added = $"{quantity}x {perfume.Brand.Name} - {perfume.Name}"
        };
    }

    private async Task<object> HandleRemoveAsync(Dictionary<string, object> args)
    {
        var productIdStr = args["productId"]?.ToString();
        if (!Guid.TryParse(productIdStr, out var productId))
            return new { Ok = false, Error = "Invalid productId" };

        await _cartService.RemoveAsync(productId);
        return new { Ok = true, Removed = productId };
    }

    private async Task<object> HandleIncreaseAsync(Dictionary<string, object> args)
    {
        var productIdStr = args["productId"]?.ToString();
        if (!Guid.TryParse(productIdStr, out var productId))
            return new { Ok = false, Error = "Invalid productId" };

        await _cartService.IncreaseAsync(productId);
        return new { Ok = true };
    }

    private async Task<object> HandleDecreaseAsync(Dictionary<string, object> args)
    {
        var productIdStr = args["productId"]?.ToString();
        if (!Guid.TryParse(productIdStr, out var productId))
            return new { Ok = false, Error = "Invalid productId" };

        await _cartService.DecreaseAsync(productId);
        return new { Ok = true };
    }

    private async Task<object> HandleAnalyzeCartAsync()
    {
        var cart = await _cartService.GetCartAsync();

        if (!cart.Any())
            return new { Empty = true, Conseil = "Découvrez nos parfums avec recommend_perfumes!" };

        var itemCount = cart.Sum(i => i.Quantity);

        return new
        {
            Articles = itemCount,
            Items = cart.Select(i => i.ProductName).ToList(),
            Conseil = "Contactez-nous pour un devis personnalisé"
        };
    }

    private async Task<object> HandleComparePerfumesAsync(Dictionary<string, object> args)
    {
        var idsObj = args["perfumeIds"];
        var ids = new List<Guid>();

        if (idsObj is JsonElement jsonElement && jsonElement.ValueKind == JsonValueKind.Array)
        {
            foreach (var el in jsonElement.EnumerateArray())
            {
                if (Guid.TryParse(el.GetString(), out var id))
                    ids.Add(id);
            }
        }

        if (ids.Count < 2)
            return new { Error = "Fournissez au moins 2 IDs de parfums" };

        var perfumes = await _db.Perfumes
            .Include(p => p.Brand)
            .Include(p => p.PerfumeFamilies).ThenInclude(pf => pf.Family)
            .Where(p => ids.Contains(p.Id))
            .Take(3)
            .Select(p => new
            {
                p.Id,
                p.Name,
                Brand = p.Brand.Name,
                Gender = p.GenderProfile,
                Intensity = p.Intensity,
                Longevity = p.Longevity,
                Families = p.PerfumeFamilies.Select(pf => pf.Family.Name).ToList()
            })
            .ToListAsync();

        if (perfumes.Count < 2)
            return new { Error = "Parfums introuvables" };

        return new
        {
            Parfums = perfumes,
            Conseil = "Chaque parfum a ses propres caractéristiques uniques!"
        };
    }

    private async Task<object> HandleGetBrandsAsync()
    {
        var brands = await _db.Brands
            .Select(b => new
            {
                b.Id,
                b.Name,
                PerfumeCount = b.Perfumes.Count
            })
            .OrderByDescending(b => b.PerfumeCount)
            .Take(10)
            .ToListAsync();

        return new { Brands = brands };
    }

    private async Task<object> HandleGetFamiliesAsync()
    {
        var families = await _db.Families
            .Select(f => new
            {
                f.Id,
                f.Name,
                f.Description,
                PerfumeCount = f.PerfumeFamilies.Count
            })
            .OrderByDescending(f => f.PerfumeCount)
            .Take(10)
            .ToListAsync();

        return new { Families = families };
    }

    private static int GetInt32(object value)
    {
        if (value is JsonElement element)
            return element.GetInt32();
        return Convert.ToInt32(value);
    }

    private static Guid GetGuid(object value)
    {
        if (value is JsonElement element)
            return Guid.Parse(element.GetString() ?? string.Empty);
        return Guid.Parse(value.ToString() ?? string.Empty);
    }
}
