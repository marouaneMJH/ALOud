using Bogus;
using ALOud.Models;

namespace Tests.Common.TestDataBuilders
{
    /// <summary>
    /// Test data builder for Accord entities using the Builder pattern.
    /// Uses Bogus library to generate realistic fake data.
    /// </summary>
    public class AccordBuilder
    {
        private readonly Faker<Accord> _faker;

        public AccordBuilder()
        {
            _faker = new Faker<Accord>()
                .RuleFor(a => a.Id, f => f.Random.Guid())
                .RuleFor(a => a.Name, f => f.PickRandom(GetAccordNames()))
                .RuleFor(a => a.Description, f => f.Lorem.Sentence())
                .RuleFor(a => a.PerfumeAccords, f => new List<PerfumeAccord>());
        }

        /// <summary>
        /// Sets the accord ID.
        /// </summary>
        public AccordBuilder WithId(Guid id)
        {
            _faker.RuleFor(a => a.Id, id);
            return this;
        }

        /// <summary>
        /// Sets the accord name.
        /// </summary>
        public AccordBuilder WithName(string name)
        {
            _faker.RuleFor(a => a.Name, name);
            return this;
        }

        /// <summary>
        /// Sets the accord description.
        /// </summary>
        public AccordBuilder WithDescription(string description)
        {
            _faker.RuleFor(a => a.Description, description);
            return this;
        }

        /// <summary>
        /// Creates a fresh accord.
        /// </summary>
        public AccordBuilder AsFreshAccord()
        {
            var freshAccords = new[] { "Fresh", "Citrus", "Aquatic", "Green", "Marine" };
            var faker = new Faker();
            return WithName(faker.PickRandom(freshAccords))
                   .WithDescription("A clean, refreshing accord that evokes cleanliness and nature");
        }

        /// <summary>
        /// Creates a woody accord.
        /// </summary>
        public AccordBuilder AsWoodyAccord()
        {
            var woodyAccords = new[] { "Woody", "Sandalwood", "Cedar", "Vetiver", "Dry Wood" };
            var faker = new Faker();
            return WithName(faker.PickRandom(woodyAccords))
                   .WithDescription("A warm, earthy accord featuring wood and tree essences");
        }

        /// <summary>
        /// Creates a floral accord.
        /// </summary>
        public AccordBuilder AsFloralAccord()
        {
            var floralAccords = new[] { "Floral", "Rose", "Jasmine", "Lily", "White Floral" };
            var faker = new Faker();
            return WithName(faker.PickRandom(floralAccords))
                   .WithDescription("A delicate, feminine accord featuring flower essences");
        }

        /// <summary>
        /// Creates an oriental accord.
        /// </summary>
        public AccordBuilder AsOrientalAccord()
        {
            var orientalAccords = new[] { "Oriental", "Spicy", "Amber", "Resinous", "Balsamic" };
            var faker = new Faker();
            return WithName(faker.PickRandom(orientalAccords))
                   .WithDescription("A rich, exotic accord with warm and spicy characteristics");
        }

        /// <summary>
        /// Builds the Accord instance with the configured values.
        /// </summary>
        public Accord Build()
        {
            var accord = _faker.Generate();
            
            // Ensure navigation properties are properly linked
            foreach (var perfumeAccord in accord.PerfumeAccords)
            {
                perfumeAccord.AccordId = accord.Id;
                perfumeAccord.Accord = accord;
            }

            return accord;
        }

        /// <summary>
        /// Builds multiple Accord instances with the configured values.
        /// </summary>
        public List<Accord> Build(int count)
        {
            return Enumerable.Range(0, count).Select(_ => Build()).ToList();
        }

        /// <summary>
        /// Gets a list of realistic accord names for perfumery.
        /// </summary>
        private static string[] GetAccordNames()
        {
            return new[]
            {
                "Fresh", "Citrus", "Green", "Aquatic", "Marine",
                "Floral", "Rose", "Jasmine", "White Floral", "Powdery",
                "Woody", "Sandalwood", "Cedar", "Vetiver", "Dry Wood",
                "Oriental", "Spicy", "Amber", "Vanilla", "Resinous",
                "Fruity", "Sweet", "Gourmand", "Animalic", "Leather",
                "Smoky", "Incense", "Metallic", "Ozonic", "Aldehyde"
            };
        }
    }
}