using ALOud.Services.Rag.Models;
using Microsoft.AspNetCore.Mvc;

namespace ALOud.Controllers;

/// <summary>
/// Mock controller for testing AI Chat frontend without hitting real AI API
/// Access via: POST /api/ai/cart/mock
/// </summary>
[ApiController]
[Route("api/ai/cart/mock")]
public sealed class MockAiCartController : ControllerBase
{
    private static readonly Dictionary<string, RagResponse> MockResponses = new()
    {
        ["recommendations"] = new RagResponse
        {
            Answer = """
            Basé sur vos préférences, voici mes recommandations:
            ✨ **RawChemistry For Him, Pheromone Infused Cologne** [image:https://m.media-amazon.com/images/I/71DNQEfCz1L._AC_UL320_.jpg]
            Intense and captivating with pepper and incense
            Prix: 336.90 MAD - En stock

            ✨ **Gucci Guilty by Gucci for Men** [image:https://m.media-amazon.com/images/I/51N3JuhLs8L._AC_UL320_.jpg]
            Bold masculine fragrance with leather and tobacco accords
            Prix: 1645.00 MAD - En stock

            ✨ **Lattafa Asad Bourbon Unisex** [image:https://m.media-amazon.com/images/I/71nuj0MY91L._AC_UL320_.jpg]
            A sophisticated blend with woody and spicy notes
            Prix: 2380.00 MAD - Stock limité
            """,
            CartSnapshot = null
        },

        ["multiple"] = new RagResponse
        {
            Answer = """
            Voici des parfums frais pour l'été:

            1. **Nautica Voyage Eau De Toilette** [image:https://m.media-amazon.com/images/I/51S3W4tnDbL._AC_UL320_.jpg]
            Timeless elegance with cedar and vetiver
            Prix: 250.00 MAD

            2. **Gucci Guilty for Men** [image:https://m.media-amazon.com/images/I/510Da6RztXL._AC_UL320_.jpg]
            Fresh and energetic with citrus and aquatic notes            Fresh and energetic with citrus and aquatic notes Fresh and energetic with citrus and aquatic notes            Fresh and energetic with citrus and aquatic notes

            Prix: 863.10 MAD

            3. **GUESS Seductive Homme Blue** [image:https://m.media-amazon.com/images/I/71F4MviMigL._AC_UL320_.jpg]
            Fresh and energetic with citrus and aquatic notes
            Prix: 2518.00 MAD
            """,
            CartSnapshot = null
        },

        ["cart"] = new RagResponse
        {
            Answer = "J'ai ajouté 2 x Viktor&Rolf Spicebomb Extreme à votre panier.",
            CartSnapshot = new
            {
                items = new[]
                {
                    new { productId = 604, productName = "Viktor&Rolf - Spicebomb Extreme Eau de Parfum", quantity = 2, price = 1264.00m },
                    new { productId = 605, productName = "Lattafa Asad Bourbon Unisex", quantity = 1, price = 2380.00m }
                },
                totalPrice = 4908.00m,
                count = 3
            }
        },



        ["error"] = new RagResponse
        {
            Answer = "Désolé, je n'ai pas trouvé de produit avec l'ID 999. Essayez de rechercher des produits similaires.",
            CartSnapshot = null
        },

        ["single"] = new RagResponse
        {
            Answer = """
            Voici le parfum Viktor&Rolf Spicebomb Extreme:

            **Viktor&Rolf - Spicebomb Extreme Eau de Parfum** [image:https://m.media-amazon.com/images/I/61SGjKNZT0L._AC_UL320_.jpg]

            Description: Dynamic blend of grapefruit and woody notes
            Prix: 1264.00 MAD
            Stock: 46 unités disponibles
            """,
            CartSnapshot = null
        },
        ["details"] = new RagResponse
        {
            Answer = """
            📦 **Viktor&Rolf - Spicebomb Extreme Eau de Parfum** [image:https://m.media-amazon.com/images/I/61SGjKNZT0L._AC_UL320_.jpg]

            **Description:**
            Woody & Spicy cologne for men with notes of amber and vanilla. Dynamic blend of grapefruit and woody notes, perfect for the modern gentleman.

            **Détails:**
            • Prix: 1264.00 MAD
            • Stock: 46 unités disponibles
            • Type: Eau de Parfum
            • Notes: Ambre, vanille, bois, épices

            ✅ En stock - Livraison rapide disponible
            """,
            CartSnapshot = null
        }
    };

    [HttpPost]
    public ActionResult<RagResponse> Handle([FromBody] RagRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Message))
            return BadRequest("Message is required.");

        // Determine which mock response to return based on message content
        var message = request.Message.ToLower();

        if (message.Contains("detail") || message.Contains("info"))
            return Ok(MockResponses["details"]);

        if (message.Contains("recommand") || message.Contains("suggest") || message.Contains("propose"))
            return Ok(MockResponses["recommendations"]);

        if (message.Contains("cart") || message.Contains("panier") || message.Contains("add") || message.Contains("ajout"))
            return Ok(MockResponses["cart"]);

        if (message.Contains("error") || message.Contains("erreur"))
            return Ok(MockResponses["error"]);

        if (message.Contains("summer") || message.Contains("été") || message.Contains("fresh") || message.Contains("list"))
            return Ok(MockResponses["multiple"]);

        // Default to single product response
        return Ok(MockResponses["single"]);
    }

    [HttpGet("list")]
    public ActionResult<Dictionary<string, string>> ListMockResponses()
    {
        return Ok(MockResponses.Keys.Select(k => new { key = k, preview = MockResponses[k].Answer?[..Math.Min(50, MockResponses[k].Answer?.Length ?? 0)] }));
    }
}
