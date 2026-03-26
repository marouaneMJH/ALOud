using Bogus;
using ALOud.Models;

namespace Tests.Common.TestDataBuilders
{
    public class NoteBuilder
    {
        private readonly Faker<Note> _faker;

        public NoteBuilder()
        {
            _faker = new Faker<Note>()
                .RuleFor(n => n.Id, f => Guid.NewGuid())
                .RuleFor(n => n.Name, f => f.Commerce.ProductAdjective())
                .RuleFor(n => n.Category, f => f.PickRandom("Top", "Middle", "Base", "Floral", "Woody", "Fresh"))
                .RuleFor(n => n.Description, f => f.Lorem.Sentence(10))
                .RuleFor(n => n.PerfumeNotes, new List<PerfumeNote>());
        }

        public NoteBuilder WithId(Guid id)
        {
            _faker.RuleFor(n => n.Id, id);
            return this;
        }

        public NoteBuilder WithName(string name)
        {
            _faker.RuleFor(n => n.Name, name);
            return this;
        }

        public NoteBuilder WithCategory(string? category)
        {
            _faker.RuleFor(n => n.Category, category);
            return this;
        }

        public NoteBuilder WithDescription(string? description)
        {
            _faker.RuleFor(n => n.Description, description);
            return this;
        }

        public NoteBuilder WithoutCategory()
        {
            _faker.RuleFor(n => n.Category, (string?)null);
            return this;
        }

        public NoteBuilder WithoutDescription()
        {
            _faker.RuleFor(n => n.Description, (string?)null);
            return this;
        }

        public NoteBuilder WithPerfumeNotes(List<PerfumeNote> perfumeNotes)
        {
            _faker.RuleFor(n => n.PerfumeNotes, perfumeNotes);
            return this;
        }

        public NoteBuilder WithLongName()
        {
            _faker.RuleFor(n => n.Name, f => f.Random.String2(150)); // Max length
            return this;
        }

        public NoteBuilder WithTooLongName()
        {
            _faker.RuleFor(n => n.Name, f => f.Random.String2(151)); // Exceeds max length
            return this;
        }

        public NoteBuilder WithLongCategory()
        {
            _faker.RuleFor(n => n.Category, f => f.Random.String2(100)); // Max length
            return this;
        }

        public NoteBuilder WithTooLongCategory()
        {
            _faker.RuleFor(n => n.Category, f => f.Random.String2(101)); // Exceeds max length
            return this;
        }

        public NoteBuilder WithLongDescription()
        {
            _faker.RuleFor(n => n.Description, f => f.Random.String2(500)); // Max length
            return this;
        }

        public NoteBuilder WithTooLongDescription()
        {
            _faker.RuleFor(n => n.Description, f => f.Random.String2(501)); // Exceeds max length
            return this;
        }

        public Note Build() => _faker.Generate();

        public List<Note> Build(int count) => _faker.Generate(count);

        // Static convenience methods
        public static Note CreateValid() => new NoteBuilder().Build();

        public static Note CreateWithName(string name) => new NoteBuilder().WithName(name).Build();

        public static Note CreateWithCategory(string category) => new NoteBuilder().WithCategory(category).Build();

        public static Note CreateWithNameAndCategory(string name, string category) => 
            new NoteBuilder().WithName(name).WithCategory(category).Build();

        public static List<Note> CreateValidList(int count) => new NoteBuilder().Build(count);

        public static List<Note> CreateWithCategories(params string[] categories) => 
            categories.Select(category => new NoteBuilder().WithCategory(category).Build()).ToList();

        public static Note CreateWithoutCategory() => new NoteBuilder().WithoutCategory().Build();
    }
}