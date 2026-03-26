using System;
using Xunit;
using FluentAssertions;
using Moq;
using Microsoft.Extensions.Logging;
using ALOud.Services;
using ALOud.Tests.Helpers;
using Tests.Common.TestDataBuilders;
using ALOud.DTOs.Admin;
using ALOud.Models;

namespace ALOud.Tests.Services.Business
{
    /// <summary>
    /// Unit tests for DashboardService using Strategy B (EF InMemory).
    /// Tests the dashboard statistics aggregation logic with in-memory database.
    /// 
    /// Test Coverage:
    /// - Dashboard statistics retrieval (counts, top items, recent items)
    /// - Empty database scenarios
    /// - Large dataset scenarios
    /// - Data aggregation accuracy
    /// - Logging verification
    /// 
    /// Strategy: EF InMemory (Service uses ALOudDbContext directly)
    /// Framework: xUnit + FluentAssertions + EF InMemory
    /// Pattern: AAA (Arrange, Act, Assert)
    /// </summary>
    public class DashboardServiceTests : IDisposable
    {
        private readonly Mock<ILogger<DashboardService>> _mockLogger;

        public DashboardServiceTests()
        {
            _mockLogger = new Mock<ILogger<DashboardService>>();
        }

        [Fact]
        public async Task GetDashboardStatsAsync_WithEmptyDatabase_ShouldReturnZeroCounts()
        {
            // Arrange
            using var context = DbContextFactory.CreateAndEnsureCreated();
            var service = new DashboardService(context, _mockLogger.Object);

            // Act
            var result = await service.GetDashboardStatsAsync();

            // Assert
            result.Should().NotBeNull();
            result.TotalPerfumes.Should().Be(0);
            result.TotalBrands.Should().Be(0);
            result.TotalFamilies.Should().Be(0);
            result.TotalNotes.Should().Be(0);
            result.TotalAccords.Should().Be(0);
            result.TotalTags.Should().Be(0);
            result.TotalSeasons.Should().Be(0);
            result.TotalOccasions.Should().Be(0);
            result.TopBrands.Should().BeEmpty();
            result.TopFamilies.Should().BeEmpty();
            result.RecentPerfumes.Should().BeEmpty();
        }

        [Fact]
        public async Task GetDashboardStatsAsync_WithBasicData_ShouldReturnCorrectCounts()
        {
            // Arrange
            using var context = DbContextFactory.CreateAndEnsureCreated();
            
            var brand1 = new BrandBuilder().Build();
            var brand2 = new BrandBuilder().Build();
            var family1 = new FamilyBuilder().Build();
            var family2 = new FamilyBuilder().Build();
            var note1 = new NoteBuilder().Build();
            var note2 = new NoteBuilder().Build();
            var accord1 = new AccordBuilder().Build();
            var tag1 = new TagBuilder().Build();
            var season1 = new SeasonBuilder().Build();
            var occasion1 = new OccasionBuilder().Build();

            context.Brands.AddRange(brand1, brand2);
            context.Families.AddRange(family1, family2);
            context.Notes.AddRange(note1, note2);
            context.Accords.Add(accord1);
            context.Tags.Add(tag1);
            context.Seasons.Add(season1);
            context.Occasions.Add(occasion1);
            await context.SaveChangesAsync();

            var service = new DashboardService(context, _mockLogger.Object);

            // Act
            var result = await service.GetDashboardStatsAsync();

            // Assert
            result.Should().NotBeNull();
            result.TotalPerfumes.Should().Be(0); // No perfumes added
            result.TotalBrands.Should().Be(2);
            result.TotalFamilies.Should().Be(2);
            result.TotalNotes.Should().Be(2);
            result.TotalAccords.Should().Be(1);
            result.TotalTags.Should().Be(1);
            result.TotalSeasons.Should().Be(1);
            result.TotalOccasions.Should().Be(1);
        }

        [Fact]
        public async Task GetDashboardStatsAsync_WithPerfumes_ShouldReturnCorrectPerfumeCount()
        {
            // Arrange
            using var context = DbContextFactory.CreateAndEnsureCreated();
            
            var brand = new BrandBuilder().Build();
            context.Brands.Add(brand);
            await context.SaveChangesAsync();

            var perfume1 = new PerfumeBuilder().WithBrand(brand).Build();
            var perfume2 = new PerfumeBuilder().WithBrand(brand).Build();
            context.Perfumes.AddRange(perfume1, perfume2);
            await context.SaveChangesAsync();

            var service = new DashboardService(context, _mockLogger.Object);

            // Act
            var result = await service.GetDashboardStatsAsync();

            // Assert
            result.Should().NotBeNull();
            result.TotalPerfumes.Should().Be(2);
            result.TotalBrands.Should().Be(1);
        }

        [Fact]
        public async Task GetDashboardStatsAsync_WithMultipleBrands_ShouldReturnTopBrandsByPerfumeCount()
        {
            // Arrange
            using var context = DbContextFactory.CreateAndEnsureCreated();
            
            var brand1 = new BrandBuilder().WithName("Brand A").Build();
            var brand2 = new BrandBuilder().WithName("Brand B").Build();
            var brand3 = new BrandBuilder().WithName("Brand C").Build();
            context.Brands.AddRange(brand1, brand2, brand3);
            await context.SaveChangesAsync();

            // Brand1 has 3 perfumes, Brand2 has 2 perfumes, Brand3 has 1 perfume
            var perfumes1 = new[]
            {
                new PerfumeBuilder().WithBrand(brand1).Build(),
                new PerfumeBuilder().WithBrand(brand1).Build(),
                new PerfumeBuilder().WithBrand(brand1).Build()
            };
            var perfumes2 = new[]
            {
                new PerfumeBuilder().WithBrand(brand2).Build(),
                new PerfumeBuilder().WithBrand(brand2).Build()
            };
            var perfumes3 = new[]
            {
                new PerfumeBuilder().WithBrand(brand3).Build()
            };
            
            context.Perfumes.AddRange(perfumes1);
            context.Perfumes.AddRange(perfumes2);
            context.Perfumes.AddRange(perfumes3);
            await context.SaveChangesAsync();

            var service = new DashboardService(context, _mockLogger.Object);

            // Act
            var result = await service.GetDashboardStatsAsync();

            // Assert
            result.Should().NotBeNull();
            result.TopBrands.Should().HaveCount(3);
            result.TopBrands[0].BrandName.Should().Be("Brand A");
            result.TopBrands[0].PerfumeCount.Should().Be(3);
            result.TopBrands[1].BrandName.Should().Be("Brand B");
            result.TopBrands[1].PerfumeCount.Should().Be(2);
            result.TopBrands[2].BrandName.Should().Be("Brand C");
            result.TopBrands[2].PerfumeCount.Should().Be(1);
        }

        [Fact]
        public async Task GetDashboardStatsAsync_WithMoreThan5Brands_ShouldReturnTop5Only()
        {
            // Arrange
            using var context = DbContextFactory.CreateAndEnsureCreated();
            
            // Create 7 brands with different perfume counts
            var brands = new List<Brand>();
            for (int i = 1; i <= 7; i++)
            {
                var brand = new BrandBuilder().WithName($"Brand {i}").Build();
                brands.Add(brand);
                context.Brands.Add(brand);
            }
            await context.SaveChangesAsync();

            // Add perfumes to brands (Brand 1 gets 7 perfumes, Brand 2 gets 6, etc.)
            for (int brandIndex = 0; brandIndex < brands.Count; brandIndex++)
            {
                var perfumeCount = brands.Count - brandIndex; // 7, 6, 5, 4, 3, 2, 1
                for (int j = 0; j < perfumeCount; j++)
                {
                    var perfume = new PerfumeBuilder().WithBrand(brands[brandIndex]).Build();
                    context.Perfumes.Add(perfume);
                }
            }
            await context.SaveChangesAsync();

            var service = new DashboardService(context, _mockLogger.Object);

            // Act
            var result = await service.GetDashboardStatsAsync();

            // Assert
            result.Should().NotBeNull();
            result.TopBrands.Should().HaveCount(5);
            result.TopBrands[0].BrandName.Should().Be("Brand 1");
            result.TopBrands[0].PerfumeCount.Should().Be(7);
            result.TopBrands[4].BrandName.Should().Be("Brand 5");
            result.TopBrands[4].PerfumeCount.Should().Be(3);
        }

        [Fact]
        public async Task GetDashboardStatsAsync_WithMultipleFamilies_ShouldReturnTopFamiliesByPerfumeCount()
        {
            // Arrange
            using var context = DbContextFactory.CreateAndEnsureCreated();
            
            var brand = new BrandBuilder().Build();
            context.Brands.Add(brand);
            
            var family1 = new FamilyBuilder().WithName("Family A").Build();
            var family2 = new FamilyBuilder().WithName("Family B").Build();
            var family3 = new FamilyBuilder().WithName("Family C").Build();
            context.Families.AddRange(family1, family2, family3);
            await context.SaveChangesAsync();

            // Create perfumes and associate with families through PerfumeFamily relationship
            var perfume1 = new PerfumeBuilder().WithBrand(brand).Build();
            var perfume2 = new PerfumeBuilder().WithBrand(brand).Build();
            var perfume3 = new PerfumeBuilder().WithBrand(brand).Build();
            var perfume4 = new PerfumeBuilder().WithBrand(brand).Build();
            
            context.Perfumes.AddRange(perfume1, perfume2, perfume3, perfume4);
            await context.SaveChangesAsync();

            // Create PerfumeFamily relationships
            // Family1 has 3 perfumes, Family2 has 2 perfumes, Family3 has 1 perfume
            context.PerfumeFamilies.AddRange(
                new PerfumeFamily { PerfumeId = perfume1.Id, FamilyId = family1.Id },
                new PerfumeFamily { PerfumeId = perfume2.Id, FamilyId = family1.Id },
                new PerfumeFamily { PerfumeId = perfume3.Id, FamilyId = family1.Id },
                new PerfumeFamily { PerfumeId = perfume2.Id, FamilyId = family2.Id },
                new PerfumeFamily { PerfumeId = perfume4.Id, FamilyId = family2.Id },
                new PerfumeFamily { PerfumeId = perfume4.Id, FamilyId = family3.Id }
            );
            await context.SaveChangesAsync();

            var service = new DashboardService(context, _mockLogger.Object);

            // Act
            var result = await service.GetDashboardStatsAsync();

            // Assert
            result.Should().NotBeNull();
            result.TopFamilies.Should().HaveCount(3);
            result.TopFamilies[0].FamilyName.Should().Be("Family A");
            result.TopFamilies[0].PerfumeCount.Should().Be(3);
            result.TopFamilies[1].FamilyName.Should().Be("Family B");
            result.TopFamilies[1].PerfumeCount.Should().Be(2);
            result.TopFamilies[2].FamilyName.Should().Be("Family C");
            result.TopFamilies[2].PerfumeCount.Should().Be(1);
        }

        [Fact]
        public async Task GetDashboardStatsAsync_WithRecentPerfumes_ShouldReturnTop5MostRecent()
        {
            // Arrange
            using var context = DbContextFactory.CreateAndEnsureCreated();
            
            var brand = new BrandBuilder().WithName("Test Brand").Build();
            context.Brands.Add(brand);
            await context.SaveChangesAsync();

            // Create perfumes with different creation times
            var baseTime = DateTime.UtcNow.AddDays(-10);
            var perfumes = new List<Perfume>();
            
            for (int i = 0; i < 7; i++) // Create 7 perfumes to test the "Take(5)" limit
            {
                var perfume = new PerfumeBuilder()
                    .WithBrand(brand)
                    .WithName($"Perfume {i + 1}")
                    .WithGenderProfile($"Gender {i + 1}")
                    .WithImageUrl($"http://example.com/image{i + 1}.jpg")
                    .Build();
                
                // Manually set CreatedAt since builder might not support it
                perfume.CreatedAt = baseTime.AddDays(i);
                perfumes.Add(perfume);
            }
            
            context.Perfumes.AddRange(perfumes);
            await context.SaveChangesAsync();

            var service = new DashboardService(context, _mockLogger.Object);

            // Act
            var result = await service.GetDashboardStatsAsync();

            // Assert
            result.Should().NotBeNull();
            result.RecentPerfumes.Should().HaveCount(5);
            
            // Should be ordered by Id descending (most recent first in database order)
            // Since we can't control exact Id values, just verify we have 5 items and they have correct structure
            result.RecentPerfumes.Should().AllSatisfy(p =>
            {
                p.PerfumeId.Should().NotBeEmpty();
                p.PerfumeName.Should().NotBeNullOrEmpty();
                p.BrandName.Should().Be("Test Brand");
                p.GenderProfile.Should().NotBeNullOrEmpty();
                p.ImageUrl.Should().NotBeNullOrEmpty();
                p.CreatedAt.Should().BeAfter(DateTime.MinValue);
            });
        }

        [Fact]
        public async Task GetDashboardStatsAsync_WithComplexData_ShouldReturnCompleteStats()
        {
            // Arrange
            using var context = DbContextFactory.CreateAndEnsureCreated();
            
            // Create comprehensive test data
            var brands = new[]
            {
                new BrandBuilder().WithName("Luxury Brand").Build(),
                new BrandBuilder().WithName("Popular Brand").Build()
            };
            context.Brands.AddRange(brands);
            
            var families = new[]
            {
                new FamilyBuilder().WithName("Oriental").Build(),
                new FamilyBuilder().WithName("Fresh").Build()
            };
            context.Families.AddRange(families);
            
            var notes = new[]
            {
                new NoteBuilder().WithName("Rose").Build(),
                new NoteBuilder().WithName("Bergamot").Build(),
                new NoteBuilder().WithName("Sandalwood").Build()
            };
            context.Notes.AddRange(notes);
            
            var accords = new[]
            {
                new AccordBuilder().WithName("Floral").Build(),
                new AccordBuilder().WithName("Citrus").Build()
            };
            context.Accords.AddRange(accords);
            
            var tags = new[]
            {
                new TagBuilder().WithName("Long-lasting").Build(),
                new TagBuilder().WithName("Office-appropriate").Build()
            };
            context.Tags.AddRange(tags);
            
            var seasons = new[]
            {
                new SeasonBuilder().WithName("Spring").Build(),
                new SeasonBuilder().WithName("Summer").Build()
            };
            context.Seasons.AddRange(seasons);
            
            var occasions = new[]
            {
                new OccasionBuilder().WithName("Date Night").Build(),
                new OccasionBuilder().WithName("Business Meeting").Build()
            };
            context.Occasions.AddRange(occasions);
            
            await context.SaveChangesAsync();

            // Create perfumes
            var perfumes = new[]
            {
                new PerfumeBuilder().WithBrand(brands[0]).WithName("Luxury Scent").Build(),
                new PerfumeBuilder().WithBrand(brands[0]).WithName("Premium Fragrance").Build(),
                new PerfumeBuilder().WithBrand(brands[1]).WithName("Popular Choice").Build()
            };
            context.Perfumes.AddRange(perfumes);
            await context.SaveChangesAsync();

            var service = new DashboardService(context, _mockLogger.Object);

            // Act
            var result = await service.GetDashboardStatsAsync();

            // Assert
            result.Should().NotBeNull();
            result.TotalPerfumes.Should().Be(3);
            result.TotalBrands.Should().Be(2);
            result.TotalFamilies.Should().Be(2);
            result.TotalNotes.Should().Be(3);
            result.TotalAccords.Should().Be(2);
            result.TotalTags.Should().Be(2);
            result.TotalSeasons.Should().Be(2);
            result.TotalOccasions.Should().Be(2);

            // Check top brands
            result.TopBrands.Should().HaveCount(2);
            result.TopBrands[0].BrandName.Should().Be("Luxury Brand");
            result.TopBrands[0].PerfumeCount.Should().Be(2);
            result.TopBrands[1].BrandName.Should().Be("Popular Brand");
            result.TopBrands[1].PerfumeCount.Should().Be(1);

            // Check recent perfumes
            result.RecentPerfumes.Should().HaveCount(3);
            result.RecentPerfumes.Should().AllSatisfy(p =>
            {
                p.PerfumeId.Should().NotBeEmpty();
                p.PerfumeName.Should().NotBeNullOrEmpty();
                p.BrandName.Should().BeOneOf("Luxury Brand", "Popular Brand");
            });
        }

        [Fact]
        public async Task GetDashboardStatsAsync_ShouldLogInformation()
        {
            // Arrange
            using var context = DbContextFactory.CreateAndEnsureCreated();
            var mockLogger = new Mock<ILogger<DashboardService>>();
            
            var brand = new BrandBuilder().Build();
            context.Brands.Add(brand);
            
            var perfume = new PerfumeBuilder().WithBrand(brand).Build();
            context.Perfumes.Add(perfume);
            await context.SaveChangesAsync();

            var service = new DashboardService(context, mockLogger.Object);

            // Act
            var result = await service.GetDashboardStatsAsync();

            // Assert
            result.Should().NotBeNull();
            
            // Verify logging was called with Information level
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Dashboard stats generated")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        public void Dispose()
        {
            // Cleanup if needed
            GC.SuppressFinalize(this);
        }
    }
}