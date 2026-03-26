using Bogus;
using ALOud.DTOs.Seasons;

namespace Tests.Common.TestDataBuilders
{
    public class CreateSeasonDtoBuilder
    {
        private readonly Faker<CreateSeasonDto> _faker;

        public CreateSeasonDtoBuilder()
        {
            _faker = new Faker<CreateSeasonDto>()
                .RuleFor(dto => dto.Name, f => f.PickRandom("Spring", "Summer", "Autumn", "Winter", "All Season"));
        }

        public CreateSeasonDtoBuilder WithName(string name)
        {
            _faker.RuleFor(dto => dto.Name, name);
            return this;
        }

        public CreateSeasonDtoBuilder WithEmptyName()
        {
            _faker.RuleFor(dto => dto.Name, string.Empty);
            return this;
        }

        public CreateSeasonDtoBuilder WithNullName()
        {
            _faker.RuleFor(dto => dto.Name, (string)null!);
            return this;
        }

        public CreateSeasonDtoBuilder WithTooLongName()
        {
            _faker.RuleFor(dto => dto.Name, f => f.Random.String2(51)); // Exceeds 50 char limit
            return this;
        }

        public CreateSeasonDto Build() => _faker.Generate();

        public List<CreateSeasonDto> Build(int count) => _faker.Generate(count);

        // Static convenience methods
        public static CreateSeasonDto CreateValid() => new CreateSeasonDtoBuilder().Build();

        public static CreateSeasonDto CreateWithName(string name) => new CreateSeasonDtoBuilder().WithName(name).Build();

        public static CreateSeasonDto CreateInvalid() => new CreateSeasonDtoBuilder().WithEmptyName().Build();
    }
}