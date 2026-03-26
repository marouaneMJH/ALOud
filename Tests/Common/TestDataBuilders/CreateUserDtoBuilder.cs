using Bogus;
using ALOud.DTOs;

namespace Tests.Common.TestDataBuilders
{
    /// <summary>
    /// Test data builder for CreateUserDto using the Builder pattern.
    /// Uses Bogus library to generate realistic fake data that passes validation.
    /// </summary>
    public class CreateUserDtoBuilder
    {
        private readonly Faker<CreateUserDto> _faker;

        public CreateUserDtoBuilder()
        {
            _faker = new Faker<CreateUserDto>()
                .RuleFor(dto => dto.FirstName, f => f.Name.FirstName())
                .RuleFor(dto => dto.LastName, f => f.Name.LastName())
                .RuleFor(dto => dto.Email, (f, dto) => f.Internet.Email(dto.FirstName, dto.LastName).ToLowerInvariant())
                .RuleFor(dto => dto.Password, f => GenerateValidPassword(f))
                .RuleFor(dto => dto.ConfirmPassword, (f, dto) => dto.Password) // Match password
                .RuleFor(dto => dto.Address, f => f.Address.FullAddress())
                .RuleFor(dto => dto.AcceptedTerms, true);
        }

        /// <summary>
        /// Generates a password that meets the validation requirements.
        /// Must contain: uppercase, lowercase, digit, special character, min 8 chars.
        /// </summary>
        private static string GenerateValidPassword(Faker faker)
        {
            // Create a password that satisfies all requirements
            var password = faker.Internet.Password(8, false, "", "Aa1@");
            // Ensure it has all required character types
            return $"Password1@{faker.Random.Number(10, 99)}";
        }

        /// <summary>
        /// Sets the first name.
        /// </summary>
        public CreateUserDtoBuilder WithFirstName(string firstName)
        {
            _faker.RuleFor(dto => dto.FirstName, firstName);
            return this;
        }

        /// <summary>
        /// Sets the last name.
        /// </summary>
        public CreateUserDtoBuilder WithLastName(string lastName)
        {
            _faker.RuleFor(dto => dto.LastName, lastName);
            return this;
        }

        /// <summary>
        /// Sets the email address.
        /// </summary>
        public CreateUserDtoBuilder WithEmail(string email)
        {
            _faker.RuleFor(dto => dto.Email, email);
            return this;
        }

        /// <summary>
        /// Sets the password and ensures ConfirmPassword matches.
        /// </summary>
        public CreateUserDtoBuilder WithPassword(string password)
        {
            _faker.RuleFor(dto => dto.Password, password);
            _faker.RuleFor(dto => dto.ConfirmPassword, password);
            return this;
        }

        /// <summary>
        /// Sets the password and confirm password separately (for mismatch scenarios).
        /// </summary>
        public CreateUserDtoBuilder WithPasswords(string password, string confirmPassword)
        {
            _faker.RuleFor(dto => dto.Password, password);
            _faker.RuleFor(dto => dto.ConfirmPassword, confirmPassword);
            return this;
        }

        /// <summary>
        /// Sets the address.
        /// </summary>
        public CreateUserDtoBuilder WithAddress(string address)
        {
            _faker.RuleFor(dto => dto.Address, address);
            return this;
        }

        /// <summary>
        /// Sets the terms acceptance status.
        /// </summary>
        public CreateUserDtoBuilder WithTermsAccepted(bool accepted)
        {
            _faker.RuleFor(dto => dto.AcceptedTerms, accepted);
            return this;
        }

        /// <summary>
        /// Creates a DTO with an invalid email format.
        /// </summary>
        public CreateUserDtoBuilder WithInvalidEmail()
        {
            _faker.RuleFor(dto => dto.Email, "invalid-email");
            return this;
        }

        /// <summary>
        /// Creates a DTO with a password that doesn't meet requirements.
        /// </summary>
        public CreateUserDtoBuilder WithWeakPassword()
        {
            _faker.RuleFor(dto => dto.Password, "weak");
            _faker.RuleFor(dto => dto.ConfirmPassword, "weak");
            return this;
        }

        /// <summary>
        /// Creates a DTO with mismatched passwords.
        /// </summary>
        public CreateUserDtoBuilder WithMismatchedPasswords()
        {
            var validPassword = "Password1@";
            _faker.RuleFor(dto => dto.Password, validPassword);
            _faker.RuleFor(dto => dto.ConfirmPassword, "DifferentPassword1@");
            return this;
        }

        /// <summary>
        /// Creates a DTO with terms not accepted.
        /// </summary>
        public CreateUserDtoBuilder WithTermsNotAccepted()
        {
            return WithTermsAccepted(false);
        }

        /// <summary>
        /// Creates a DTO with address that's too short.
        /// </summary>
        public CreateUserDtoBuilder WithShortAddress()
        {
            _faker.RuleFor(dto => dto.Address, "Short");
            return this;
        }

        /// <summary>
        /// Builds the CreateUserDto instance with the configured values.
        /// </summary>
        public CreateUserDto Build()
        {
            return _faker.Generate();
        }

        /// <summary>
        /// Builds multiple CreateUserDto instances with the configured values.
        /// </summary>
        public List<CreateUserDto> Build(int count)
        {
            return _faker.Generate(count);
        }
    }
}