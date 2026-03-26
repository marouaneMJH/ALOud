using Bogus;
using ALOud.Models;

namespace Tests.Common.TestDataBuilders
{
    public class SeasonBuilder
    {
        private readonly Faker<Season> _faker;

        public SeasonBuilder()
        {
            _faker = new Faker<Season>()
                .RuleFor(s => s.Id, f => Guid.NewGuid())
                .RuleFor(s => s.Name, f => f.PickRandom("Spring", "Summer", "Autumn", "Winter", "All Season"))
                .RuleFor(s => s.PerfumeSeasons, new List<PerfumeSeason>());
        }

        public SeasonBuilder WithId(Guid id)
        {
            _faker.RuleFor(s => s.Id, id);
            return this;
        }

        public SeasonBuilder WithName(string name)
        {
            _faker.RuleFor(s => s.Name, name);
            return this;
        }

        public SeasonBuilder WithPerfumeSeasons(List<PerfumeSeason> perfumeSeasons)
        {
            _faker.RuleFor(s => s.PerfumeSeasons, perfumeSeasons);
            return this;
        }

        public SeasonBuilder WithLongName()
        {
            _faker.RuleFor(s => s.Name, f => f.Random.String2(50)); // Max length
            return this;
        }

        public SeasonBuilder WithTooLongName()
        {
            _faker.RuleFor(s => s.Name, f => f.Random.String2(51)); // Exceeds max length
            return this;
        }

        public Season Build() => _faker.Generate();

        public List<Season> Build(int count) => _faker.Generate(count);

        // Static convenience methods
        public static Season CreateValid() => new SeasonBuilder().Build();

        public static Season CreateWithName(string name) => new SeasonBuilder().WithName(name).Build();

        public static List<Season> CreateValidList(int count) => new SeasonBuilder().Build(count);

        public static List<Season> CreateWithNames(params string[] names) => 
            names.Select(name => new SeasonBuilder().WithName(name).Build()).ToList();
    }
}