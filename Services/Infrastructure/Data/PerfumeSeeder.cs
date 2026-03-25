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

            var brands = SeedBrands(context);
            var families = SeedFamilies(context);
            var notes = SeedNotes(context);
            var accords = SeedAccords(context);
            var seasons = SeedSeasons(context);
            var occasions = SeedOccasions(context);

            SeedPerfumesAndRelationships(context, brands, families, notes, accords, seasons, occasions);
            
            await context.SaveChangesAsync();
            Console.WriteLine("Perfume database seeding complete!");
        }

        private static Dictionary<string, Models.Brand> SeedBrands(ALOudDbContext context)
        {
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
            return brands;
        }

        private static Dictionary<string, Models.Family> SeedFamilies(ALOudDbContext context)
        {
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
                ["Fresh"] = new Models.Family { Id = Guid.NewGuid(), Name = "Fresh" },
                ["Citrus"] = new Models.Family { Id = Guid.NewGuid(), Name = "Citrus" },
            };
            context.Families.AddRange(families.Values);
            return families;
        }

        private static Dictionary<string, Models.Note> SeedNotes(ALOudDbContext context)
        {
            var notes = new Dictionary<string, Models.Note>
            {
                // Top Notes
                ["Bergamot"] = new Models.Note { Id = Guid.NewGuid(), Name = "Bergamot", Category = "Top" },
                ["Lemon"] = new Models.Note { Id = Guid.NewGuid(), Name = "Lemon", Category = "Top" },
                ["Orange"] = new Models.Note { Id = Guid.NewGuid(), Name = "Orange", Category = "Top" },
                ["Grapefruit"] = new Models.Note { Id = Guid.NewGuid(), Name = "Grapefruit", Category = "Top" },
                ["Lavender"] = new Models.Note { Id = Guid.NewGuid(), Name = "Lavender", Category = "Top" },
                ["Rosemary"] = new Models.Note { Id = Guid.NewGuid(), Name = "Rosemary", Category = "Top" },
                ["Mint"] = new Models.Note { Id = Guid.NewGuid(), Name = "Mint", Category = "Top" },
                ["Cardamom"] = new Models.Note { Id = Guid.NewGuid(), Name = "Cardamom", Category = "Top" },
                ["Pink Pepper"] = new Models.Note { Id = Guid.NewGuid(), Name = "Pink Pepper", Category = "Top" },
                
                // Middle Notes  
                ["Rose"] = new Models.Note { Id = Guid.NewGuid(), Name = "Rose", Category = "Middle" },
                ["Jasmine"] = new Models.Note { Id = Guid.NewGuid(), Name = "Jasmine", Category = "Middle" },
                ["Iris"] = new Models.Note { Id = Guid.NewGuid(), Name = "Iris", Category = "Middle" },
                ["Violet"] = new Models.Note { Id = Guid.NewGuid(), Name = "Violet", Category = "Middle" },
                ["Geranium"] = new Models.Note { Id = Guid.NewGuid(), Name = "Geranium", Category = "Middle" },
                ["Cedar"] = new Models.Note { Id = Guid.NewGuid(), Name = "Cedar", Category = "Middle" },
                ["Pine"] = new Models.Note { Id = Guid.NewGuid(), Name = "Pine", Category = "Middle" },
                ["Cloves"] = new Models.Note { Id = Guid.NewGuid(), Name = "Cloves", Category = "Middle" },
                
                // Base Notes
                ["Sandalwood"] = new Models.Note { Id = Guid.NewGuid(), Name = "Sandalwood", Category = "Base" },
                ["Cedar Wood"] = new Models.Note { Id = Guid.NewGuid(), Name = "Cedar Wood", Category = "Base" },
                ["Vetiver"] = new Models.Note { Id = Guid.NewGuid(), Name = "Vetiver", Category = "Base" },
                ["Patchouli"] = new Models.Note { Id = Guid.NewGuid(), Name = "Patchouli", Category = "Base" },
                ["Musk"] = new Models.Note { Id = Guid.NewGuid(), Name = "Musk", Category = "Base" },
                ["Amber"] = new Models.Note { Id = Guid.NewGuid(), Name = "Amber", Category = "Base" },
                ["Vanilla"] = new Models.Note { Id = Guid.NewGuid(), Name = "Vanilla", Category = "Base" },
                ["Tonka Bean"] = new Models.Note { Id = Guid.NewGuid(), Name = "Tonka Bean", Category = "Base" },
            };
            context.Notes.AddRange(notes.Values);
            return notes;
        }

        private static Dictionary<string, Models.Accord> SeedAccords(ALOudDbContext context)
        {
            var accords = new Dictionary<string, Models.Accord>
            {
                ["Fresh"] = new Models.Accord { Id = Guid.NewGuid(), Name = "Fresh" },
                ["Woody"] = new Models.Accord { Id = Guid.NewGuid(), Name = "Woody" },
                ["Aromatic"] = new Models.Accord { Id = Guid.NewGuid(), Name = "Aromatic" },
                ["Spicy"] = new Models.Accord { Id = Guid.NewGuid(), Name = "Spicy" },
                ["Floral"] = new Models.Accord { Id = Guid.NewGuid(), Name = "Floral" },
                ["Sweet"] = new Models.Accord { Id = Guid.NewGuid(), Name = "Sweet" },
                ["Citrus"] = new Models.Accord { Id = Guid.NewGuid(), Name = "Citrus" },
                ["Musky"] = new Models.Accord { Id = Guid.NewGuid(), Name = "Musky" },
            };
            context.Accords.AddRange(accords.Values);
            return accords;
        }

        private static Dictionary<string, Models.Season> SeedSeasons(ALOudDbContext context)
        {
            var seasons = new Dictionary<string, Models.Season>
            {
                ["Spring"] = new Models.Season { Id = Guid.NewGuid(), Name = "Spring" },
                ["Summer"] = new Models.Season { Id = Guid.NewGuid(), Name = "Summer" },
                ["Fall"] = new Models.Season { Id = Guid.NewGuid(), Name = "Fall" },
                ["Winter"] = new Models.Season { Id = Guid.NewGuid(), Name = "Winter" },
                ["All Year"] = new Models.Season { Id = Guid.NewGuid(), Name = "All Year" },
            };
            context.Seasons.AddRange(seasons.Values);
            return seasons;
        }

        private static Dictionary<string, Models.Occasion> SeedOccasions(ALOudDbContext context)
        {
            var occasions = new Dictionary<string, Models.Occasion>
            {
                ["Daily"] = new Models.Occasion { Id = Guid.NewGuid(), Name = "Daily" },
                ["Office"] = new Models.Occasion { Id = Guid.NewGuid(), Name = "Office" },
                ["Date Night"] = new Models.Occasion { Id = Guid.NewGuid(), Name = "Date Night" },
                ["Formal Events"] = new Models.Occasion { Id = Guid.NewGuid(), Name = "Formal Events" },
                ["Nightlife"] = new Models.Occasion { Id = Guid.NewGuid(), Name = "Nightlife" },
                ["Sport"] = new Models.Occasion { Id = Guid.NewGuid(), Name = "Sport" },
                ["Casual"] = new Models.Occasion { Id = Guid.NewGuid(), Name = "Casual" },
            };
            context.Occasions.AddRange(occasions.Values);
            return occasions;
        }

        private static void SeedPerfumesAndRelationships(
            ALOudDbContext context,
            Dictionary<string, Models.Brand> brands,
            Dictionary<string, Models.Family> families,
            Dictionary<string, Models.Note> notes,
            Dictionary<string, Models.Accord> accords,
            Dictionary<string, Models.Season> seasons,
            Dictionary<string, Models.Occasion> occasions)
        {
            // Sample perfume data - simplified for demonstration
            var perfumeData = new[]
            {
                new {
                    Name = "Sauvage",
                    Brand = "Dior", 
                    Price = 850m,
                    Stock = 25,
                    Description = "A radically fresh composition",
                    Families = new[] { "Aromatic", "Fougère" },
                    Notes = new[] { "Bergamot", "Pink Pepper", "Geranium", "Cedar" },
                    Accords = new[] { "Fresh", "Aromatic", "Woody" },
                    Seasons = new[] { "Spring", "Summer", "All Year" },
                    Occasions = new[] { "Daily", "Casual", "Office" }
                },
                new {
                    Name = "Bleu de Chanel",
                    Brand = "Chanel",
                    Price = 950m,
                    Stock = 20,
                    Description = "An unexpected accord between the invigorating freshness of citrus",
                    Families = new[] { "Woody", "Aromatic" },
                    Notes = new[] { "Lemon", "Mint", "Pink Pepper", "Cedar Wood" },
                    Accords = new[] { "Fresh", "Woody", "Citrus" },
                    Seasons = new[] { "All Year" },
                    Occasions = new[] { "Daily", "Office", "Formal Events" }
                }
            };

            foreach (var data in perfumeData)
            {
                var perfume = new Models.Perfume
                {
                    Id = Guid.NewGuid(),
                    Name = data.Name,
                    BrandId = brands[data.Brand].Id,
                    Price = data.Price,
                    StockQuantity = data.Stock,
                    Description = data.Description,
                    CreatedAt = DateTime.UtcNow
                };

                context.Perfumes.Add(perfume);

                // Add relationships
                AddPerfumeRelationships(context, perfume, data, families, notes, accords, seasons, occasions);
            }
        }

        private static void AddPerfumeRelationships(
            ALOudDbContext context,
            Models.Perfume perfume,
            dynamic data,
            Dictionary<string, Models.Family> families,
            Dictionary<string, Models.Note> notes,
            Dictionary<string, Models.Accord> accords,
            Dictionary<string, Models.Season> seasons,
            Dictionary<string, Models.Occasion> occasions)
        {
            // Add families
            foreach (string familyName in data.Families)
            {
                if (families.ContainsKey(familyName))
                {
                    context.PerfumeFamilies.Add(new Models.PerfumeFamily
                    {
                        PerfumeId = perfume.Id,
                        FamilyId = families[familyName].Id
                    });
                }
            }

            // Add notes
            foreach (string noteName in data.Notes)
            {
                if (notes.ContainsKey(noteName))
                {
                    context.PerfumeNotes.Add(new Models.PerfumeNote
                    {
                        PerfumeId = perfume.Id,
                        NoteId = notes[noteName].Id
                    });
                }
            }

            // Add accords
            foreach (string accordName in data.Accords)
            {
                if (accords.ContainsKey(accordName))
                {
                    context.PerfumeAccords.Add(new Models.PerfumeAccord
                    {
                        PerfumeId = perfume.Id,
                        AccordId = accords[accordName].Id
                    });
                }
            }

            // Add seasons
            foreach (string seasonName in data.Seasons)
            {
                if (seasons.ContainsKey(seasonName))
                {
                    context.PerfumeSeasons.Add(new Models.PerfumeSeason
                    {
                        PerfumeId = perfume.Id,
                        SeasonId = seasons[seasonName].Id
                    });
                }
            }

            // Add occasions
            foreach (string occasionName in data.Occasions)
            {
                if (occasions.ContainsKey(occasionName))
                {
                    context.PerfumeOccasions.Add(new Models.PerfumeOccasion
                    {
                        PerfumeId = perfume.Id,
                        OccasionId = occasions[occasionName].Id
                    });
                }
            }
        }
    }
}