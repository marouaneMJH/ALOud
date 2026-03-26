using Bogus;
using ALOud.Models;

namespace Tests.Common.TestDataBuilders
{
    /// <summary>
    /// Test data builder for Brand entities using the Builder pattern.
    /// Uses Bogus library to generate realistic fake data.
    /// </summary>
    public class BrandBuilder
    {
        private readonly Faker<Brand> _faker;

        public BrandBuilder()
        {
            _faker = new Faker<Brand>()
                .RuleFor(b => b.Id, f => f.Random.Guid())
                .RuleFor(b => b.Name, f => f.Company.CompanyName())
                .RuleFor(b => b.Perfumes, f => new List<Perfume>());
        }

        /// <summary>
        /// Sets the brand ID.
        /// </summary>
        public BrandBuilder WithId(Guid id)
        {
            _faker.RuleFor(b => b.Id, id);
            return this;
        }

        /// <summary>
        /// Sets the brand name.
        /// </summary>
        public BrandBuilder WithName(string name)
        {
            _faker.RuleFor(b => b.Name, name);
            return this;
        }

        /// <summary>
        /// Adds perfumes to the brand.
        /// </summary>
        public BrandBuilder WithPerfumes(params Perfume[] perfumes)
        {
            _faker.RuleFor(b => b.Perfumes, perfumes.ToList());
            return this;
        }

        /// <summary>
        /// Creates a luxury brand.
        /// </summary>
        public BrandBuilder AsLuxuryBrand()
        {
            var luxuryBrands = new[] { "Chanel", "Dior", "Creed", "Tom Ford", "Maison Francis Kurkdjian", "Amouage" };
            var faker = new Faker();
            return WithName(faker.PickRandom(luxuryBrands));
        }

        /// <summary>
        /// Creates a designer brand.
        /// </summary>
        public BrandBuilder AsDesignerBrand()
        {
            var designerBrands = new[] { "Calvin Klein", "Hugo Boss", "Versace", "Dolce & Gabbana", "Prada", "Armani" };
            var faker = new Faker();
            return WithName(faker.PickRandom(designerBrands));
        }

        /// <summary>
        /// Creates a niche brand.
        /// </summary>
        public BrandBuilder AsNicheBrand()
        {
            var nicheBrands = new[] { "Le Labo", "Byredo", "Diptyque", "L'Artisan Parfumeur", "Penhaligon's", "Serge Lutens" };
            var faker = new Faker();
            return WithName(faker.PickRandom(nicheBrands));
        }

        /// <summary>
        /// Builds the Brand instance with the configured values.
        /// </summary>
        public Brand Build()
        {
            var brand = _faker.Generate();
            
            // Ensure navigation properties are properly linked
            foreach (var perfume in brand.Perfumes)
            {
                perfume.BrandId = brand.Id;
                perfume.Brand = brand;
            }

            return brand;
        }

        /// <summary>
        /// Builds multiple Brand instances with the configured values.
        /// </summary>
        public List<Brand> Build(int count)
        {
            return Enumerable.Range(0, count).Select(_ => Build()).ToList();
        }
    }
}