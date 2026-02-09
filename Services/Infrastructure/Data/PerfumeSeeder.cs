using Microsoft.EntityFrameworkCore;
using ALOud.Data;

namespace ALOud.Services.Data
{
    public static class PerfumeSeeder
    {
        public static async Task SeedAsync(ALOudDbContext context)
        {
            // Check if data already exists
            if (await context.Perfumes.AnyAsync())
            {
                Console.WriteLine("Database already contains perfumes. Skipping seed.");
                return;
            }

            Console.WriteLine("Seeding perfume database...");

            // =====================================================
            // BRANDS
            // =====================================================
            var brands = new Dictionary<string, Models.Brand>
            {
                ["Dior"] = new Models.Brand { Id = Guid.NewGuid(), Name = "Dior" },
                ["Chanel"] = new Models.Brand { Id = Guid.NewGuid(), Name = "Chanel" },
                ["Creed"] = new Models.Brand { Id = Guid.NewGuid(), Name = "Creed" },
                ["Tom Ford"] = new Models.Brand { Id = Guid.NewGuid(), Name = "Tom Ford" },
                ["Giorgio Armani"] = new Models.Brand { Id = Guid.NewGuid(), Name = "Giorgio Armani" },
                ["Lancôme"] = new Models.Brand { Id = Guid.NewGuid(), Name = "Lancôme" },
                ["Yves Saint Laurent"] = new Models.Brand { Id = Guid.NewGuid(), Name = "Yves Saint Laurent" },
                ["Viktor & Rolf"] = new Models.Brand { Id = Guid.NewGuid(), Name = "Viktor & Rolf" },
                ["Maison Francis Kurkdjian"] = new Models.Brand { Id = Guid.NewGuid(), Name = "Maison Francis Kurkdjian" },
                ["Le Labo"] = new Models.Brand { Id = Guid.NewGuid(), Name = "Le Labo" },
                ["Paco Rabanne"] = new Models.Brand { Id = Guid.NewGuid(), Name = "Paco Rabanne" },
                ["Dolce & Gabbana"] = new Models.Brand { Id = Guid.NewGuid(), Name = "Dolce & Gabbana" },
                ["Versace"] = new Models.Brand { Id = Guid.NewGuid(), Name = "Versace" },
            };
            context.Brands.AddRange(brands.Values);

            // =====================================================
            // FAMILIES
            // =====================================================
            var families = new Dictionary<string, Models.Family>
            {
                ["Aromatic"] = new Models.Family { Id = Guid.NewGuid(), Name = "Aromatic" },
                ["Fougère"] = new Models.Family { Id = Guid.NewGuid(), Name = "Fougère" },
                ["Woody"] = new Models.Family { Id = Guid.NewGuid(), Name = "Woody" },
                ["Fruity"] = new Models.Family { Id = Guid.NewGuid(), Name = "Fruity" },
                ["Chypre"] = new Models.Family { Id = Guid.NewGuid(), Name = "Chypre" },
                ["Oriental"] = new Models.Family { Id = Guid.NewGuid(), Name = "Oriental" },
                ["Aquatic"] = new Models.Family { Id = Guid.NewGuid(), Name = "Aquatic" },
                ["Floral"] = new Models.Family { Id = Guid.NewGuid(), Name = "Floral" },
                ["Gourmand"] = new Models.Family { Id = Guid.NewGuid(), Name = "Gourmand" },
                ["Amber"] = new Models.Family { Id = Guid.NewGuid(), Name = "Amber" },
                ["Spicy"] = new Models.Family { Id = Guid.NewGuid(), Name = "Spicy" },
                ["Leather"] = new Models.Family { Id = Guid.NewGuid(), Name = "Leather" },
                ["Fresh"] = new Models.Family { Id = Guid.NewGuid(), Name = "Fresh" },
                ["Citrus"] = new Models.Family { Id = Guid.NewGuid(), Name = "Citrus" },
            };
            context.Families.AddRange(families.Values);

            // =====================================================
            // NOTES
            // =====================================================
            var notes = new Dictionary<string, Models.Note>
            {
                // Top notes
                ["Bergamote"] = new Models.Note { Id = Guid.NewGuid(), Name = "Bergamote", Category = "Top" },
                ["Poivre"] = new Models.Note { Id = Guid.NewGuid(), Name = "Poivre", Category = "Top" },
                ["Citron"] = new Models.Note { Id = Guid.NewGuid(), Name = "Citron", Category = "Top" },
                ["Menthe"] = new Models.Note { Id = Guid.NewGuid(), Name = "Menthe", Category = "Top" },
                ["Pamplemousse"] = new Models.Note { Id = Guid.NewGuid(), Name = "Pamplemousse", Category = "Top" },
                ["Ananas"] = new Models.Note { Id = Guid.NewGuid(), Name = "Ananas", Category = "Top" },
                ["Pomme"] = new Models.Note { Id = Guid.NewGuid(), Name = "Pomme", Category = "Top" },
                ["Cassis"] = new Models.Note { Id = Guid.NewGuid(), Name = "Cassis", Category = "Top" },
                ["Mandarine"] = new Models.Note { Id = Guid.NewGuid(), Name = "Mandarine", Category = "Top" },
                ["Orange"] = new Models.Note { Id = Guid.NewGuid(), Name = "Orange", Category = "Top" },
                ["Poire"] = new Models.Note { Id = Guid.NewGuid(), Name = "Poire", Category = "Top" },
                ["Café"] = new Models.Note { Id = Guid.NewGuid(), Name = "Café", Category = "Top" },
                ["Thé"] = new Models.Note { Id = Guid.NewGuid(), Name = "Thé", Category = "Top" },
                ["Safran"] = new Models.Note { Id = Guid.NewGuid(), Name = "Safran", Category = "Top" },
                ["Cardamome"] = new Models.Note { Id = Guid.NewGuid(), Name = "Cardamome", Category = "Top" },
                ["Néroli"] = new Models.Note { Id = Guid.NewGuid(), Name = "Néroli", Category = "Top" },
                ["Aquatique"] = new Models.Note { Id = Guid.NewGuid(), Name = "Aquatique", Category = "Top" },
                ["Bois de Rose"] = new Models.Note { Id = Guid.NewGuid(), Name = "Bois de Rose", Category = "Top" },
                ["Poivre Sichuan"] = new Models.Note { Id = Guid.NewGuid(), Name = "Poivre Sichuan", Category = "Top" },
                ["Coing"] = new Models.Note { Id = Guid.NewGuid(), Name = "Coing", Category = "Top" },
                ["Marine"] = new Models.Note { Id = Guid.NewGuid(), Name = "Marine", Category = "Top" },
                ["Tabac"] = new Models.Note { Id = Guid.NewGuid(), Name = "Tabac", Category = "Top" },
                ["Épices"] = new Models.Note { Id = Guid.NewGuid(), Name = "Épices", Category = "Top" },
                ["Carvi"] = new Models.Note { Id = Guid.NewGuid(), Name = "Carvi", Category = "Top" },
                ["Violet"] = new Models.Note { Id = Guid.NewGuid(), Name = "Violet", Category = "Top" },
                ["Rose Noire"] = new Models.Note { Id = Guid.NewGuid(), Name = "Rose Noire", Category = "Top" },
                ["Cerise Noire"] = new Models.Note { Id = Guid.NewGuid(), Name = "Cerise Noire", Category = "Top" },
                ["Liqueur de Cerise"] = new Models.Note { Id = Guid.NewGuid(), Name = "Liqueur de Cerise", Category = "Top" },

                // Middle/Heart notes
                ["Lavande"] = new Models.Note { Id = Guid.NewGuid(), Name = "Lavande", Category = "Middle" },
                ["Géranium"] = new Models.Note { Id = Guid.NewGuid(), Name = "Géranium", Category = "Middle" },
                ["Jasmin"] = new Models.Note { Id = Guid.NewGuid(), Name = "Jasmin", Category = "Middle" },
                ["Rose"] = new Models.Note { Id = Guid.NewGuid(), Name = "Rose", Category = "Middle" },
                ["Gingembre"] = new Models.Note { Id = Guid.NewGuid(), Name = "Gingembre", Category = "Middle" },
                ["Bouleau"] = new Models.Note { Id = Guid.NewGuid(), Name = "Bouleau", Category = "Middle" },
                ["Oud"] = new Models.Note { Id = Guid.NewGuid(), Name = "Oud", Category = "Middle" },
                ["Sauge"] = new Models.Note { Id = Guid.NewGuid(), Name = "Sauge", Category = "Middle" },
                ["Romarin"] = new Models.Note { Id = Guid.NewGuid(), Name = "Romarin", Category = "Middle" },
                ["Rose de Grasse"] = new Models.Note { Id = Guid.NewGuid(), Name = "Rose de Grasse", Category = "Middle" },
                ["Pivoine"] = new Models.Note { Id = Guid.NewGuid(), Name = "Pivoine", Category = "Middle" },
                ["Iris"] = new Models.Note { Id = Guid.NewGuid(), Name = "Iris", Category = "Middle" },
                ["Litchi"] = new Models.Note { Id = Guid.NewGuid(), Name = "Litchi", Category = "Middle" },
                ["Fleur d'Oranger"] = new Models.Note { Id = Guid.NewGuid(), Name = "Fleur d'Oranger", Category = "Middle" },
                ["Jasmin Sambac"] = new Models.Note { Id = Guid.NewGuid(), Name = "Jasmin Sambac", Category = "Middle" },
                ["Rose Centifolia"] = new Models.Note { Id = Guid.NewGuid(), Name = "Rose Centifolia", Category = "Middle" },
                ["Freesia"] = new Models.Note { Id = Guid.NewGuid(), Name = "Freesia", Category = "Middle" },
                ["Orchidée"] = new Models.Note { Id = Guid.NewGuid(), Name = "Orchidée", Category = "Middle" },
                ["Cannelle"] = new Models.Note { Id = Guid.NewGuid(), Name = "Cannelle", Category = "Middle" },
                ["Laurier"] = new Models.Note { Id = Guid.NewGuid(), Name = "Laurier", Category = "Middle" },
                ["Bambou"] = new Models.Note { Id = Guid.NewGuid(), Name = "Bambou", Category = "Middle" },
                ["Rose Blanche"] = new Models.Note { Id = Guid.NewGuid(), Name = "Rose Blanche", Category = "Middle" },
                ["Jacinthe"] = new Models.Note { Id = Guid.NewGuid(), Name = "Jacinthe", Category = "Middle" },
                ["Truffe"] = new Models.Note { Id = Guid.NewGuid(), Name = "Truffe", Category = "Middle" },
                ["Cacao"] = new Models.Note { Id = Guid.NewGuid(), Name = "Cacao", Category = "Middle" },
                ["Papyrus"] = new Models.Note { Id = Guid.NewGuid(), Name = "Papyrus", Category = "Middle" },
                ["Cerise Griotte"] = new Models.Note { Id = Guid.NewGuid(), Name = "Cerise Griotte", Category = "Middle" },
                ["Amande Amère"] = new Models.Note { Id = Guid.NewGuid(), Name = "Amande Amère", Category = "Middle" },
                ["Santal"] = new Models.Note { Id = Guid.NewGuid(), Name = "Santal", Category = "Middle" },

                // Base notes
                ["Ambroxan"] = new Models.Note { Id = Guid.NewGuid(), Name = "Ambroxan", Category = "Base" },
                ["Cèdre"] = new Models.Note { Id = Guid.NewGuid(), Name = "Cèdre", Category = "Base" },
                ["Encens"] = new Models.Note { Id = Guid.NewGuid(), Name = "Encens", Category = "Base" },
                ["Mousse de Chêne"] = new Models.Note { Id = Guid.NewGuid(), Name = "Mousse de Chêne", Category = "Base" },
                ["Musc"] = new Models.Note { Id = Guid.NewGuid(), Name = "Musc", Category = "Base" },
                ["Ambre"] = new Models.Note { Id = Guid.NewGuid(), Name = "Ambre", Category = "Base" },
                ["Vétiver"] = new Models.Note { Id = Guid.NewGuid(), Name = "Vétiver", Category = "Base" },
                ["Tonka"] = new Models.Note { Id = Guid.NewGuid(), Name = "Tonka", Category = "Base" },
                ["Patchouli"] = new Models.Note { Id = Guid.NewGuid(), Name = "Patchouli", Category = "Base" },
                ["Musc Blanc"] = new Models.Note { Id = Guid.NewGuid(), Name = "Musc Blanc", Category = "Base" },
                ["Praline"] = new Models.Note { Id = Guid.NewGuid(), Name = "Praline", Category = "Base" },
                ["Vanille"] = new Models.Note { Id = Guid.NewGuid(), Name = "Vanille", Category = "Base" },
                ["Cuir"] = new Models.Note { Id = Guid.NewGuid(), Name = "Cuir", Category = "Base" },
                ["Ambre Blanc"] = new Models.Note { Id = Guid.NewGuid(), Name = "Ambre Blanc", Category = "Base" },
                ["Bois"] = new Models.Note { Id = Guid.NewGuid(), Name = "Bois", Category = "Base" },
                ["Bois de Gaïac"] = new Models.Note { Id = Guid.NewGuid(), Name = "Bois de Gaïac", Category = "Base" },
                ["Ambre Gris"] = new Models.Note { Id = Guid.NewGuid(), Name = "Ambre Gris", Category = "Base" },
                ["Cèdre Blanc"] = new Models.Note { Id = Guid.NewGuid(), Name = "Cèdre Blanc", Category = "Base" },
                ["Fruits Secs"] = new Models.Note { Id = Guid.NewGuid(), Name = "Fruits Secs", Category = "Base" },
                ["Bois Précieux"] = new Models.Note { Id = Guid.NewGuid(), Name = "Bois Précieux", Category = "Base" },
            };
            context.Notes.AddRange(notes.Values);

            // =====================================================
            // ACCORDS
            // =====================================================
            var accords = new Dictionary<string, Models.Accord>
            {
                ["Aromatic"] = new Models.Accord { Id = Guid.NewGuid(), Name = "Aromatic" },
                ["Fresh Spicy"] = new Models.Accord { Id = Guid.NewGuid(), Name = "Fresh Spicy" },
                ["Woody"] = new Models.Accord { Id = Guid.NewGuid(), Name = "Woody" },
                ["Fresh"] = new Models.Accord { Id = Guid.NewGuid(), Name = "Fresh" },
                ["Citrus"] = new Models.Accord { Id = Guid.NewGuid(), Name = "Citrus" },
                ["Fruity"] = new Models.Accord { Id = Guid.NewGuid(), Name = "Fruity" },
                ["Smoky"] = new Models.Accord { Id = Guid.NewGuid(), Name = "Smoky" },
                ["Oud"] = new Models.Accord { Id = Guid.NewGuid(), Name = "Oud" },
                ["Spicy"] = new Models.Accord { Id = Guid.NewGuid(), Name = "Spicy" },
                ["Aquatic"] = new Models.Accord { Id = Guid.NewGuid(), Name = "Aquatic" },
                ["Floral"] = new Models.Accord { Id = Guid.NewGuid(), Name = "Floral" },
                ["Powdery"] = new Models.Accord { Id = Guid.NewGuid(), Name = "Powdery" },
                ["Oriental"] = new Models.Accord { Id = Guid.NewGuid(), Name = "Oriental" },
                ["Gourmand"] = new Models.Accord { Id = Guid.NewGuid(), Name = "Gourmand" },
                ["Sweet"] = new Models.Accord { Id = Guid.NewGuid(), Name = "Sweet" },
                ["Coffee"] = new Models.Accord { Id = Guid.NewGuid(), Name = "Coffee" },
                ["Vanilla"] = new Models.Accord { Id = Guid.NewGuid(), Name = "Vanilla" },
                ["Amber"] = new Models.Accord { Id = Guid.NewGuid(), Name = "Amber" },
                ["Tobacco"] = new Models.Accord { Id = Guid.NewGuid(), Name = "Tobacco" },
                ["Warm Spicy"] = new Models.Accord { Id = Guid.NewGuid(), Name = "Warm Spicy" },
                ["Sandalwood"] = new Models.Accord { Id = Guid.NewGuid(), Name = "Sandalwood" },
                ["Leather"] = new Models.Accord { Id = Guid.NewGuid(), Name = "Leather" },
                ["Rose"] = new Models.Accord { Id = Guid.NewGuid(), Name = "Rose" },
                ["Earthy"] = new Models.Accord { Id = Guid.NewGuid(), Name = "Earthy" },
                ["Cherry"] = new Models.Accord { Id = Guid.NewGuid(), Name = "Cherry" },
                ["Almond"] = new Models.Accord { Id = Guid.NewGuid(), Name = "Almond" },
            };
            context.Accords.AddRange(accords.Values);

            // =====================================================
            // TAGS
            // =====================================================
            var tags = new Dictionary<string, Models.Tag>
            {
                ["Best-seller"] = new Models.Tag { Id = Guid.NewGuid(), Name = "Best-seller" },
                ["Masculin"] = new Models.Tag { Id = Guid.NewGuid(), Name = "Masculin" },
                ["Signature"] = new Models.Tag { Id = Guid.NewGuid(), Name = "Signature" },
                ["Moderne"] = new Models.Tag { Id = Guid.NewGuid(), Name = "Moderne" },
                ["Classique"] = new Models.Tag { Id = Guid.NewGuid(), Name = "Classique" },
                ["Élégant"] = new Models.Tag { Id = Guid.NewGuid(), Name = "Élégant" },
                ["Intemporel"] = new Models.Tag { Id = Guid.NewGuid(), Name = "Intemporel" },
                ["Luxueux"] = new Models.Tag { Id = Guid.NewGuid(), Name = "Luxueux" },
                ["Iconique"] = new Models.Tag { Id = Guid.NewGuid(), Name = "Iconique" },
                ["Prestige"] = new Models.Tag { Id = Guid.NewGuid(), Name = "Prestige" },
                ["Oriental"] = new Models.Tag { Id = Guid.NewGuid(), Name = "Oriental" },
                ["Boisé"] = new Models.Tag { Id = Guid.NewGuid(), Name = "Boisé" },
                ["Frais"] = new Models.Tag { Id = Guid.NewGuid(), Name = "Frais" },
                ["Féminin"] = new Models.Tag { Id = Guid.NewGuid(), Name = "Féminin" },
                ["Romantique"] = new Models.Tag { Id = Guid.NewGuid(), Name = "Romantique" },
                ["Sensuel"] = new Models.Tag { Id = Guid.NewGuid(), Name = "Sensuel" },
                ["Gourmand"] = new Models.Tag { Id = Guid.NewGuid(), Name = "Gourmand" },
                ["Doux"] = new Models.Tag { Id = Guid.NewGuid(), Name = "Doux" },
                ["Addictif"] = new Models.Tag { Id = Guid.NewGuid(), Name = "Addictif" },
                ["Rock"] = new Models.Tag { Id = Guid.NewGuid(), Name = "Rock" },
                ["Floral"] = new Models.Tag { Id = Guid.NewGuid(), Name = "Floral" },
                ["Intense"] = new Models.Tag { Id = Guid.NewGuid(), Name = "Intense" },
                ["Opulent"] = new Models.Tag { Id = Guid.NewGuid(), Name = "Opulent" },
                ["Chaleureux"] = new Models.Tag { Id = Guid.NewGuid(), Name = "Chaleureux" },
                ["Culte"] = new Models.Tag { Id = Guid.NewGuid(), Name = "Culte" },
                ["Minimaliste"] = new Models.Tag { Id = Guid.NewGuid(), Name = "Minimaliste" },
                ["NYC"] = new Models.Tag { Id = Guid.NewGuid(), Name = "NYC" },
                ["Sombre"] = new Models.Tag { Id = Guid.NewGuid(), Name = "Sombre" },
                ["Décadent"] = new Models.Tag { Id = Guid.NewGuid(), Name = "Décadent" },
                ["Provocant"] = new Models.Tag { Id = Guid.NewGuid(), Name = "Provocant" },
                ["Fruité"] = new Models.Tag { Id = Guid.NewGuid(), Name = "Fruité" },
                ["Audacieux"] = new Models.Tag { Id = Guid.NewGuid(), Name = "Audacieux" },
                ["Séducteur"] = new Models.Tag { Id = Guid.NewGuid(), Name = "Séducteur" },
                ["Festif"] = new Models.Tag { Id = Guid.NewGuid(), Name = "Festif" },
                ["Sportif"] = new Models.Tag { Id = Guid.NewGuid(), Name = "Sportif" },
                ["Dynamique"] = new Models.Tag { Id = Guid.NewGuid(), Name = "Dynamique" },
                ["Champion"] = new Models.Tag { Id = Guid.NewGuid(), Name = "Champion" },
                ["Estival"] = new Models.Tag { Id = Guid.NewGuid(), Name = "Estival" },
                ["Méditerranéen"] = new Models.Tag { Id = Guid.NewGuid(), Name = "Méditerranéen" },
                ["Italien"] = new Models.Tag { Id = Guid.NewGuid(), Name = "Italien" },
                ["Décontracté"] = new Models.Tag { Id = Guid.NewGuid(), Name = "Décontracté" },
                ["Délicat"] = new Models.Tag { Id = Guid.NewGuid(), Name = "Délicat" },
                ["Tendre"] = new Models.Tag { Id = Guid.NewGuid(), Name = "Tendre" },
                ["Printemps"] = new Models.Tag { Id = Guid.NewGuid(), Name = "Printemps" },
                ["Raffiné"] = new Models.Tag { Id = Guid.NewGuid(), Name = "Raffiné" },
                ["Unisexe"] = new Models.Tag { Id = Guid.NewGuid(), Name = "Unisexe" },
            };
            context.Tags.AddRange(tags.Values);

            // =====================================================
            // SEASONS
            // =====================================================
            var seasons = new Dictionary<string, Models.Season>
            {
                ["Printemps"] = new Models.Season { Id = Guid.NewGuid(), Name = "Printemps" },
                ["Été"] = new Models.Season { Id = Guid.NewGuid(), Name = "Été" },
                ["Automne"] = new Models.Season { Id = Guid.NewGuid(), Name = "Automne" },
                ["Hiver"] = new Models.Season { Id = Guid.NewGuid(), Name = "Hiver" },
            };
            context.Seasons.AddRange(seasons.Values);

            // =====================================================
            // OCCASIONS
            // =====================================================
            var occasions = new Dictionary<string, Models.Occasion>
            {
                ["Quotidien"] = new Models.Occasion { Id = Guid.NewGuid(), Name = "Quotidien" },
                ["Bureau"] = new Models.Occasion { Id = Guid.NewGuid(), Name = "Bureau" },
                ["Soirée"] = new Models.Occasion { Id = Guid.NewGuid(), Name = "Soirée" },
                ["Rendez-vous"] = new Models.Occasion { Id = Guid.NewGuid(), Name = "Rendez-vous" },
                ["Mariage"] = new Models.Occasion { Id = Guid.NewGuid(), Name = "Mariage" },
                ["Sport"] = new Models.Occasion { Id = Guid.NewGuid(), Name = "Sport" },
                ["Vacances"] = new Models.Occasion { Id = Guid.NewGuid(), Name = "Vacances" },
                ["Cérémonie"] = new Models.Occasion { Id = Guid.NewGuid(), Name = "Cérémonie" },
            };
            context.Occasions.AddRange(occasions.Values);

            await context.SaveChangesAsync();
            Console.WriteLine("Master data seeded.");

            // =====================================================
            // PERFUMES
            // =====================================================
            var perfumeData = new[]
            {
                new {
                    Name = "Sauvage",
                    Brand = "Dior",
                    Description = "Sauvage est une fragrance audacieuse et radicalement fraîche composée de notes de bergamote de Calabre et d'un accord ambroxan boisé. Un parfum noble et masculin qui évoque les grands espaces.",
                    ImageUrl = "https://fimgs.net/mdimg/perfume/375x500.56585.jpg",
                    Gender = "Male",
                    Intensity = "Forte",
                    Longevity = "8-12 heures",
                    Sillage = "Fort",
                    PriceRange = "$$$",
                    Price = 1200m,
                    Stock = 45,
                    Families = new[] { "Aromatic", "Fougère" },
                    Notes = new[] { ("Bergamote", "Top"), ("Poivre", "Top"), ("Lavande", "Middle"), ("Géranium", "Middle"), ("Ambroxan", "Base"), ("Cèdre", "Base") },
                    Accords = new[] { ("Aromatic", "Strong"), ("Fresh Spicy", "Medium"), ("Woody", "Medium") },
                    Tags = new[] { "Best-seller", "Masculin", "Signature", "Moderne" },
                    Seasons = new[] { "Printemps", "Été", "Automne" },
                    Occasions = new[] { "Quotidien", "Bureau", "Soirée" }
                },
                new {
                    Name = "Bleu de Chanel",
                    Brand = "Chanel",
                    Description = "Bleu de Chanel est un parfum boisé aromatique qui exprime la liberté. Un homme qui s'affranchit des conventions et qui cultive ce qu'il a d'unique.",
                    ImageUrl = "https://fimgs.net/mdimg/perfume/375x500.25967.jpg",
                    Gender = "Male",
                    Intensity = "Modérée",
                    Longevity = "6-8 heures",
                    Sillage = "Modéré",
                    PriceRange = "$$$",
                    Price = 1350m,
                    Stock = 38,
                    Families = new[] { "Woody", "Aromatic" },
                    Notes = new[] { ("Citron", "Top"), ("Menthe", "Top"), ("Pamplemousse", "Top"), ("Gingembre", "Middle"), ("Jasmin", "Middle"), ("Cèdre", "Base"), ("Santal", "Base"), ("Encens", "Base") },
                    Accords = new[] { ("Woody", "Strong"), ("Fresh", "Medium"), ("Citrus", "Medium") },
                    Tags = new[] { "Classique", "Élégant", "Signature", "Intemporel" },
                    Seasons = new[] { "Printemps", "Automne" },
                    Occasions = new[] { "Bureau", "Soirée", "Rendez-vous" }
                },
                new {
                    Name = "Aventus",
                    Brand = "Creed",
                    Description = "Aventus célèbre la force, la vision et le succès. Ce parfum emblématique s'ouvre sur des notes fraîches d'ananas et de pomme, révélant un cœur de bouleau et de jasmin.",
                    ImageUrl = "https://fimgs.net/mdimg/perfume/375x500.9828.jpg",
                    Gender = "Male",
                    Intensity = "Forte",
                    Longevity = "+12 heures",
                    Sillage = "Énorme",
                    PriceRange = "$$$$",
                    Price = 3500m,
                    Stock = 15,
                    Families = new[] { "Fruity", "Chypre" },
                    Notes = new[] { ("Ananas", "Top"), ("Pomme", "Top"), ("Cassis", "Top"), ("Bergamote", "Top"), ("Rose", "Middle"), ("Jasmin", "Middle"), ("Bouleau", "Middle"), ("Mousse de Chêne", "Base"), ("Musc", "Base"), ("Ambre", "Base") },
                    Accords = new[] { ("Fruity", "Strong"), ("Smoky", "Medium"), ("Woody", "Strong") },
                    Tags = new[] { "Luxueux", "Iconique", "Signature", "Prestige" },
                    Seasons = new[] { "Printemps", "Été", "Automne" },
                    Occasions = new[] { "Cérémonie", "Soirée", "Bureau" }
                },
                new {
                    Name = "Oud Wood",
                    Brand = "Tom Ford",
                    Description = "Oud Wood est une interprétation moderne et rare du bois d'oud. L'exotisme du bois d'oud se mêle au bois de rose, au cardamome et au bois de santal.",
                    ImageUrl = "https://fimgs.net/mdimg/perfume/375x500.7829.jpg",
                    Gender = "Unisex",
                    Intensity = "Forte",
                    Longevity = "8-12 heures",
                    Sillage = "Fort",
                    PriceRange = "$$$$",
                    Price = 2800m,
                    Stock = 22,
                    Families = new[] { "Woody", "Oriental" },
                    Notes = new[] { ("Bois de Rose", "Top"), ("Cardamome", "Top"), ("Poivre Sichuan", "Top"), ("Oud", "Middle"), ("Santal", "Middle"), ("Vétiver", "Base"), ("Ambre", "Base"), ("Tonka", "Base") },
                    Accords = new[] { ("Oud", "Strong"), ("Woody", "Strong"), ("Spicy", "Medium") },
                    Tags = new[] { "Oriental", "Luxueux", "Boisé", "Signature" },
                    Seasons = new[] { "Automne", "Hiver" },
                    Occasions = new[] { "Soirée", "Cérémonie", "Rendez-vous" }
                },
                new {
                    Name = "Acqua di Gio Profumo",
                    Brand = "Giorgio Armani",
                    Description = "Acqua di Gio Profumo est une interprétation plus intense et sophistiquée du classique Acqua di Gio. Un mélange d'aquatique, d'encens et de notes boisées.",
                    ImageUrl = "https://fimgs.net/mdimg/perfume/375x500.28900.jpg",
                    Gender = "Male",
                    Intensity = "Modérée",
                    Longevity = "6-8 heures",
                    Sillage = "Modéré",
                    PriceRange = "$$$",
                    Price = 950m,
                    Stock = 52,
                    Families = new[] { "Aquatic", "Aromatic" },
                    Notes = new[] { ("Bergamote", "Top"), ("Aquatique", "Top"), ("Géranium", "Middle"), ("Sauge", "Middle"), ("Romarin", "Middle"), ("Encens", "Base"), ("Patchouli", "Base"), ("Ambre", "Base") },
                    Accords = new[] { ("Aquatic", "Strong"), ("Aromatic", "Medium"), ("Woody", "Medium") },
                    Tags = new[] { "Frais", "Classique", "Élégant", "Intemporel" },
                    Seasons = new[] { "Printemps", "Été" },
                    Occasions = new[] { "Quotidien", "Bureau", "Vacances" }
                },
                new {
                    Name = "Miss Dior",
                    Brand = "Dior",
                    Description = "Miss Dior est un chypre fleuri moderne qui célèbre l'amour et la fraîcheur. Des notes de rose de Grasse et de pivoine s'épanouissent sur un fond de patchouli.",
                    ImageUrl = "https://fimgs.net/mdimg/perfume/375x500.48270.jpg",
                    Gender = "Female",
                    Intensity = "Modérée",
                    Longevity = "6-8 heures",
                    Sillage = "Modéré",
                    PriceRange = "$$$",
                    Price = 1150m,
                    Stock = 48,
                    Families = new[] { "Floral", "Chypre" },
                    Notes = new[] { ("Mandarine", "Top"), ("Bergamote", "Top"), ("Rose de Grasse", "Middle"), ("Pivoine", "Middle"), ("Iris", "Middle"), ("Patchouli", "Base"), ("Musc Blanc", "Base") },
                    Accords = new[] { ("Floral", "Strong"), ("Powdery", "Medium"), ("Fresh", "Medium") },
                    Tags = new[] { "Féminin", "Romantique", "Élégant", "Classique" },
                    Seasons = new[] { "Printemps", "Automne" },
                    Occasions = new[] { "Rendez-vous", "Bureau", "Cérémonie" }
                },
                new {
                    Name = "Coco Mademoiselle",
                    Brand = "Chanel",
                    Description = "Coco Mademoiselle est un oriental frais irrésistible et imprévisible. Une essence fraîche et pétillante d'orange et de jasmin, avec un fond sensuel de patchouli.",
                    ImageUrl = "https://fimgs.net/mdimg/perfume/375x500.611.jpg",
                    Gender = "Female",
                    Intensity = "Modérée",
                    Longevity = "6-8 heures",
                    Sillage = "Modéré",
                    PriceRange = "$$$",
                    Price = 1400m,
                    Stock = 35,
                    Families = new[] { "Oriental", "Floral" },
                    Notes = new[] { ("Orange", "Top"), ("Bergamote", "Top"), ("Rose", "Middle"), ("Jasmin", "Middle"), ("Litchi", "Middle"), ("Patchouli", "Base"), ("Vétiver", "Base"), ("Musc Blanc", "Base") },
                    Accords = new[] { ("Fresh", "Strong"), ("Floral", "Strong"), ("Oriental", "Medium") },
                    Tags = new[] { "Iconique", "Féminin", "Sensuel", "Best-seller" },
                    Seasons = new[] { "Printemps", "Été", "Automne" },
                    Occasions = new[] { "Quotidien", "Bureau", "Soirée" }
                },
                new {
                    Name = "La Vie Est Belle",
                    Brand = "Lancôme",
                    Description = "La Vie Est Belle est une déclaration au bonheur. Un iris gourmand magnifié par un cœur de jasmin et fleur d'oranger, sur un fond de patchouli et de praline.",
                    ImageUrl = "https://fimgs.net/mdimg/perfume/375x500.15255.jpg",
                    Gender = "Female",
                    Intensity = "Modérée",
                    Longevity = "8-12 heures",
                    Sillage = "Fort",
                    PriceRange = "$$$",
                    Price = 980m,
                    Stock = 60,
                    Families = new[] { "Gourmand", "Floral" },
                    Notes = new[] { ("Cassis", "Top"), ("Poire", "Top"), ("Iris", "Middle"), ("Jasmin", "Middle"), ("Fleur d'Oranger", "Middle"), ("Praline", "Base"), ("Patchouli", "Base"), ("Vanille", "Base") },
                    Accords = new[] { ("Gourmand", "Strong"), ("Floral", "Medium"), ("Sweet", "Strong") },
                    Tags = new[] { "Gourmand", "Féminin", "Doux", "Best-seller" },
                    Seasons = new[] { "Automne", "Hiver" },
                    Occasions = new[] { "Quotidien", "Rendez-vous", "Soirée" }
                },
                new {
                    Name = "Black Opium",
                    Brand = "Yves Saint Laurent",
                    Description = "Black Opium est une overdose de café et de vanille, une addiction féminine et rock. Un parfum mystérieux et envoûtant qui marie café noir, fleur d'oranger et vanille.",
                    ImageUrl = "https://fimgs.net/mdimg/perfume/375x500.25324.jpg",
                    Gender = "Female",
                    Intensity = "Forte",
                    Longevity = "8-12 heures",
                    Sillage = "Fort",
                    PriceRange = "$$$",
                    Price = 1100m,
                    Stock = 42,
                    Families = new[] { "Oriental", "Gourmand" },
                    Notes = new[] { ("Café", "Top"), ("Mandarine", "Top"), ("Poire", "Top"), ("Fleur d'Oranger", "Middle"), ("Jasmin", "Middle"), ("Vanille", "Base"), ("Cèdre", "Base"), ("Patchouli", "Base") },
                    Accords = new[] { ("Coffee", "Strong"), ("Vanilla", "Strong"), ("Sweet", "Medium") },
                    Tags = new[] { "Addictif", "Sensuel", "Moderne", "Rock" },
                    Seasons = new[] { "Automne", "Hiver" },
                    Occasions = new[] { "Soirée", "Rendez-vous", "Cérémonie" }
                },
                new {
                    Name = "Flowerbomb",
                    Brand = "Viktor & Rolf",
                    Description = "Flowerbomb est une explosion florale qui transforme le négatif en positif. Un bouquet intense de jasmin, rose, freesia et orchidée sur un fond gourmand de patchouli.",
                    ImageUrl = "https://fimgs.net/mdimg/perfume/375x500.2619.jpg",
                    Gender = "Female",
                    Intensity = "Forte",
                    Longevity = "8-12 heures",
                    Sillage = "Fort",
                    PriceRange = "$$$",
                    Price = 1050m,
                    Stock = 55,
                    Families = new[] { "Floral", "Oriental" },
                    Notes = new[] { ("Thé", "Top"), ("Bergamote", "Top"), ("Jasmin Sambac", "Middle"), ("Rose Centifolia", "Middle"), ("Freesia", "Middle"), ("Orchidée", "Middle"), ("Patchouli", "Base"), ("Musc", "Base") },
                    Accords = new[] { ("Floral", "Strong"), ("Sweet", "Strong"), ("Powdery", "Medium") },
                    Tags = new[] { "Floral", "Intense", "Féminin", "Signature" },
                    Seasons = new[] { "Automne", "Hiver", "Printemps" },
                    Occasions = new[] { "Soirée", "Rendez-vous", "Cérémonie" }
                },
                new {
                    Name = "Baccarat Rouge 540",
                    Brand = "Maison Francis Kurkdjian",
                    Description = "Baccarat Rouge 540 est une création magistrale qui fusionne le jasmin, le safran et le bois de cèdre avec une overdose d'ambre et de musc.",
                    ImageUrl = "https://fimgs.net/mdimg/perfume/375x500.33519.jpg",
                    Gender = "Unisex",
                    Intensity = "Très forte",
                    Longevity = "+12 heures",
                    Sillage = "Énorme",
                    PriceRange = "$$$$",
                    Price = 3200m,
                    Stock = 18,
                    Families = new[] { "Amber", "Floral" },
                    Notes = new[] { ("Safran", "Top"), ("Jasmin", "Middle"), ("Ambre", "Base"), ("Cèdre", "Base"), ("Musc", "Base") },
                    Accords = new[] { ("Amber", "Strong"), ("Sweet", "Strong"), ("Woody", "Medium") },
                    Tags = new[] { "Luxueux", "Iconique", "Moderne", "Signature" },
                    Seasons = new[] { "Automne", "Hiver" },
                    Occasions = new[] { "Cérémonie", "Soirée", "Rendez-vous" }
                },
                new {
                    Name = "Tobacco Vanille",
                    Brand = "Tom Ford",
                    Description = "Tobacco Vanille capture l'essence des clubs privés anglais. Une fusion opulente de feuilles de tabac, vanille, cacao et fruits secs pour une chaleur enveloppante.",
                    ImageUrl = "https://fimgs.net/mdimg/perfume/375x500.1825.jpg",
                    Gender = "Unisex",
                    Intensity = "Très forte",
                    Longevity = "+12 heures",
                    Sillage = "Énorme",
                    PriceRange = "$$$$",
                    Price = 2900m,
                    Stock = 20,
                    Families = new[] { "Oriental", "Spicy" },
                    Notes = new[] { ("Tabac", "Top"), ("Épices", "Top"), ("Vanille", "Middle"), ("Cacao", "Middle"), ("Fruits Secs", "Base"), ("Bois", "Base") },
                    Accords = new[] { ("Tobacco", "Strong"), ("Vanilla", "Strong"), ("Warm Spicy", "Medium") },
                    Tags = new[] { "Opulent", "Chaleureux", "Addictif", "Signature" },
                    Seasons = new[] { "Automne", "Hiver" },
                    Occasions = new[] { "Soirée", "Cérémonie", "Rendez-vous" }
                },
                new {
                    Name = "Santal 33",
                    Brand = "Le Labo",
                    Description = "Santal 33 est un santal addictif et mystérieux. Notes de carvi, iris et violet sur un cœur de santal et de cèdre, avec un fond cuiré et musqué.",
                    ImageUrl = "https://fimgs.net/mdimg/perfume/375x500.15949.jpg",
                    Gender = "Unisex",
                    Intensity = "Modérée",
                    Longevity = "8-12 heures",
                    Sillage = "Modéré",
                    PriceRange = "$$$$",
                    Price = 2600m,
                    Stock = 25,
                    Families = new[] { "Woody", "Aromatic" },
                    Notes = new[] { ("Carvi", "Top"), ("Iris", "Top"), ("Violet", "Top"), ("Santal", "Middle"), ("Papyrus", "Middle"), ("Cuir", "Base"), ("Ambre", "Base"), ("Cèdre", "Base") },
                    Accords = new[] { ("Sandalwood", "Strong"), ("Leather", "Medium"), ("Woody", "Strong") },
                    Tags = new[] { "Culte", "Moderne", "Minimaliste", "NYC" },
                    Seasons = new[] { "Printemps", "Automne" },
                    Occasions = new[] { "Quotidien", "Bureau", "Soirée" }
                },
                new {
                    Name = "1 Million",
                    Brand = "Paco Rabanne",
                    Description = "1 Million est un cuir épicé frais et audacieux. Mandarine et menthe fraîche s'ouvrent sur un cœur de rose et cannelle, avec un fond de cuir et ambre blanc.",
                    ImageUrl = "https://fimgs.net/mdimg/perfume/375x500.6697.jpg",
                    Gender = "Male",
                    Intensity = "Forte",
                    Longevity = "6-8 heures",
                    Sillage = "Fort",
                    PriceRange = "$$",
                    Price = 750m,
                    Stock = 65,
                    Families = new[] { "Spicy", "Leather" },
                    Notes = new[] { ("Mandarine", "Top"), ("Menthe", "Top"), ("Pamplemousse", "Top"), ("Rose", "Middle"), ("Cannelle", "Middle"), ("Cuir", "Base"), ("Ambre Blanc", "Base"), ("Bois", "Base") },
                    Accords = new[] { ("Spicy", "Strong"), ("Leather", "Medium"), ("Fresh", "Medium") },
                    Tags = new[] { "Audacieux", "Séducteur", "Festif", "Best-seller" },
                    Seasons = new[] { "Automne", "Hiver" },
                    Occasions = new[] { "Soirée", "Rendez-vous", "Cérémonie" }
                },
                new {
                    Name = "Invictus",
                    Brand = "Paco Rabanne",
                    Description = "Invictus est un aromatic aquatique frais et puissant. Notes marines et de pamplemousse avec un cœur de laurier et jasmin, sur un fond de bois de gaïac.",
                    ImageUrl = "https://fimgs.net/mdimg/perfume/375x500.23088.jpg",
                    Gender = "Male",
                    Intensity = "Modérée",
                    Longevity = "6-8 heures",
                    Sillage = "Modéré",
                    PriceRange = "$$",
                    Price = 680m,
                    Stock = 70,
                    Families = new[] { "Aquatic", "Fresh" },
                    Notes = new[] { ("Marine", "Top"), ("Pamplemousse", "Top"), ("Laurier", "Middle"), ("Jasmin", "Middle"), ("Bois de Gaïac", "Base"), ("Ambre Gris", "Base"), ("Mousse de Chêne", "Base") },
                    Accords = new[] { ("Fresh", "Strong"), ("Aquatic", "Strong"), ("Woody", "Medium") },
                    Tags = new[] { "Sportif", "Frais", "Dynamique", "Champion" },
                    Seasons = new[] { "Printemps", "Été" },
                    Occasions = new[] { "Quotidien", "Sport", "Bureau" }
                },
                new {
                    Name = "Light Blue",
                    Brand = "Dolce & Gabbana",
                    Description = "Light Blue capture l'essence de l'été méditerranéen. Pomme de Sicile et cèdre s'associent au jasmin et bambou, sur un fond d'ambre et musc blanc.",
                    ImageUrl = "https://fimgs.net/mdimg/perfume/375x500.485.jpg",
                    Gender = "Female",
                    Intensity = "Légère",
                    Longevity = "4-6 heures",
                    Sillage = "Intime",
                    PriceRange = "$$",
                    Price = 720m,
                    Stock = 80,
                    Families = new[] { "Citrus", "Floral" },
                    Notes = new[] { ("Pomme", "Top"), ("Citron", "Top"), ("Cèdre", "Top"), ("Jasmin", "Middle"), ("Bambou", "Middle"), ("Rose Blanche", "Middle"), ("Ambre", "Base"), ("Musc Blanc", "Base"), ("Cèdre", "Base") },
                    Accords = new[] { ("Citrus", "Strong"), ("Fresh", "Strong"), ("Floral", "Medium") },
                    Tags = new[] { "Estival", "Frais", "Méditerranéen", "Classique" },
                    Seasons = new[] { "Printemps", "Été" },
                    Occasions = new[] { "Quotidien", "Vacances", "Bureau" }
                },
                new {
                    Name = "Versace Pour Homme",
                    Brand = "Versace",
                    Description = "Versace Pour Homme est un aromatic méditerranéen. Citrus et néroli sur un cœur de cèdre, sauge et ambre, avec un fond de musc et bois précieux.",
                    ImageUrl = "https://fimgs.net/mdimg/perfume/375x500.4740.jpg",
                    Gender = "Male",
                    Intensity = "Légère",
                    Longevity = "4-6 heures",
                    Sillage = "Intime",
                    PriceRange = "$$",
                    Price = 650m,
                    Stock = 75,
                    Families = new[] { "Aromatic", "Fougère" },
                    Notes = new[] { ("Néroli", "Top"), ("Citron", "Top"), ("Bergamote", "Top"), ("Cèdre", "Middle"), ("Sauge", "Middle"), ("Ambre", "Middle"), ("Musc", "Base"), ("Bois Précieux", "Base") },
                    Accords = new[] { ("Fresh", "Strong"), ("Citrus", "Medium"), ("Aromatic", "Medium") },
                    Tags = new[] { "Italien", "Décontracté", "Élégant", "Classique" },
                    Seasons = new[] { "Printemps", "Été" },
                    Occasions = new[] { "Quotidien", "Bureau", "Vacances" }
                },
                new {
                    Name = "Chance Eau Tendre",
                    Brand = "Chanel",
                    Description = "Chance Eau Tendre est une interprétation délicate du hasard. Un tourbillon floral fruité de pamplemousse, jasmin et jacinthe sur un lit de cèdre blanc et iris.",
                    ImageUrl = "https://fimgs.net/mdimg/perfume/375x500.12842.jpg",
                    Gender = "Female",
                    Intensity = "Légère",
                    Longevity = "4-6 heures",
                    Sillage = "Modéré",
                    PriceRange = "$$$",
                    Price = 1200m,
                    Stock = 45,
                    Families = new[] { "Floral", "Fruity" },
                    Notes = new[] { ("Pamplemousse", "Top"), ("Coing", "Top"), ("Jasmin", "Middle"), ("Jacinthe", "Middle"), ("Rose", "Middle"), ("Cèdre Blanc", "Base"), ("Iris", "Base"), ("Ambre", "Base") },
                    Accords = new[] { ("Floral", "Medium"), ("Fresh", "Strong"), ("Citrus", "Medium") },
                    Tags = new[] { "Délicat", "Romantique", "Tendre", "Printemps" },
                    Seasons = new[] { "Printemps", "Été" },
                    Occasions = new[] { "Quotidien", "Bureau", "Rendez-vous" }
                },
                new {
                    Name = "Lost Cherry",
                    Brand = "Tom Ford",
                    Description = "Lost Cherry est un élixir gourmand de cerise noire et d'amande amère. Notes de griotte, liqueur de cerise et amande sur un fond de santal, vétiver et cèdre.",
                    ImageUrl = "https://fimgs.net/mdimg/perfume/375x500.52464.jpg",
                    Gender = "Unisex",
                    Intensity = "Forte",
                    Longevity = "8-12 heures",
                    Sillage = "Fort",
                    PriceRange = "$$$$",
                    Price = 3400m,
                    Stock = 16,
                    Families = new[] { "Gourmand", "Fruity" },
                    Notes = new[] { ("Cerise Noire", "Top"), ("Liqueur de Cerise", "Top"), ("Amande Amère", "Middle"), ("Cerise Griotte", "Middle"), ("Santal", "Base"), ("Vétiver", "Base"), ("Cèdre", "Base") },
                    Accords = new[] { ("Cherry", "Strong"), ("Almond", "Medium"), ("Gourmand", "Strong") },
                    Tags = new[] { "Provocant", "Gourmand", "Fruité", "Addictif" },
                    Seasons = new[] { "Automne", "Hiver" },
                    Occasions = new[] { "Soirée", "Rendez-vous" }
                },
                new {
                    Name = "Noir de Noir",
                    Brand = "Tom Ford",
                    Description = "Noir de Noir est un floral sombre et sensuel. Rose noire et safran sur un cœur de truffe et vanille, avec un fond de patchouli, oud et mousse de chêne.",
                    ImageUrl = "https://fimgs.net/mdimg/perfume/375x500.3055.jpg",
                    Gender = "Unisex",
                    Intensity = "Très forte",
                    Longevity = "+12 heures",
                    Sillage = "Fort",
                    PriceRange = "$$$$",
                    Price = 3100m,
                    Stock = 12,
                    Families = new[] { "Oriental", "Floral" },
                    Notes = new[] { ("Rose Noire", "Top"), ("Safran", "Top"), ("Truffe", "Middle"), ("Vanille", "Middle"), ("Patchouli", "Base"), ("Oud", "Base"), ("Mousse de Chêne", "Base") },
                    Accords = new[] { ("Rose", "Strong"), ("Oud", "Medium"), ("Earthy", "Strong") },
                    Tags = new[] { "Sombre", "Sensuel", "Décadent", "Luxueux" },
                    Seasons = new[] { "Automne", "Hiver" },
                    Occasions = new[] { "Soirée", "Rendez-vous", "Cérémonie" }
                },
            };

            foreach (var p in perfumeData)
            {
                var perfume = new Models.Perfume
                {
                    Id = Guid.NewGuid(),
                    Name = p.Name,
                    BrandId = brands[p.Brand].Id,
                    Description = p.Description,
                    ImageUrl = p.ImageUrl,
                    GenderProfile = p.Gender,
                    Intensity = p.Intensity,
                    Longevity = p.Longevity,
                    Sillage = p.Sillage,
                    PriceRange = p.PriceRange,
                    Price = p.Price,
                    StockQuantity = p.Stock,
                    CreatedAt = DateTime.UtcNow.AddDays(-Random.Shared.Next(1, 365))
                };
                context.Perfumes.Add(perfume);

                // Families
                foreach (var familyName in p.Families)
                {
                    if (families.TryGetValue(familyName, out var family))
                    {
                        context.PerfumeFamilies.Add(new Models.PerfumeFamily
                        {
                            PerfumeId = perfume.Id,
                            FamilyId = family.Id
                        });
                    }
                }

                // Notes
                foreach (var (noteName, noteLevel) in p.Notes)
                {
                    if (notes.TryGetValue(noteName, out var note))
                    {
                        context.PerfumeNotes.Add(new Models.PerfumeNote
                        {
                            PerfumeId = perfume.Id,
                            NoteId = note.Id,
                            NoteLevel = noteLevel
                        });
                    }
                }

                // Accords
                foreach (var (accordName, intensity) in p.Accords)
                {
                    if (accords.TryGetValue(accordName, out var accord))
                    {
                        context.PerfumeAccords.Add(new Models.PerfumeAccord
                        {
                            PerfumeId = perfume.Id,
                            AccordId = accord.Id,
                            Intensity = intensity
                        });
                    }
                }

                // Tags
                foreach (var tagName in p.Tags)
                {
                    if (tags.TryGetValue(tagName, out var tag))
                    {
                        context.PerfumeTags.Add(new Models.PerfumeTag
                        {
                            PerfumeId = perfume.Id,
                            TagId = tag.Id
                        });
                    }
                }

                // Seasons
                foreach (var seasonName in p.Seasons)
                {
                    if (seasons.TryGetValue(seasonName, out var season))
                    {
                        context.PerfumeSeasons.Add(new Models.PerfumeSeason
                        {
                            PerfumeId = perfume.Id,
                            SeasonId = season.Id
                        });
                    }
                }

                // Occasions
                foreach (var occasionName in p.Occasions)
                {
                    if (occasions.TryGetValue(occasionName, out var occasion))
                    {
                        context.PerfumeOccasions.Add(new Models.PerfumeOccasion
                        {
                            PerfumeId = perfume.Id,
                            OccasionId = occasion.Id
                        });
                    }
                }
            }

            await context.SaveChangesAsync();
            Console.WriteLine($"Seeded {perfumeData.Length} perfumes with all relationships.");
            Console.WriteLine("Perfume database seeding complete!");
        }
    }
}
