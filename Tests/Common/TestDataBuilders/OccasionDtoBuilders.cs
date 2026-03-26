using Bogus;
using ALOud.DTOs.Occasions;

namespace Tests.Common.TestDataBuilders
{
    public class CreateOccasionDtoBuilder
    {
        private readonly Faker<CreateOccasionDto> _faker;

        public CreateOccasionDtoBuilder()
        {
            _faker = new Faker<CreateOccasionDto>()
                .RuleFor(dto => dto.Name, f => f.PickRandom("Office", "Evening", "Date Night", "Casual", "Formal", "Party"));
        }

        public CreateOccasionDtoBuilder WithName(string name)
        {
            _faker.RuleFor(dto => dto.Name, name);
            return this;
        }

        public CreateOccasionDtoBuilder WithEmptyName()
        {
            _faker.RuleFor(dto => dto.Name, string.Empty);
            return this;
        }

        public CreateOccasionDtoBuilder WithNullName()
        {
            _faker.RuleFor(dto => dto.Name, (string)null!);
            return this;
        }

        public CreateOccasionDtoBuilder WithTooLongName()
        {
            _faker.RuleFor(dto => dto.Name, f => f.Random.String2(101)); // Exceeds 100 char limit
            return this;
        }

        public CreateOccasionDto Build() => _faker.Generate();

        public List<CreateOccasionDto> Build(int count) => _faker.Generate(count);

        // Static convenience methods
        public static CreateOccasionDto CreateValid() => new CreateOccasionDtoBuilder().Build();

        public static CreateOccasionDto CreateWithName(string name) => new CreateOccasionDtoBuilder().WithName(name).Build();

        public static CreateOccasionDto CreateInvalid() => new CreateOccasionDtoBuilder().WithEmptyName().Build();
    }

    public class UpdateOccasionDtoBuilder
    {
        private readonly Faker<UpdateOccasionDto> _faker;

        public UpdateOccasionDtoBuilder()
        {
            _faker = new Faker<UpdateOccasionDto>()
                .RuleFor(dto => dto.Id, f => Guid.NewGuid())
                .RuleFor(dto => dto.Name, f => f.PickRandom("Office", "Evening", "Date Night", "Casual", "Formal", "Party"));
        }

        public UpdateOccasionDtoBuilder WithId(Guid id)
        {
            _faker.RuleFor(dto => dto.Id, id);
            return this;
        }

        public UpdateOccasionDtoBuilder WithName(string name)
        {
            _faker.RuleFor(dto => dto.Name, name);
            return this;
        }

        public UpdateOccasionDtoBuilder WithEmptyName()
        {
            _faker.RuleFor(dto => dto.Name, string.Empty);
            return this;
        }

        public UpdateOccasionDtoBuilder WithTooLongName()
        {
            _faker.RuleFor(dto => dto.Name, f => f.Random.String2(101)); // Exceeds 100 char limit
            return this;
        }

        public UpdateOccasionDto Build() => _faker.Generate();

        // Static convenience methods
        public static UpdateOccasionDto CreateValid() => new UpdateOccasionDtoBuilder().Build();

        public static UpdateOccasionDto CreateWithId(Guid id) => new UpdateOccasionDtoBuilder().WithId(id).Build();

        public static UpdateOccasionDto CreateInvalid() => new UpdateOccasionDtoBuilder().WithEmptyName().Build();
    }
}