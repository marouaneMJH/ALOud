using Bogus;
using ALOud.Models;

namespace Tests.Common.TestDataBuilders
{
    public class OccasionBuilder
    {
        private readonly Faker<Occasion> _faker;

        public OccasionBuilder()
        {
            _faker = new Faker<Occasion>()
                .RuleFor(o => o.Id, f => Guid.NewGuid())
                .RuleFor(o => o.Name, f => f.PickRandom("Office", "Evening", "Date Night", "Casual", "Formal", "Party", "Wedding", "Travel"))
                .RuleFor(o => o.PerfumeOccasions, new List<PerfumeOccasion>());
        }

        public OccasionBuilder WithId(Guid id)
        {
            _faker.RuleFor(o => o.Id, id);
            return this;
        }

        public OccasionBuilder WithName(string name)
        {
            _faker.RuleFor(o => o.Name, name);
            return this;
        }

        public OccasionBuilder WithPerfumeOccasions(List<PerfumeOccasion> perfumeOccasions)
        {
            _faker.RuleFor(o => o.PerfumeOccasions, perfumeOccasions);
            return this;
        }

        public OccasionBuilder WithLongName()
        {
            _faker.RuleFor(o => o.Name, f => f.Random.String2(100)); // Max length
            return this;
        }

        public OccasionBuilder WithTooLongName()
        {
            _faker.RuleFor(o => o.Name, f => f.Random.String2(101)); // Exceeds max length
            return this;
        }

        public Occasion Build() => _faker.Generate();

        public List<Occasion> Build(int count) => _faker.Generate(count);

        // Static convenience methods
        public static Occasion CreateValid() => new OccasionBuilder().Build();

        public static Occasion CreateWithName(string name) => new OccasionBuilder().WithName(name).Build();

        public static List<Occasion> CreateValidList(int count) => new OccasionBuilder().Build(count);

        public static List<Occasion> CreateWithNames(params string[] names) => 
            names.Select(name => new OccasionBuilder().WithName(name).Build()).ToList();
    }
}