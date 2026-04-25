using Bogus;
using ALOud.Models;

namespace Tests.Common.TestDataBuilders
{
    public class FamilyBuilder
    {
        private readonly Faker<Family> _faker;

        public FamilyBuilder()
        {
            _faker = new Faker<Family>()
                .RuleFor(f => f.Id, f => Guid.NewGuid())
                .RuleFor(f => f.Name, f => f.Commerce.ProductName())
                .RuleFor(f => f.Description, f => f.Lorem.Sentence(10))
                .RuleFor(f => f.PerfumeFamilies, new List<PerfumeFamily>());
        }

        public FamilyBuilder WithId(Guid id)
        {
            _faker.RuleFor(f => f.Id, id);
            return this;
        }

        public FamilyBuilder WithName(string name)
        {
            _faker.RuleFor(f => f.Name, name);
            return this;
        }

        public FamilyBuilder WithDescription(string? description)
        {
            _faker.RuleFor(f => f.Description, description);
            return this;
        }

        public FamilyBuilder WithoutDescription()
        {
            _faker.RuleFor(f => f.Description, (string?)null);
            return this;
        }

        public FamilyBuilder WithPerfumeFamilies(List<PerfumeFamily> perfumeFamilies)
        {
            _faker.RuleFor(f => f.PerfumeFamilies, perfumeFamilies);
            return this;
        }

        public FamilyBuilder WithLongName()
        {
            _faker.RuleFor(f => f.Name, f => f.Random.String2(150)); // Max length
            return this;
        }

        public FamilyBuilder WithTooLongName()
        {
            _faker.RuleFor(f => f.Name, f => f.Random.String2(151)); // Exceeds max length
            return this;
        }

        public FamilyBuilder WithLongDescription()
        {
            _faker.RuleFor(f => f.Description, f => f.Random.String2(500)); // Max length
            return this;
        }

        public FamilyBuilder WithTooLongDescription()
        {
            _faker.RuleFor(f => f.Description, f => f.Random.String2(501)); // Exceeds max length
            return this;
        }

        public Family Build() => _faker.Generate();

        public List<Family> Build(int count) => _faker.Generate(count);

        // Static convenience methods
        public static Family CreateValid() => new FamilyBuilder().Build();

        public static Family CreateWithName(string name) => new FamilyBuilder().WithName(name).Build();

        public static List<Family> CreateValidList(int count) => new FamilyBuilder().Build(count);
    }
}