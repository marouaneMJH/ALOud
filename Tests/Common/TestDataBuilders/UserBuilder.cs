using Bogus;
using ALOud.Models;

namespace Tests.Common.TestDataBuilders
{
    /// <summary>
    /// Test data builder for User entities using the Builder pattern.
    /// Uses Bogus library to generate realistic fake data.
    /// </summary>
    public class UserBuilder
    {
        private readonly Faker<User> _faker;

        public UserBuilder()
        {
            _faker = new Faker<User>()
                .RuleFor(u => u.Id, f => f.Random.Guid())
                .RuleFor(u => u.FirstName, f => f.Name.FirstName())
                .RuleFor(u => u.LastName, f => f.Name.LastName())
                .RuleFor(u => u.Email, (f, u) => f.Internet.Email(u.FirstName, u.LastName).ToLowerInvariant())
                .RuleFor(u => u.PasswordHash, f => f.Internet.Password(16)) // Simulated hash
                .RuleFor(u => u.IsActive, f => true)
                .RuleFor(u => u.IsEmailVerified, f => true)
                .RuleFor(u => u.Address, f => f.Address.FullAddress())
                .RuleFor(u => u.CreatedAt, f => f.Date.Past(1));
        }

        /// <summary>
        /// Sets the user ID to a specific value.
        /// </summary>
        public UserBuilder WithId(Guid id)
        {
            _faker.RuleFor(u => u.Id, id);
            return this;
        }

        /// <summary>
        /// Sets the user's first name.
        /// </summary>
        public UserBuilder WithFirstName(string firstName)
        {
            _faker.RuleFor(u => u.FirstName, firstName);
            return this;
        }

        /// <summary>
        /// Sets the user's last name.
        /// </summary>
        public UserBuilder WithLastName(string lastName)
        {
            _faker.RuleFor(u => u.LastName, lastName);
            return this;
        }

        /// <summary>
        /// Sets the user's email address.
        /// </summary>
        public UserBuilder WithEmail(string email)
        {
            _faker.RuleFor(u => u.Email, email);
            return this;
        }

        /// <summary>
        /// Sets the user's password hash.
        /// </summary>
        public UserBuilder WithPasswordHash(string passwordHash)
        {
            _faker.RuleFor(u => u.PasswordHash, passwordHash);
            return this;
        }

        /// <summary>
        /// Sets the user as active or inactive.
        /// </summary>
        public UserBuilder WithActiveStatus(bool isActive)
        {
            _faker.RuleFor(u => u.IsActive, isActive);
            return this;
        }

        /// <summary>
        /// Sets the email verification status.
        /// </summary>
        public UserBuilder WithEmailVerified(bool isVerified)
        {
            _faker.RuleFor(u => u.IsEmailVerified, isVerified);
            return this;
        }

        /// <summary>
        /// Sets the user's address.
        /// </summary>
        public UserBuilder WithAddress(string address)
        {
            _faker.RuleFor(u => u.Address, address);
            return this;
        }

        /// <summary>
        /// Sets the creation date.
        /// </summary>
        public UserBuilder WithCreatedAt(DateTime createdAt)
        {
            _faker.RuleFor(u => u.CreatedAt, createdAt);
            return this;
        }

        /// <summary>
        /// Creates a user that exists in the system with verified email and active status.
        /// </summary>
        public UserBuilder AsExistingUser()
        {
            return WithActiveStatus(true)
                   .WithEmailVerified(true);
        }

        /// <summary>
        /// Creates a user with unverified email.
        /// </summary>
        public UserBuilder AsUnverifiedUser()
        {
            return WithEmailVerified(false);
        }

        /// <summary>
        /// Creates an inactive user.
        /// </summary>
        public UserBuilder AsInactiveUser()
        {
            return WithActiveStatus(false);
        }

        /// <summary>
        /// Builds the User instance with the configured values.
        /// </summary>
        public User Build()
        {
            return _faker.Generate();
        }

        /// <summary>
        /// Builds multiple User instances with the configured values.
        /// </summary>
        public List<User> Build(int count)
        {
            return _faker.Generate(count);
        }
    }
}