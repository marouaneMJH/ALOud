using Bogus;
using ALOud.DTOs.Seasons;

namespace Tests.Common.TestDataBuilders
{
    public class UpdateSeasonDtoBuilder
    {
        private readonly Faker<UpdateSeasonDto> _faker;

        public UpdateSeasonDtoBuilder()
        {
            _faker = new Faker<UpdateSeasonDto>()
                .RuleFor(dto => dto.Id, f => Guid.NewGuid())
                .RuleFor(dto => dto.Name, f => f.PickRandom("Spring", "Summer", "Autumn", "Winter", "All Season"));
        }

        public UpdateSeasonDtoBuilder WithId(Guid id)
        {
            _faker.RuleFor(dto => dto.Id, id);
            return this;
        }

        public UpdateSeasonDtoBuilder WithName(string name)
        {
            _faker.RuleFor(dto => dto.Name, name);
            return this;
        }

        public UpdateSeasonDtoBuilder WithEmptyName()
        {
            _faker.RuleFor(dto => dto.Name, string.Empty);
            return this;
        }

        public UpdateSeasonDtoBuilder WithNullName()
        {
            _faker.RuleFor(dto => dto.Name, (string)null!);
            return this;
        }

        public UpdateSeasonDtoBuilder WithTooLongName()
        {
            _faker.RuleFor(dto => dto.Name, f => f.Random.String2(51)); // Exceeds 50 char limit
            return this;
        }

        public UpdateSeasonDto Build() => _faker.Generate();

        public List<UpdateSeasonDto> Build(int count) => _faker.Generate(count);

        // Static convenience methods
        public static UpdateSeasonDto CreateValid() => new UpdateSeasonDtoBuilder().Build();

        public static UpdateSeasonDto CreateWithId(Guid id) => new UpdateSeasonDtoBuilder().WithId(id).Build();

        public static UpdateSeasonDto CreateInvalid() => new UpdateSeasonDtoBuilder().WithEmptyName().Build();
    }
}