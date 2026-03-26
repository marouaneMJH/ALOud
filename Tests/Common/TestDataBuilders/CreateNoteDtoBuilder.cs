using Bogus;
using ALOud.DTOs.Notes;

namespace Tests.Common.TestDataBuilders
{
    public class CreateNoteDtoBuilder
    {
        private readonly Faker<CreateNoteDto> _faker;

        public CreateNoteDtoBuilder()
        {
            _faker = new Faker<CreateNoteDto>()
                .RuleFor(dto => dto.Name, f => f.Commerce.ProductAdjective())
                .RuleFor(dto => dto.Category, f => f.PickRandom("Top", "Middle", "Base", "Floral", "Woody", "Fresh"))
                .RuleFor(dto => dto.Description, f => f.Lorem.Sentence(10));
        }

        public CreateNoteDtoBuilder WithName(string name)
        {
            _faker.RuleFor(dto => dto.Name, name);
            return this;
        }

        public CreateNoteDtoBuilder WithCategory(string? category)
        {
            _faker.RuleFor(dto => dto.Category, category);
            return this;
        }

        public CreateNoteDtoBuilder WithDescription(string? description)
        {
            _faker.RuleFor(dto => dto.Description, description);
            return this;
        }

        public CreateNoteDtoBuilder WithoutCategory()
        {
            _faker.RuleFor(dto => dto.Category, (string?)null);
            return this;
        }

        public CreateNoteDtoBuilder WithoutDescription()
        {
            _faker.RuleFor(dto => dto.Description, (string?)null);
            return this;
        }

        public CreateNoteDtoBuilder WithEmptyName()
        {
            _faker.RuleFor(dto => dto.Name, string.Empty);
            return this;
        }

        public CreateNoteDtoBuilder WithNullName()
        {
            _faker.RuleFor(dto => dto.Name, (string)null!);
            return this;
        }

        public CreateNoteDtoBuilder WithTooLongName()
        {
            _faker.RuleFor(dto => dto.Name, f => f.Random.String2(151)); // Exceeds 150 char limit
            return this;
        }

        public CreateNoteDtoBuilder WithTooLongCategory()
        {
            _faker.RuleFor(dto => dto.Category, f => f.Random.String2(101)); // Exceeds 100 char limit
            return this;
        }

        public CreateNoteDtoBuilder WithTooLongDescription()
        {
            _faker.RuleFor(dto => dto.Description, f => f.Random.String2(501)); // Exceeds 500 char limit
            return this;
        }

        public CreateNoteDto Build() => _faker.Generate();

        public List<CreateNoteDto> Build(int count) => _faker.Generate(count);

        // Static convenience methods
        public static CreateNoteDto CreateValid() => new CreateNoteDtoBuilder().Build();

        public static CreateNoteDto CreateWithName(string name) => new CreateNoteDtoBuilder().WithName(name).Build();

        public static CreateNoteDto CreateWithCategory(string category) => new CreateNoteDtoBuilder().WithCategory(category).Build();

        public static CreateNoteDto CreateInvalid() => new CreateNoteDtoBuilder().WithEmptyName().Build();
    }
}