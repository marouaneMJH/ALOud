using Bogus;
using ALOud.DTOs.Families;

namespace Tests.Common.TestDataBuilders
{
    public class UpdateFamilyDtoBuilder
    {
        private readonly Faker<UpdateFamilyDto> _faker;

        public UpdateFamilyDtoBuilder()
        {
            _faker = new Faker<UpdateFamilyDto>()
                .RuleFor(dto => dto.Id, f => Guid.NewGuid())
                .RuleFor(dto => dto.Name, f => f.Commerce.ProductName())
                .RuleFor(dto => dto.Description, f => f.Lorem.Sentence(10));
        }

        public UpdateFamilyDtoBuilder WithId(Guid id)
        {
            _faker.RuleFor(dto => dto.Id, id);
            return this;
        }

        public UpdateFamilyDtoBuilder WithName(string name)
        {
            _faker.RuleFor(dto => dto.Name, name);
            return this;
        }

        public UpdateFamilyDtoBuilder WithDescription(string? description)
        {
            _faker.RuleFor(dto => dto.Description, description);
            return this;
        }

        public UpdateFamilyDtoBuilder WithoutDescription()
        {
            _faker.RuleFor(dto => dto.Description, (string?)null);
            return this;
        }

        public UpdateFamilyDtoBuilder WithEmptyName()
        {
            _faker.RuleFor(dto => dto.Name, string.Empty);
            return this;
        }

        public UpdateFamilyDtoBuilder WithNullName()
        {
            _faker.RuleFor(dto => dto.Name, (string)null!);
            return this;
        }

        public UpdateFamilyDtoBuilder WithTooLongName()
        {
            _faker.RuleFor(dto => dto.Name, f => f.Random.String2(151)); // Exceeds 150 char limit
            return this;
        }

        public UpdateFamilyDtoBuilder WithTooLongDescription()
        {
            _faker.RuleFor(dto => dto.Description, f => f.Random.String2(501)); // Exceeds 500 char limit
            return this;
        }

        public UpdateFamilyDto Build() => _faker.Generate();

        public List<UpdateFamilyDto> Build(int count) => _faker.Generate(count);

        // Static convenience methods
        public static UpdateFamilyDto CreateValid() => new UpdateFamilyDtoBuilder().Build();

        public static UpdateFamilyDto CreateWithId(Guid id) => new UpdateFamilyDtoBuilder().WithId(id).Build();

        public static UpdateFamilyDto CreateInvalid() => new UpdateFamilyDtoBuilder().WithEmptyName().Build();
    }
}