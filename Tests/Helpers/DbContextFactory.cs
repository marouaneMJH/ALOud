using Microsoft.EntityFrameworkCore;
using ALOud.Data;

namespace ALOud.Tests.Helpers
{
    /// <summary>
    /// Factory for creating in-memory ALOudDbContext instances for unit testing.
    /// Uses EF Core InMemory provider for isolated test environments.
    /// </summary>
    public static class DbContextFactory
    {
        /// <summary>
        /// Creates a new ALOudDbContext instance configured with InMemory database.
        /// Each context uses a unique database name to ensure test isolation.
        /// </summary>
        /// <param name="databaseName">Optional database name. If not provided, generates a unique name.</param>
        /// <returns>A new ALOudDbContext instance configured for InMemory testing.</returns>
        public static ALOudDbContext CreateInMemoryContext(string? databaseName = null)
        {
            // Generate unique database name if not provided to ensure test isolation
            var dbName = databaseName ?? Guid.NewGuid().ToString();
            
            var options = new DbContextOptionsBuilder<ALOudDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;

            return new ALOudDbContext(options);
        }

        /// <summary>
        /// Creates a new ALOudDbContext instance and ensures the database is created.
        /// Useful for tests that require the database schema to be initialized.
        /// </summary>
        /// <param name="databaseName">Optional database name. If not provided, generates a unique name.</param>
        /// <returns>A new ALOudDbContext instance with database created.</returns>
        public static ALOudDbContext CreateAndEnsureCreated(string? databaseName = null)
        {
            var context = CreateInMemoryContext(databaseName);
            context.Database.EnsureCreated();
            return context;
        }

        /// <summary>
        /// Creates a new ALOudDbContext instance with sample seed data.
        /// Useful for tests that require pre-populated data.
        /// </summary>
        /// <param name="databaseName">Optional database name. If not provided, generates a unique name.</param>
        /// <returns>A new ALOudDbContext instance with seeded test data.</returns>
        public static ALOudDbContext CreateWithSeedData(string? databaseName = null)
        {
            var context = CreateAndEnsureCreated(databaseName);
            SeedTestData(context);
            return context;
        }

        /// <summary>
        /// Seeds the context with basic test data for common test scenarios.
        /// </summary>
        /// <param name="context">The context to seed with test data.</param>
        private static void SeedTestData(ALOudDbContext context)
        {
            // Sample brands
            var chanel = new Models.Brand { Id = Guid.NewGuid(), Name = "Chanel" };
            var dior = new Models.Brand { Id = Guid.NewGuid(), Name = "Dior" };
            
            context.Brands.AddRange(chanel, dior);

            // Sample families
            var floral = new Models.Family { Id = Guid.NewGuid(), Name = "Floral", Description = "Floral fragrances" };
            var oriental = new Models.Family { Id = Guid.NewGuid(), Name = "Oriental", Description = "Oriental fragrances" };
            
            context.Families.AddRange(floral, oriental);

            // Sample notes
            var rose = new Models.Note { Id = Guid.NewGuid(), Name = "Rose", Category = "Floral" };
            var vanilla = new Models.Note { Id = Guid.NewGuid(), Name = "Vanilla", Category = "Sweet" };
            
            context.Notes.AddRange(rose, vanilla);

            // Sample accords
            var romantic = new Models.Accord { Id = Guid.NewGuid(), Name = "Romantic", Description = "Romantic scent profile" };
            var powdery = new Models.Accord { Id = Guid.NewGuid(), Name = "Powdery", Description = "Powdery scent profile" };
            
            context.Accords.AddRange(romantic, powdery);

            // Sample tags
            var elegant = new Models.Tag { Id = Guid.NewGuid(), Name = "Elegant" };
            var classic = new Models.Tag { Id = Guid.NewGuid(), Name = "Classic" };
            
            context.Tags.AddRange(elegant, classic);

            // Sample seasons
            var spring = new Models.Season { Id = Guid.NewGuid(), Name = "Spring" };
            var winter = new Models.Season { Id = Guid.NewGuid(), Name = "Winter" };
            
            context.Seasons.AddRange(spring, winter);

            // Sample occasions
            var office = new Models.Occasion { Id = Guid.NewGuid(), Name = "Office" };
            var date = new Models.Occasion { Id = Guid.NewGuid(), Name = "Date Night" };
            
            context.Occasions.AddRange(office, date);

            // Sample perfume
            var coco = new Models.Perfume
            {
                Id = Guid.NewGuid(),
                Name = "Coco Mademoiselle",
                BrandId = chanel.Id,
                Brand = chanel,
                Description = "A timeless fragrance",
                Intensity = "Moderate",
                Longevity = "Long-lasting",
                Sillage = "Moderate",
                GenderProfile = "Women",
                PriceRange = "Luxury",
                Price = 120.00m,
                CreatedAt = DateTime.UtcNow
            };
            
            context.Perfumes.Add(coco);

            // Save all changes
            context.SaveChanges();
        }
    }
}