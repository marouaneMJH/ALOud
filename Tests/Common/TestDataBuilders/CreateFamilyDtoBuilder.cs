using Bogus;
using ALOud.DTOs.Families;

namespace Tests.Common.TestDataBuilders
{
    public class CreateFamilyDtoBuilder
    {
        private readonly Faker<CreateFamilyDto> _faker;

        public CreateFamilyDtoBuilder()
        {
            _faker = new Faker<CreateFamilyDto>()
                .RuleFor(dto => dto.Name, f => f.Commerce.ProductName())
                .RuleFor(dto => dto.Description, f => f.Lorem.Sentence(10));
        }

        public CreateFamilyDtoBuilder WithName(string name)
        {
            _faker.RuleFor(dto => dto.Name, name);
            return this;
        }

        public CreateFamilyDtoBuilder WithDescription(string? description)
        {
            _faker.RuleFor(dto => dto.Description, description);
            return this;
        }

        public CreateFamilyDtoBuilder WithoutDescription()
        {
            _faker.RuleFor(dto => dto.Description, (string?)null);
            return this;
        }

        public CreateFamilyDtoBuilder WithEmptyName()
        {
            _faker.RuleFor(dto => dto.Name, string.Empty);
            return this;
        }

        public CreateFamilyDtoBuilder WithNullName()
        {
            _faker.RuleFor(dto => dto.Name, (string)null!);
            return this;
        }

        public CreateFamilyDtoBuilder WithTooLongName()
        {
            _faker.RuleFor(dto => dto.Name, f => f.Random.String2(151)); // Exceeds 150 char limit
            return this;
        }

        public CreateFamilyDtoBuilder WithTooLongDescription()
        {
            _faker.RuleFor(dto => dto.Description, f => f.Random.String2(501)); // Exceeds 500 char limit
            return this;
        }

        public CreateFamilyDto Build() => _faker.Generate();

        public List<CreateFamilyDto> Build(int count) => _faker.Generate(count);

        // Static convenience methods
        public static CreateFamilyDto CreateValid() => new CreateFamilyDtoBuilder().Build();

        public static CreateFamilyDto CreateWithName(string name) => new CreateFamilyDtoBuilder().WithName(name).Build();

        public static CreateFamilyDto CreateInvalid() => new CreateFamilyDtoBuilder().WithEmptyName().Build();
    }
}