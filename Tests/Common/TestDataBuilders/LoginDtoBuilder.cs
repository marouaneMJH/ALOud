using Bogus;
using ALOud.DTOs;

namespace Tests.Common.TestDataBuilders
{
    /// <summary>
    /// Test data builder for LoginDto using the Builder pattern.
    /// Uses Bogus library to generate realistic fake data.
    /// </summary>
    public class LoginDtoBuilder
    {
        private readonly Faker<LoginDto> _faker;

        public LoginDtoBuilder()
        {
            _faker = new Faker<LoginDto>()
                .RuleFor(dto => dto.Email, f => f.Internet.Email().ToLowerInvariant())
                .RuleFor(dto => dto.Password, f => f.Internet.Password());
        }

        /// <summary>
        /// Sets the email address.
        /// </summary>
        public LoginDtoBuilder WithEmail(string email)
        {
            _faker.RuleFor(dto => dto.Email, email);
            return this;
        }

        /// <summary>
        /// Sets the password.
        /// </summary>
        public LoginDtoBuilder WithPassword(string password)
        {
            _faker.RuleFor(dto => dto.Password, password);
            return this;
        }

        /// <summary>
        /// Creates a DTO with a specific user's credentials.
        /// </summary>
        public LoginDtoBuilder WithCredentials(string email, string password)
        {
            return WithEmail(email).WithPassword(password);
        }

        /// <summary>
        /// Creates a DTO with an invalid email format.
        /// </summary>
        public LoginDtoBuilder WithInvalidEmail()
        {
            _faker.RuleFor(dto => dto.Email, "invalid-email-format");
            return this;
        }

        /// <summary>
        /// Creates a DTO with empty credentials.
        /// </summary>
        public LoginDtoBuilder WithEmptyCredentials()
        {
            _faker.RuleFor(dto => dto.Email, string.Empty);
            _faker.RuleFor(dto => dto.Password, string.Empty);
            return this;
        }

        /// <summary>
        /// Creates a DTO with empty email.
        /// </summary>
        public LoginDtoBuilder WithEmptyEmail()
        {
            _faker.RuleFor(dto => dto.Email, string.Empty);
            return this;
        }

        /// <summary>
        /// Creates a DTO with empty password.
        /// </summary>
        public LoginDtoBuilder WithEmptyPassword()
        {
            _faker.RuleFor(dto => dto.Password, string.Empty);
            return this;
        }

        /// <summary>
        /// Builds the LoginDto instance with the configured values.
        /// </summary>
        public LoginDto Build()
        {
            return _faker.Generate();
        }

        /// <summary>
        /// Builds multiple LoginDto instances with the configured values.
        /// </summary>
        public List<LoginDto> Build(int count)
        {
            return _faker.Generate(count);
        }
    }
}