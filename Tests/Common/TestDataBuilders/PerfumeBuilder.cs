using Bogus;
using ALOud.Models;

namespace Tests.Common.TestDataBuilders
{
    /// <summary>
    /// Test data builder for Perfume entities using the Builder pattern.
    /// Uses Bogus library to generate realistic fake data.
    /// </summary>
    public class PerfumeBuilder
    {
        private readonly Faker<Perfume> _faker;

        public PerfumeBuilder()
        {
            _faker = new Faker<Perfume>()
                .RuleFor(p => p.Id, f => f.Random.Guid())
                .RuleFor(p => p.Name, f => f.Commerce.ProductName())
                .RuleFor(p => p.Intensity, f => f.PickRandom("Light", "Moderate", "Strong", "Very Strong"))
                .RuleFor(p => p.Longevity, f => f.PickRandom("Short", "Moderate", "Long-lasting", "Very Long-lasting"))
                .RuleFor(p => p.Sillage, f => f.PickRandom("Intimate", "Moderate", "Strong", "Enormous"))
                .RuleFor(p => p.GenderProfile, f => f.PickRandom("Men", "Women", "Unisex"))
                .RuleFor(p => p.PriceRange, f => f.PickRandom("Budget", "Mid-range", "Luxury", "Niche"))
                .RuleFor(p => p.Price, f => f.Random.Decimal(20, 500))
                .RuleFor(p => p.StockQuantity, f => f.Random.Int(0, 100))
                .RuleFor(p => p.Description, f => f.Lorem.Paragraph())
                .RuleFor(p => p.ImageUrl, f => f.Internet.Url())
                .RuleFor(p => p.CreatedAt, f => f.Date.Past(2))
                .RuleFor(p => p.BrandId, f => f.Random.Guid())
                .RuleFor(p => p.PerfumeFamilies, f => new List<PerfumeFamily>())
                .RuleFor(p => p.PerfumeNotes, f => new List<PerfumeNote>())
                .RuleFor(p => p.PerfumeAccords, f => new List<PerfumeAccord>())
                .RuleFor(p => p.PerfumeTags, f => new List<PerfumeTag>())
                .RuleFor(p => p.PerfumeSeasons, f => new List<PerfumeSeason>())
                .RuleFor(p => p.PerfumeOccasions, f => new List<PerfumeOccasion>());
        }

        /// <summary>
        /// Sets the perfume ID.
        /// </summary>
        public PerfumeBuilder WithId(Guid id)
        {
            _faker.RuleFor(p => p.Id, id);
            return this;
        }

        /// <summary>
        /// Sets the perfume name.
        /// </summary>
        public PerfumeBuilder WithName(string name)
        {
            _faker.RuleFor(p => p.Name, name);
            return this;
        }

        /// <summary>
        /// Sets the brand ID.
        /// </summary>
        public PerfumeBuilder WithBrandId(Guid brandId)
        {
            _faker.RuleFor(p => p.BrandId, brandId);
            return this;
        }

        /// <summary>
        /// Sets the brand navigation property.
        /// </summary>
        public PerfumeBuilder WithBrand(Brand brand)
        {
            _faker.RuleFor(p => p.BrandId, brand.Id);
            _faker.RuleFor(p => p.Brand, brand);
            return this;
        }

        /// <summary>
        /// Sets the intensity level.
        /// </summary>
        public PerfumeBuilder WithIntensity(string intensity)
        {
            _faker.RuleFor(p => p.Intensity, intensity);
            return this;
        }

        /// <summary>
        /// Sets the longevity.
        /// </summary>
        public PerfumeBuilder WithLongevity(string longevity)
        {
            _faker.RuleFor(p => p.Longevity, longevity);
            return this;
        }

        /// <summary>
        /// Sets the sillage.
        /// </summary>
        public PerfumeBuilder WithSillage(string sillage)
        {
            _faker.RuleFor(p => p.Sillage, sillage);
            return this;
        }

        /// <summary>
        /// Sets the gender profile.
        /// </summary>
        public PerfumeBuilder WithGenderProfile(string genderProfile)
        {
            _faker.RuleFor(p => p.GenderProfile, genderProfile);
            return this;
        }

        /// <summary>
        /// Sets the price range.
        /// </summary>
        public PerfumeBuilder WithPriceRange(string priceRange)
        {
            _faker.RuleFor(p => p.PriceRange, priceRange);
            return this;
        }

        /// <summary>
        /// Sets the price.
        /// </summary>
        public PerfumeBuilder WithPrice(decimal price)
        {
            _faker.RuleFor(p => p.Price, price);
            return this;
        }

        /// <summary>
        /// Sets the stock quantity.
        /// </summary>
        public PerfumeBuilder WithStockQuantity(int quantity)
        {
            _faker.RuleFor(p => p.StockQuantity, quantity);
            return this;
        }

        /// <summary>
        /// Sets the description.
        /// </summary>
        public PerfumeBuilder WithDescription(string description)
        {
            _faker.RuleFor(p => p.Description, description);
            return this;
        }

        /// <summary>
        /// Sets the image URL.
        /// </summary>
        public PerfumeBuilder WithImageUrl(string imageUrl)
        {
            _faker.RuleFor(p => p.ImageUrl, imageUrl);
            return this;
        }

        /// <summary>
        /// Sets the creation date.
        /// </summary>
        public PerfumeBuilder WithCreatedAt(DateTime createdAt)
        {
            _faker.RuleFor(p => p.CreatedAt, createdAt);
            return this;
        }

        /// <summary>
        /// Adds perfume families to the perfume.
        /// </summary>
        public PerfumeBuilder WithFamilies(params Family[] families)
        {
            var perfumeFamilies = families.Select(f => new PerfumeFamily
            {
                FamilyId = f.Id,
                Family = f
            }).ToList();

            _faker.RuleFor(p => p.PerfumeFamilies, perfumeFamilies);
            return this;
        }

        /// <summary>
        /// Adds perfume notes to the perfume.
        /// </summary>
        public PerfumeBuilder WithNotes(params (Note note, string level)[] notes)
        {
            var perfumeNotes = notes.Select(n => new PerfumeNote
            {
                NoteId = n.note.Id,
                Note = n.note,
                NoteLevel = n.level
            }).ToList();

            _faker.RuleFor(p => p.PerfumeNotes, perfumeNotes);
            return this;
        }

        /// <summary>
        /// Adds perfume accords to the perfume.
        /// </summary>
        public PerfumeBuilder WithAccords(params (Accord accord, string intensity)[] accords)
        {
            var perfumeAccords = accords.Select(a => new PerfumeAccord
            {
                AccordId = a.accord.Id,
                Accord = a.accord,
                Intensity = a.intensity
            }).ToList();

            _faker.RuleFor(p => p.PerfumeAccords, perfumeAccords);
            return this;
        }

        /// <summary>
        /// Creates a perfume configured for women.
        /// </summary>
        public PerfumeBuilder AsWomensPerfume()
        {
            return WithGenderProfile("Women")
                   .WithIntensity("Moderate")
                   .WithSillage("Moderate");
        }

        /// <summary>
        /// Creates a perfume configured for men.
        /// </summary>
        public PerfumeBuilder AsMensPerfume()
        {
            return WithGenderProfile("Men")
                   .WithIntensity("Strong")
                   .WithSillage("Strong");
        }

        /// <summary>
        /// Creates a perfume configured as unisex.
        /// </summary>
        public PerfumeBuilder AsUnisexPerfume()
        {
            return WithGenderProfile("Unisex")
                   .WithIntensity("Moderate")
                   .WithSillage("Moderate");
        }

        /// <summary>
        /// Creates a luxury perfume with high price.
        /// </summary>
        public PerfumeBuilder AsLuxuryPerfume()
        {
            return WithPriceRange("Luxury")
                   .WithPrice(200);
        }

        /// <summary>
        /// Creates a budget perfume with low price.
        /// </summary>
        public PerfumeBuilder AsBudgetPerfume()
        {
            return WithPriceRange("Budget")
                   .WithPrice(25);
        }

        /// <summary>
        /// Creates a perfume that's out of stock.
        /// </summary>
        public PerfumeBuilder AsOutOfStock()
        {
            return WithStockQuantity(0);
        }

        /// <summary>
        /// Builds the Perfume instance with the configured values.
        /// </summary>
        public Perfume Build()
        {
            var perfume = _faker.Generate();
            
            // Ensure navigation properties are properly linked
            foreach (var family in perfume.PerfumeFamilies)
            {
                family.PerfumeId = perfume.Id;
                family.Perfume = perfume;
            }
            
            foreach (var note in perfume.PerfumeNotes)
            {
                note.PerfumeId = perfume.Id;
                note.Perfume = perfume;
            }
            
            foreach (var accord in perfume.PerfumeAccords)
            {
                accord.PerfumeId = perfume.Id;
                accord.Perfume = perfume;
            }

            return perfume;
        }

        /// <summary>
        /// Builds multiple Perfume instances with the configured values.
        /// </summary>
        public List<Perfume> Build(int count)
        {
            return Enumerable.Range(0, count).Select(_ => Build()).ToList();
        }
    }
}