using Bogus;
using ALOud.Models;

namespace Tests.Common.TestDataBuilders
{
    public class TagBuilder
    {
        private readonly Faker<Tag> _faker;

        public TagBuilder()
        {
            _faker = new Faker<Tag>()
                .RuleFor(t => t.Id, f => Guid.NewGuid())
                .RuleFor(t => t.Name, f => f.PickRandom("Luxury", "Vintage", "Modern", "Oriental", "Fresh", "Intense", "Light", "Bold"))
                .RuleFor(t => t.PerfumeTags, new List<PerfumeTag>());
        }

        public TagBuilder WithId(Guid id)
        {
            _faker.RuleFor(t => t.Id, id);
            return this;
        }

        public TagBuilder WithName(string name)
        {
            _faker.RuleFor(t => t.Name, name);
            return this;
        }

        public TagBuilder WithPerfumeTags(List<PerfumeTag> perfumeTags)
        {
            _faker.RuleFor(t => t.PerfumeTags, perfumeTags);
            return this;
        }

        public TagBuilder WithLongName()
        {
            _faker.RuleFor(t => t.Name, f => f.Random.String2(100)); // Max length
            return this;
        }

        public TagBuilder WithTooLongName()
        {
            _faker.RuleFor(t => t.Name, f => f.Random.String2(101)); // Exceeds max length
            return this;
        }

        public Tag Build() => _faker.Generate();

        public List<Tag> Build(int count) => _faker.Generate(count);

        // Static convenience methods
        public static Tag CreateValid() => new TagBuilder().Build();

        public static Tag CreateWithName(string name) => new TagBuilder().WithName(name).Build();

        public static List<Tag> CreateValidList(int count) => new TagBuilder().Build(count);

        public static List<Tag> CreateWithNames(params string[] names) => 
            names.Select(name => new TagBuilder().WithName(name).Build()).ToList();
    }
}