using Bogus;
using ALOud.Services.Infrastructure.ExpertSystem.Domain;

namespace Tests.Common.TestDataBuilders
{
    /// <summary>
    /// Test data builder for UserProfile entities using the Builder pattern.
    /// Uses Bogus library to generate realistic fake data for expert system testing.
    /// </summary>
    public class UserProfileBuilder
    {
        private readonly Faker<UserProfile> _faker;

        public UserProfileBuilder()
        {
            _faker = new Faker<UserProfile>()
                .RuleFor(up => up.Climate, f => f.PickRandom<EClimate>())
                .RuleFor(up => up.Occasion, f => f.PickRandom<EOccasion>())
                .RuleFor(up => up.SkinType, f => f.PickRandom<ESkinType>())
                .RuleFor(up => up.Compliment, f => f.PickRandom<EComplimentDesire>())
                .RuleFor(up => up.SeasonPreference, f => f.PickRandom<ESeasonPreference>())
                .RuleFor(up => up.Persona, f => f.PickRandom<EPersona>())
                .RuleFor(up => up.Sensitivity, f => f.PickRandom<ESensitivity>())
                .RuleFor(up => up.WantsLongPerformance, f => f.Random.Bool());
        }

        /// <summary>
        /// Sets the climate preference.
        /// </summary>
        public UserProfileBuilder WithClimate(EClimate climate)
        {
            _faker.RuleFor(up => up.Climate, climate);
            return this;
        }

        /// <summary>
        /// Sets the occasion preference.
        /// </summary>
        public UserProfileBuilder WithOccasion(EOccasion occasion)
        {
            _faker.RuleFor(up => up.Occasion, occasion);
            return this;
        }

        /// <summary>
        /// Sets the skin type.
        /// </summary>
        public UserProfileBuilder WithSkinType(ESkinType skinType)
        {
            _faker.RuleFor(up => up.SkinType, skinType);
            return this;
        }

        /// <summary>
        /// Sets the compliment desire.
        /// </summary>
        public UserProfileBuilder WithCompliment(EComplimentDesire compliment)
        {
            _faker.RuleFor(up => up.Compliment, compliment);
            return this;
        }

        /// <summary>
        /// Sets the season preference.
        /// </summary>
        public UserProfileBuilder WithSeasonPreference(ESeasonPreference seasonPreference)
        {
            _faker.RuleFor(up => up.SeasonPreference, seasonPreference);
            return this;
        }

        /// <summary>
        /// Sets the persona type.
        /// </summary>
        public UserProfileBuilder WithPersona(EPersona persona)
        {
            _faker.RuleFor(up => up.Persona, persona);
            return this;
        }

        /// <summary>
        /// Sets the sensitivity level.
        /// </summary>
        public UserProfileBuilder WithSensitivity(ESensitivity sensitivity)
        {
            _faker.RuleFor(up => up.Sensitivity, sensitivity);
            return this;
        }

        /// <summary>
        /// Sets whether the user wants long performance.
        /// </summary>
        public UserProfileBuilder WithLongPerformance(bool wantsLongPerformance)
        {
            _faker.RuleFor(up => up.WantsLongPerformance, wantsLongPerformance);
            return this;
        }

        /// <summary>
        /// Creates a profile for hot climate office scenario.
        /// </summary>
        public UserProfileBuilder AsHotClimateOfficeWorker()
        {
            return WithClimate(EClimate.Hot)
                   .WithOccasion(EOccasion.Office)
                   .WithPersona(EPersona.Corporate)
                   .WithSensitivity(ESensitivity.None);
        }

        /// <summary>
        /// Creates a profile for cold climate date night scenario.
        /// </summary>
        public UserProfileBuilder AsColdClimateDateNight()
        {
            return WithClimate(EClimate.Cold)
                   .WithOccasion(EOccasion.Date)
                   .WithPersona(EPersona.Sexy)
                   .WithCompliment(EComplimentDesire.Yes)
                   .WithLongPerformance(true);
        }

        /// <summary>
        /// Creates a profile with migraine sensitivity.
        /// </summary>
        public UserProfileBuilder WithMigraineSensitivity()
        {
            return WithSensitivity(ESensitivity.Migraine);
        }

        /// <summary>
        /// Creates a profile for gym/sport activities.
        /// </summary>
        public UserProfileBuilder AsGymGoer()
        {
            return WithOccasion(EOccasion.Gym)
                   .WithPersona(EPersona.Sporty)
                   .WithSensitivity(ESensitivity.PrefersMinimal);
        }

        /// <summary>
        /// Creates a profile for nightlife scenario.
        /// </summary>
        public UserProfileBuilder AsNightlifeEnthusiast()
        {
            return WithOccasion(EOccasion.Nightlife)
                   .WithCompliment(EComplimentDesire.Yes)
                   .WithLongPerformance(true)
                   .WithPersona(EPersona.Sexy);
        }

        /// <summary>
        /// Creates a profile with multiple sensitivities.
        /// </summary>
        public UserProfileBuilder WithMultipleSensitivities()
        {
            return WithSensitivity(ESensitivity.HatesSweet); // Only one can be set at a time
        }

        /// <summary>
        /// Creates a minimal preference profile.
        /// </summary>
        public UserProfileBuilder AsMinimalist()
        {
            return WithPersona(EPersona.Minimalist)
                   .WithSensitivity(ESensitivity.PrefersMinimal);
        }

        /// <summary>
        /// Creates a corporate professional profile.
        /// </summary>
        public UserProfileBuilder AsCorporateProfessional()
        {
            return WithPersona(EPersona.Corporate)
                   .WithOccasion(EOccasion.Office)
                   .WithCompliment(EComplimentDesire.Neutral);
        }

        /// <summary>
        /// Creates an elegant profile for formal occasions.
        /// </summary>
        public UserProfileBuilder AsElegantFormal()
        {
            return WithPersona(EPersona.Elegant)
                   .WithOccasion(EOccasion.Formal)
                   .WithCompliment(EComplimentDesire.Yes);
        }

        /// <summary>
        /// Builds the UserProfile instance with the configured values.
        /// </summary>
        public UserProfile Build()
        {
            return _faker.Generate();
        }

        /// <summary>
        /// Builds multiple UserProfile instances with the configured values.
        /// </summary>
        public List<UserProfile> Build(int count)
        {
            return _faker.Generate(count);
        }
    }
}