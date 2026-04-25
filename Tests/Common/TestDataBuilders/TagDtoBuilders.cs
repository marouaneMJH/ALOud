using Bogus;
using ALOud.DTOs.Tags;

namespace Tests.Common.TestDataBuilders
{
    public class CreateTagDtoBuilder
    {
        private readonly Faker<CreateTagDto> _faker;

        public CreateTagDtoBuilder()
        {
            _faker = new Faker<CreateTagDto>()
                .RuleFor(dto => dto.Name, f => f.PickRandom("Luxury", "Vintage", "Modern", "Oriental", "Fresh", "Intense"));
        }

        public CreateTagDtoBuilder WithName(string name)
        {
            _faker.RuleFor(dto => dto.Name, name);
            return this;
        }

        public CreateTagDtoBuilder WithEmptyName()
        {
            _faker.RuleFor(dto => dto.Name, string.Empty);
            return this;
        }

        public CreateTagDtoBuilder WithNullName()
        {
            _faker.RuleFor(dto => dto.Name, (string)null!);
            return this;
        }

        public CreateTagDtoBuilder WithTooLongName()
        {
            _faker.RuleFor(dto => dto.Name, f => f.Random.String2(101)); // Exceeds 100 char limit
            return this;
        }

        public CreateTagDto Build() => _faker.Generate();

        public List<CreateTagDto> Build(int count) => _faker.Generate(count);

        // Static convenience methods
        public static CreateTagDto CreateValid() => new CreateTagDtoBuilder().Build();

        public static CreateTagDto CreateWithName(string name) => new CreateTagDtoBuilder().WithName(name).Build();

        public static CreateTagDto CreateInvalid() => new CreateTagDtoBuilder().WithEmptyName().Build();
    }

    public class UpdateTagDtoBuilder
    {
        private readonly Faker<UpdateTagDto> _faker;

        public UpdateTagDtoBuilder()
        {
            _faker = new Faker<UpdateTagDto>()
                .RuleFor(dto => dto.Id, f => Guid.NewGuid())
                .RuleFor(dto => dto.Name, f => f.PickRandom("Luxury", "Vintage", "Modern", "Oriental", "Fresh", "Intense"));
        }

        public UpdateTagDtoBuilder WithId(Guid id)
        {
            _faker.RuleFor(dto => dto.Id, id);
            return this;
        }

        public UpdateTagDtoBuilder WithName(string name)
        {
            _faker.RuleFor(dto => dto.Name, name);
            return this;
        }

        public UpdateTagDtoBuilder WithEmptyName()
        {
            _faker.RuleFor(dto => dto.Name, string.Empty);
            return this;
        }

        public UpdateTagDtoBuilder WithTooLongName()
        {
            _faker.RuleFor(dto => dto.Name, f => f.Random.String2(101)); // Exceeds 100 char limit
            return this;
        }

        public UpdateTagDto Build() => _faker.Generate();

        // Static convenience methods
        public static UpdateTagDto CreateValid() => new UpdateTagDtoBuilder().Build();

        public static UpdateTagDto CreateWithId(Guid id) => new UpdateTagDtoBuilder().WithId(id).Build();

        public static UpdateTagDto CreateInvalid() => new UpdateTagDtoBuilder().WithEmptyName().Build();
    }
}