using Bogus;
using ALOud.DTOs.Notes;

namespace Tests.Common.TestDataBuilders
{
    public class UpdateNoteDtoBuilder
    {
        private readonly Faker<UpdateNoteDto> _faker;

        public UpdateNoteDtoBuilder()
        {
            _faker = new Faker<UpdateNoteDto>()
                .RuleFor(dto => dto.Id, f => Guid.NewGuid())
                .RuleFor(dto => dto.Name, f => f.Commerce.ProductAdjective())
                .RuleFor(dto => dto.Category, f => f.PickRandom("Top", "Middle", "Base", "Floral", "Woody", "Fresh"))
                .RuleFor(dto => dto.Description, f => f.Lorem.Sentence(10));
        }

        public UpdateNoteDtoBuilder WithId(Guid id)
        {
            _faker.RuleFor(dto => dto.Id, id);
            return this;
        }

        public UpdateNoteDtoBuilder WithName(string name)
        {
            _faker.RuleFor(dto => dto.Name, name);
            return this;
        }

        public UpdateNoteDtoBuilder WithCategory(string? category)
        {
            _faker.RuleFor(dto => dto.Category, category);
            return this;
        }

        public UpdateNoteDtoBuilder WithDescription(string? description)
        {
            _faker.RuleFor(dto => dto.Description, description);
            return this;
        }

        public UpdateNoteDtoBuilder WithoutCategory()
        {
            _faker.RuleFor(dto => dto.Category, (string?)null);
            return this;
        }

        public UpdateNoteDtoBuilder WithoutDescription()
        {
            _faker.RuleFor(dto => dto.Description, (string?)null);
            return this;
        }

        public UpdateNoteDtoBuilder WithEmptyName()
        {
            _faker.RuleFor(dto => dto.Name, string.Empty);
            return this;
        }

        public UpdateNoteDtoBuilder WithNullName()
        {
            _faker.RuleFor(dto => dto.Name, (string)null!);
            return this;
        }

        public UpdateNoteDtoBuilder WithTooLongName()
        {
            _faker.RuleFor(dto => dto.Name, f => f.Random.String2(151)); // Exceeds 150 char limit
            return this;
        }

        public UpdateNoteDtoBuilder WithTooLongCategory()
        {
            _faker.RuleFor(dto => dto.Category, f => f.Random.String2(101)); // Exceeds 100 char limit
            return this;
        }

        public UpdateNoteDtoBuilder WithTooLongDescription()
        {
            _faker.RuleFor(dto => dto.Description, f => f.Random.String2(501)); // Exceeds 500 char limit
            return this;
        }

        public UpdateNoteDto Build() => _faker.Generate();

        public List<UpdateNoteDto> Build(int count) => _faker.Generate(count);

        // Static convenience methods
        public static UpdateNoteDto CreateValid() => new UpdateNoteDtoBuilder().Build();

        public static UpdateNoteDto CreateWithId(Guid id) => new UpdateNoteDtoBuilder().WithId(id).Build();

        public static UpdateNoteDto CreateInvalid() => new UpdateNoteDtoBuilder().WithEmptyName().Build();
    }
}