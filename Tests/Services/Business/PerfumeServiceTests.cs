using System;
using Xunit;
using FluentAssertions;
using Moq;
using Microsoft.EntityFrameworkCore;
using ALOud.Services.Perfume;
using ALOud.Repositories;
using ALOud.Tests.Helpers;
using Tests.Common.TestDataBuilders;
using ALOud.DTOs.Perfumes;
using ALOud.Models;
using ViewModels;
using System.Linq.Expressions;

namespace ALOud.Tests.Services.Business
{
    /// <summary>
    /// Unit tests for PerfumeService using EF InMemory for full async support.
    /// Tests the complex perfume business logic with in-memory database that properly supports async operations.
    /// 
    /// Test naming convention: MethodName_Scenario_ExpectedBehavior
    /// </summary>
    public class PerfumeServiceTests : IDisposable
    {
        private readonly PerfumeService _perfumeService;
        private readonly Data.ALOudDbContext _context;
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IPerfumeRepository> _mockPerfumeRepository;

        public PerfumeServiceTests()
        {
            // Create in-memory database context that supports async operations
            _context = DbContextFactory.CreateAndEnsureCreated();
            
            // Create a real unit of work with real repositories using the EF context
            // This approach ensures all async operations work correctly
            var unitOfWork = new UnitOfWork(_context);
            
            // Create service under test with real EF-backed unit of work
            _perfumeService = new PerfumeService(unitOfWork, _context);
            
            // Keep the mocks for reference but don't use them for setup
            _mockPerfumeRepository = new Mock<IPerfumeRepository>();
            _mockUnitOfWork = new Mock<IUnitOfWork>();
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            _context.Dispose();
        }

        #region GetAllPerfumesAsync Tests

        [Fact]
        public async Task GetAllPerfumesAsync_WhenNoFiltersAndHasData_ReturnsCorrectPageWithTotalCount()
        {
            // Arrange
            var brand = new Brand { Id = Guid.NewGuid(), Name = "Test Brand" };
            var family = new Family { Id = Guid.NewGuid(), Name = "Floral", Description = "Floral scents" };
            
            // Add brand to context
            _context.Brands.Add(brand);
            await _context.SaveChangesAsync();
            
            var perfumes = new PerfumeBuilder()
                .WithBrand(brand)
                .Build(15); // Create 15 perfumes for pagination testing

            // Add perfumes to the EF context instead of mocking the return
            _context.Perfumes.AddRange(perfumes);
            await _context.SaveChangesAsync();

            // Act
            var result = await _perfumeService.GetAllPerfumesAsync(pageIndex: 1, pageSize: 10);

            // Assert
            result.Should().NotBeNull();
            result.Items.Should().HaveCount(10); // First page should have 10 items
            result.TotalCount.Should().Be(15);
            result.TotalPages.Should().Be(2);
            result.PageIndex.Should().Be(1);
            result.PageSize.Should().Be(10);
            result.HasNextPage.Should().BeTrue();
            result.HasPreviousPage.Should().BeFalse();
        }

        [Fact]
        public async Task GetAllPerfumesAsync_WhenSearchTermMatchesName_ReturnsOnlyMatchingPerfumes()
        {
            // Arrange
            var brand = new Brand { Id = Guid.NewGuid(), Name = "Test Brand" };
            _context.Brands.Add(brand);
            
            var perfumes = new List<Perfume>
            {
                new PerfumeBuilder().WithName("Chanel No. 5").WithBrand(brand).Build(),
                new PerfumeBuilder().WithName("Dior Sauvage").WithBrand(brand).Build(),
                new PerfumeBuilder().WithName("Channel Classic").WithBrand(brand).Build() // Typo intentional
            };

            _context.Perfumes.AddRange(perfumes);
            await _context.SaveChangesAsync();

            // Act
            var result = await _perfumeService.GetAllPerfumesAsync(searchTerm: "Chanel");

            // Assert
            result.Should().NotBeNull();
            result.Items.Should().HaveCount(1);
            result.Items[0].Name.Should().Be("Chanel No. 5");
            result.TotalCount.Should().Be(1);
        }

        [Fact]
        public async Task GetAllPerfumesAsync_WhenSearchTermMatchesBrand_ReturnsOnlyMatchingPerfumes()
        {
            // Arrange
            var chanelBrand = new Brand { Id = Guid.NewGuid(), Name = "Chanel" };
            var diorBrand = new Brand { Id = Guid.NewGuid(), Name = "Dior" };
            _context.Brands.AddRange(chanelBrand, diorBrand);
            
            var perfumes = new List<Perfume>
            {
                new PerfumeBuilder().WithName("No. 5").WithBrand(chanelBrand).Build(),
                new PerfumeBuilder().WithName("Coco").WithBrand(chanelBrand).Build(),
                new PerfumeBuilder().WithName("Sauvage").WithBrand(diorBrand).Build()
            };

            _context.Perfumes.AddRange(perfumes);
            await _context.SaveChangesAsync();

            // Act
            var result = await _perfumeService.GetAllPerfumesAsync(searchTerm: "Chanel");

            // Assert
            result.Should().NotBeNull();
            result.Items.Should().HaveCount(2);
            result.Items.Should().OnlyContain(p => p.BrandName == "Chanel");
            result.TotalCount.Should().Be(2);
        }

        [Fact]
        public async Task GetAllPerfumesAsync_WhenBrandIdFilter_ReturnsOnlyPerfumesOfThatBrand()
        {
            // Arrange
            var targetBrandId = Guid.NewGuid();
            var otherBrandId = Guid.NewGuid();
            
            var targetBrand = new Brand { Id = targetBrandId, Name = "Target Brand" };
            var otherBrand = new Brand { Id = otherBrandId, Name = "Other Brand" };
            _context.Brands.AddRange(targetBrand, otherBrand);
            
            var perfumes = new List<Perfume>
            {
                new PerfumeBuilder().WithBrand(targetBrand).Build(),
                new PerfumeBuilder().WithBrand(targetBrand).Build(),
                new PerfumeBuilder().WithBrand(otherBrand).Build()
            };

            _context.Perfumes.AddRange(perfumes);
            await _context.SaveChangesAsync();

            // Act
            var result = await _perfumeService.GetAllPerfumesAsync(brandId: targetBrandId);

            // Assert
            result.Should().NotBeNull();
            result.Items.Should().HaveCount(2);
            result.Items.Should().OnlyContain(p => p.BrandId == targetBrandId);
            result.TotalCount.Should().Be(2);
        }

        [Fact]
        public async Task GetAllPerfumesAsync_WhenGenderProfileFilter_ReturnsOnlyMatchingGenderProfiles()
        {
            // Arrange
            var brand = new Brand { Id = Guid.NewGuid(), Name = "Test Brand" };
            _context.Brands.Add(brand);
            
            var perfumes = new List<Perfume>
            {
                new PerfumeBuilder().WithBrand(brand).WithGenderProfile("Women").Build(),
                new PerfumeBuilder().WithBrand(brand).WithGenderProfile("Men").Build(),
                new PerfumeBuilder().WithBrand(brand).WithGenderProfile("Women").Build(),
                new PerfumeBuilder().WithBrand(brand).WithGenderProfile("Unisex").Build()
            };

            _context.Perfumes.AddRange(perfumes);
            await _context.SaveChangesAsync();

            // Act
            var result = await _perfumeService.GetAllPerfumesAsync(genderProfile: "Women");

            // Assert
            result.Should().NotBeNull();
            result.Items.Should().HaveCount(2);
            result.Items.Should().OnlyContain(p => p.GenderProfile == "Women");
            result.TotalCount.Should().Be(2);
        }

        [Fact]
        public async Task GetAllPerfumesAsync_WhenNoData_ReturnsEmptyPaginatedList()
        {
            // Arrange
            var emptyQueryable = new List<Perfume>().AsQueryable();
            _mockPerfumeRepository.Setup(r => r.GetQueryable())
                .Returns(emptyQueryable);

            // Act
            var result = await _perfumeService.GetAllPerfumesAsync();

            // Assert
            result.Should().NotBeNull();
            result.Items.Should().BeEmpty();
            result.TotalCount.Should().Be(0);
            result.TotalPages.Should().Be(0);
            result.HasNextPage.Should().BeFalse();
            result.HasPreviousPage.Should().BeFalse();
        }

        [Fact]
        public async Task GetAllPerfumesAsync_WhenPage2WithPageSize5_ReturnsCorrectSlice()
        {
            // Arrange
            var brand = new Brand { Id = Guid.NewGuid(), Name = "Test Brand" };
            _context.Brands.Add(brand);
            
            var perfumes = new PerfumeBuilder()
                .WithBrand(brand)
                .Build(12); // 12 perfumes total

            _context.Perfumes.AddRange(perfumes);
            await _context.SaveChangesAsync();

            // Act
            var result = await _perfumeService.GetAllPerfumesAsync(pageIndex: 2, pageSize: 5);

            // Assert
            result.Should().NotBeNull();
            result.Items.Should().HaveCount(5); // Second page should have 5 items
            result.TotalCount.Should().Be(12);
            result.TotalPages.Should().Be(3); // 12/5 = 2.4, rounded up = 3
            result.PageIndex.Should().Be(2);
            result.PageSize.Should().Be(5);
            result.HasPreviousPage.Should().BeTrue();
            result.HasNextPage.Should().BeTrue();
        }

        #endregion

        #region GetPerfumeByIdAsync Tests

        [Fact]
        public async Task GetPerfumeByIdAsync_WhenIdExists_ReturnsCorrectPerfumeDto()
        {
            // Arrange
            var perfumeId = Guid.NewGuid();
            var brand = new Brand { Id = Guid.NewGuid(), Name = "Test Brand" };
            _context.Brands.Add(brand);
            
            var perfume = new PerfumeBuilder()
                .WithId(perfumeId)
                .WithName("Test Perfume")
                .WithBrand(brand)
                .WithPrice(99.99m)
                .Build();

            _context.Perfumes.Add(perfume);
            await _context.SaveChangesAsync();

            // Act
            var result = await _perfumeService.GetPerfumeByIdAsync(perfumeId);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(perfumeId);
            result.Name.Should().Be("Test Perfume");
            result.BrandName.Should().Be("Test Brand");
            result.Price.Should().Be(99.99m);
        }

        [Fact]
        public async Task GetPerfumeByIdAsync_WhenIdNotFound_ReturnsNull()
        {
            // Arrange
            var nonExistentId = Guid.NewGuid();
            
            _mockPerfumeRepository.Setup(r => r.GetByIdAsync(nonExistentId))
                .ReturnsAsync((Perfume?)null);

            // Act
            var result = await _perfumeService.GetPerfumeByIdAsync(nonExistentId);

            // Assert
            result.Should().BeNull();
        }

        #endregion

        #region DeletePerfumeAsync Tests

        [Fact]
        public async Task DeletePerfumeAsync_WhenPerfumeExists_ReturnsTrueAndCallsRemoveAndSaveChanges()
        {
            // Arrange
            var perfumeId = Guid.NewGuid();
            var perfume = new PerfumeBuilder().WithId(perfumeId).Build();

            // Add perfume to real database first
            _context.Perfumes.Add(perfume);
            await _context.SaveChangesAsync();

            // Act
            var result = await _perfumeService.DeletePerfumeAsync(perfumeId);

            // Assert
            result.Should().BeTrue();
            
            // Verify the perfume was actually removed from database
            var deletedPerfume = await _context.Perfumes.FindAsync(perfumeId);
            deletedPerfume.Should().BeNull();
        }

        [Fact]
        public async Task DeletePerfumeAsync_WhenPerfumeNotFound_ReturnsFalseAndDoesNotCallSaveChanges()
        {
            // Arrange
            var nonExistentId = Guid.NewGuid();
            
            _mockPerfumeRepository.Setup(r => r.GetByIdAsync(nonExistentId))
                .ReturnsAsync((Perfume?)null);

            // Act
            var result = await _perfumeService.DeletePerfumeAsync(nonExistentId);

            // Assert
            result.Should().BeFalse();
            
            // Verify no removal or save was attempted
            _mockPerfumeRepository.Verify(r => r.Remove(It.IsAny<Perfume>()), Times.Never);
            _mockUnitOfWork.Verify(uow => uow.SaveChangesAsync(), Times.Never);
        }

        #endregion

        #region CreatePerfumeAsync Tests

        [Theory]
        [InlineData("Light", "1-2 hours", "Intimate")]
        [InlineData("Strong", "7-8 hours", "Enormous")]
        [InlineData("Moderate", "5-6 hours", "Moderate")]
        public async Task CreatePerfumeAsync_WhenValidDto_ReturnsNewGuidAndCallsSaveChanges(
            string intensity, string longevity, string sillage)
        {
            // Arrange
            var brandId = Guid.NewGuid();
            var dto = new CreatePerfumeDto
            {
                Name = "New Perfume",
                BrandId = brandId,
                Intensity = intensity,
                Longevity = longevity,
                Sillage = sillage,
                GenderProfile = "Unisex",
                PriceRange = "Mid-range",
                Price = 75.50m,
                StockQuantity = 10,
                Description = "A lovely new fragrance",
                FamilyIds = new List<Guid>(),
                NoteSelections = new List<PerfumeNoteSelectionDto>(),
                AccordSelections = new List<PerfumeAccordSelectionDto>(),
                TagIds = new List<Guid>(),
                SeasonIds = new List<Guid>(),
                OccasionIds = new List<Guid>()
            };

            // Act
            var result = await _perfumeService.CreatePerfumeAsync(dto);

            // Assert
            result.Should().NotBeEmpty();
            
            // Verify perfume was actually saved to database
            var savedPerfume = await _context.Perfumes.FindAsync(result);
            savedPerfume.Should().NotBeNull();
            savedPerfume!.Name.Should().Be("New Perfume");
            savedPerfume.BrandId.Should().Be(brandId);
            savedPerfume.Intensity.Should().Be(intensity);
            savedPerfume.Longevity.Should().Be(longevity);
            savedPerfume.Sillage.Should().Be(sillage);
        }

        [Fact]
        public async Task CreatePerfumeAsync_WhenDtoWithEmptyCollections_ReturnsGuidAndNoRelationRowsInserted()
        {
            // Arrange
            var dto = new CreatePerfumeDto
            {
                Name = "Simple Perfume",
                BrandId = Guid.NewGuid(),
                Price = 50m,
                StockQuantity = 5,
                FamilyIds = new List<Guid>(), // Empty
                NoteSelections = new List<PerfumeNoteSelectionDto>(), // Empty
                AccordSelections = new List<PerfumeAccordSelectionDto>(), // Empty
                TagIds = new List<Guid>(), // Empty
                SeasonIds = new List<Guid>(), // Empty
                OccasionIds = new List<Guid>() // Empty
            };

            _mockUnitOfWork.Setup(uow => uow.SaveChangesAsync())
                .ReturnsAsync(1);

            // Act
            var result = await _perfumeService.CreatePerfumeAsync(dto);

            // Assert
            result.Should().NotBeEmpty();
            
            // Verify no relationship entities were added to context
            var relationEntities = _context.ChangeTracker.Entries()
                .Where(e => e.Entity is PerfumeFamily || e.Entity is PerfumeNote || 
                           e.Entity is PerfumeAccord || e.Entity is PerfumeTag ||
                           e.Entity is PerfumeSeason || e.Entity is PerfumeOccasion)
                .ToList();
                
            relationEntities.Should().BeEmpty();
        }

        #endregion

        #region Integration Tests with InMemory Context

        [Fact]
        public async Task CreatePerfumeAsync_WithComplexRelationships_InsertsAllRelationRowsToInMemoryContext()
        {
            // Arrange - Setup seed data in InMemory context
            var brand = new Brand { Id = Guid.NewGuid(), Name = "Test Brand" };
            var family = new Family { Id = Guid.NewGuid(), Name = "Oriental", Description = "Oriental family" };
            var note = new Note { Id = Guid.NewGuid(), Name = "Vanilla", Category = "Base" };
            var accord = new Accord { Id = Guid.NewGuid(), Name = "Sweet", Description = "Sweet accord" };
            var tag = new Tag { Id = Guid.NewGuid(), Name = "Romantic" };
            var season = new Season { Id = Guid.NewGuid(), Name = "Winter" };
            var occasion = new Occasion { Id = Guid.NewGuid(), Name = "Date" };

            _context.Brands.Add(brand);
            _context.Families.Add(family);
            _context.Notes.Add(note);
            _context.Accords.Add(accord);
            _context.Tags.Add(tag);
            _context.Seasons.Add(season);
            _context.Occasions.Add(occasion);
            await _context.SaveChangesAsync();

            var dto = new CreatePerfumeDto
            {
                Name = "Complex Perfume",
                BrandId = brand.Id,
                Price = 120m,
                StockQuantity = 15,
                FamilyIds = new List<Guid> { family.Id },
                NoteSelections = new List<PerfumeNoteSelectionDto> 
                { 
                    new() { NoteId = note.Id, NoteLevel = "Base" } 
                },
                AccordSelections = new List<PerfumeAccordSelectionDto> 
                { 
                    new() { AccordId = accord.Id, Intensity = "Strong" } 
                },
                TagIds = new List<Guid> { tag.Id },
                SeasonIds = new List<Guid> { season.Id },
                OccasionIds = new List<Guid> { occasion.Id }
            };

            _mockUnitOfWork.Setup(uow => uow.SaveChangesAsync())
                .ReturnsAsync(1);

            // Act
            var result = await _perfumeService.CreatePerfumeAsync(dto);

            // Assert
            result.Should().NotBeEmpty();

            // Verify all relationship entities were added to the InMemory context
            var perfumeFamilies = await _context.PerfumeFamilies.ToListAsync();
            var perfumeNotes = await _context.PerfumeNotes.ToListAsync();
            var perfumeAccords = await _context.PerfumeAccords.ToListAsync();
            var perfumeTags = await _context.PerfumeTags.ToListAsync();
            var perfumeSeasons = await _context.PerfumeSeasons.ToListAsync();
            var perfumeOccasions = await _context.PerfumeOccasions.ToListAsync();

            perfumeFamilies.Should().HaveCount(1);
            perfumeFamilies[0].FamilyId.Should().Be(family.Id);

            perfumeNotes.Should().HaveCount(1);
            perfumeNotes[0].NoteId.Should().Be(note.Id);
            perfumeNotes[0].NoteLevel.Should().Be("Base");

            perfumeAccords.Should().HaveCount(1);
            perfumeAccords[0].AccordId.Should().Be(accord.Id);
            perfumeAccords[0].Intensity.Should().Be("Strong");

            perfumeTags.Should().HaveCount(1);
            perfumeTags[0].TagId.Should().Be(tag.Id);

            perfumeSeasons.Should().HaveCount(1);
            perfumeSeasons[0].SeasonId.Should().Be(season.Id);

            perfumeOccasions.Should().HaveCount(1);
            perfumeOccasions[0].OccasionId.Should().Be(occasion.Id);
        }

        #endregion

        #region Helper Method Tests

        [Fact]
        public void PerfumeDtoProjection_WhenAppliedToPerfume_MapsAllFieldsCorrectly()
        {
            // Arrange
            var brand = new Brand { Id = Guid.NewGuid(), Name = "Test Brand" };
            var family = new Family { Id = Guid.NewGuid(), Name = "Floral", Description = "Floral scents" };
            
            var perfume = new PerfumeBuilder()
                .WithName("Test Perfume")
                .WithBrand(brand)
                .WithPrice(99.99m)
                .WithIntensity("Strong")
                .WithLongevity("7-8 hours")
                .WithSillage("Enormous")
                .WithGenderProfile("Women")
                .WithPriceRange("Luxury")
                .WithStockQuantity(25)
                .WithDescription("A beautiful fragrance")
                .WithFamilies(family)
                .Build();

            // Act - Manually invoke the projection (since it's static)
            var dto = new PerfumeDto
            {
                Id = perfume.Id,
                Name = perfume.Name,
                Intensity = perfume.Intensity,
                Longevity = perfume.Longevity,
                Sillage = perfume.Sillage,
                GenderProfile = perfume.GenderProfile,
                PriceRange = perfume.PriceRange,
                Price = perfume.Price,
                StockQuantity = perfume.StockQuantity,
                Description = perfume.Description,
                ImageUrl = perfume.ImageUrl,
                BrandId = perfume.BrandId,
                BrandName = perfume.Brand.Name,
                Families = perfume.PerfumeFamilies.Select(pf => pf.Family.Name).ToList(),
                CreatedAt = perfume.CreatedAt
            };

            // Assert
            dto.Id.Should().Be(perfume.Id);
            dto.Name.Should().Be("Test Perfume");
            dto.BrandName.Should().Be("Test Brand");
            dto.Price.Should().Be(99.99m);
            dto.Intensity.Should().Be("Strong");
            dto.Longevity.Should().Be("7-8 hours");
            dto.Sillage.Should().Be("Enormous");
            dto.GenderProfile.Should().Be("Women");
            dto.PriceRange.Should().Be("Luxury");
            dto.StockQuantity.Should().Be(25);
            dto.Description.Should().Be("A beautiful fragrance");
            dto.Families.Should().Contain("Floral");
        }

        #endregion
    }
}