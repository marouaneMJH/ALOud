using System;
using Xunit;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using ALOud.Services.Family;
using ALOud.DTOs.Families;
using Tests.Common.TestDataBuilders;
using ALOud.Tests.Helpers;
using ALOud.Data;

namespace Tests.Services.Business
{
    public class FamilyServiceTests : IDisposable
    {
        private readonly ALOudDbContext _context;
        private readonly FamilyService _familyService;

        public FamilyServiceTests()
        {
            _context = DbContextFactory.CreateAndEnsureCreated();
            _familyService = new FamilyService(_context);
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            _context.Dispose();
        }

        #region GetAllFamiliesAsync Tests

        [Fact]
        public async Task GetAllFamiliesAsync_WithDefaultParameters_ShouldReturnPaginatedResults()
        {
            // Arrange
            var families = FamilyBuilder.CreateValidList(15);
            _context.Families.AddRange(families);
            await _context.SaveChangesAsync();

            // Act
            var result = await _familyService.GetAllFamiliesAsync();

            // Assert
            result.Should().NotBeNull();
            result.Items.Should().HaveCount(10); // Default page size
            result.PageIndex.Should().Be(1);
            result.PageSize.Should().Be(10);
            result.TotalCount.Should().Be(15);
            result.TotalPages.Should().Be(2);
            result.Items.Should().BeInAscendingOrder(f => f.Name);
        }

        [Fact]
        public async Task GetAllFamiliesAsync_WithCustomPagination_ShouldReturnCorrectPage()
        {
            // Arrange
            var families = FamilyBuilder.CreateValidList(25);
            _context.Families.AddRange(families);
            await _context.SaveChangesAsync();

            // Act
            var result = await _familyService.GetAllFamiliesAsync(pageIndex: 2, pageSize: 5);

            // Assert
            result.Should().NotBeNull();
            result.Items.Should().HaveCount(5);
            result.PageIndex.Should().Be(2);
            result.PageSize.Should().Be(5);
            result.TotalCount.Should().Be(25);
            result.TotalPages.Should().Be(5);
        }

        [Fact]
        public async Task GetAllFamiliesAsync_WithSearchTerm_ShouldReturnFilteredResults()
        {
            // Arrange
            var searchFamily = FamilyBuilder.CreateWithName("Oriental Woody");
            var families = new List<ALOud.Models.Family> { searchFamily };
            families.AddRange(FamilyBuilder.CreateValidList(10));
            _context.Families.AddRange(families);
            await _context.SaveChangesAsync();

            // Act
            var result = await _familyService.GetAllFamiliesAsync(searchTerm: "Oriental");

            // Assert
            result.Should().NotBeNull();
            result.Items.Should().HaveCount(1);
            result.Items.First().Name.Should().Contain("Oriental");
            result.TotalCount.Should().Be(1);
        }

        [Fact]
        public async Task GetAllFamiliesAsync_WithNonMatchingSearchTerm_ShouldReturnEmptyResults()
        {
            // Arrange
            var families = FamilyBuilder.CreateValidList(5);
            _context.Families.AddRange(families);
            await _context.SaveChangesAsync();

            // Act
            var result = await _familyService.GetAllFamiliesAsync(searchTerm: "NonExistentFamily");

            // Assert
            result.Should().NotBeNull();
            result.Items.Should().BeEmpty();
            result.TotalCount.Should().Be(0);
        }

        [Fact]
        public async Task GetAllFamiliesAsync_WhenEmpty_ShouldReturnEmptyPaginatedList()
        {
            // Act
            var result = await _familyService.GetAllFamiliesAsync();

            // Assert
            result.Should().NotBeNull();
            result.Items.Should().BeEmpty();
            result.TotalCount.Should().Be(0);
            result.TotalPages.Should().Be(0);
        }

        #endregion

        #region GetAllFamiliesForSelectAsync Tests

        [Fact]
        public async Task GetAllFamiliesForSelectAsync_ShouldReturnAllFamiliesOrderedByName()
        {
            // Arrange
            var families = new List<ALOud.Models.Family>
            {
                FamilyBuilder.CreateWithName("Woody"),
                FamilyBuilder.CreateWithName("Floral"),
                FamilyBuilder.CreateWithName("Oriental")
            };
            _context.Families.AddRange(families);
            await _context.SaveChangesAsync();

            // Act
            var result = await _familyService.GetAllFamiliesForSelectAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(3);
            result.Should().BeInAscendingOrder(f => f.Name);
            result.All(f => !string.IsNullOrEmpty(f.Name)).Should().BeTrue();
            result.All(f => f.Id != Guid.Empty).Should().BeTrue();
        }

        [Fact]
        public async Task GetAllFamiliesForSelectAsync_WhenEmpty_ShouldReturnEmptyList()
        {
            // Act
            var result = await _familyService.GetAllFamiliesForSelectAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        #endregion

        #region GetFamilyByIdAsync Tests

        [Fact]
        public async Task GetFamilyByIdAsync_WithValidId_ShouldReturnFamily()
        {
            // Arrange
            var family = FamilyBuilder.CreateValid();
            _context.Families.Add(family);
            await _context.SaveChangesAsync();

            // Act
            var result = await _familyService.GetFamilyByIdAsync(family.Id);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(family.Id);
            result.Name.Should().Be(family.Name);
            result.Description.Should().Be(family.Description);
            result.PerfumeCount.Should().Be(0); // No perfume families seeded
        }

        [Fact]
        public async Task GetFamilyByIdAsync_WithInvalidId_ShouldReturnNull()
        {
            // Arrange
            var nonExistentId = Guid.NewGuid();

            // Act
            var result = await _familyService.GetFamilyByIdAsync(nonExistentId);

            // Assert
            result.Should().BeNull();
        }

        #endregion

        #region GetFamilyForEditAsync Tests

        [Fact]
        public async Task GetFamilyForEditAsync_WithValidId_ShouldReturnUpdateDto()
        {
            // Arrange
            var family = FamilyBuilder.CreateValid();
            _context.Families.Add(family);
            await _context.SaveChangesAsync();

            // Act
            var result = await _familyService.GetFamilyForEditAsync(family.Id);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(family.Id);
            result.Name.Should().Be(family.Name);
            result.Description.Should().Be(family.Description);
        }

        [Fact]
        public async Task GetFamilyForEditAsync_WithInvalidId_ShouldReturnNull()
        {
            // Arrange
            var nonExistentId = Guid.NewGuid();

            // Act
            var result = await _familyService.GetFamilyForEditAsync(nonExistentId);

            // Assert
            result.Should().BeNull();
        }

        #endregion

        #region CreateFamilyAsync Tests

        [Fact]
        public async Task CreateFamilyAsync_WithValidDto_ShouldCreateFamilyAndReturnId()
        {
            // Arrange
            var createDto = CreateFamilyDtoBuilder.CreateValid();

            // Act
            var result = await _familyService.CreateFamilyAsync(createDto);

            // Assert
            result.Should().NotBe(Guid.Empty);

            // Verify family was created in database
            var createdFamily = await _context.Families.FindAsync(result);
            createdFamily.Should().NotBeNull();
            createdFamily!.Name.Should().Be(createDto.Name);
            createdFamily.Description.Should().Be(createDto.Description);
        }

        [Fact]
        public async Task CreateFamilyAsync_WithoutDescription_ShouldCreateFamily()
        {
            // Arrange
            var createDto = new CreateFamilyDtoBuilder().WithoutDescription().Build();

            // Act
            var result = await _familyService.CreateFamilyAsync(createDto);

            // Assert
            result.Should().NotBe(Guid.Empty);

            // Verify family was created with null description
            var createdFamily = await _context.Families.FindAsync(result);
            createdFamily.Should().NotBeNull();
            createdFamily!.Description.Should().BeNull();
        }

        #endregion

        #region UpdateFamilyAsync Tests

        [Fact]
        public async Task UpdateFamilyAsync_WithValidDto_ShouldUpdateFamilyAndReturnTrue()
        {
            // Arrange
            var family = FamilyBuilder.CreateValid();
            _context.Families.Add(family);
            await _context.SaveChangesAsync();

            var updateDto = UpdateFamilyDtoBuilder.CreateWithId(family.Id);
            updateDto.Name = "Updated Family Name";
            updateDto.Description = "Updated description";

            // Act
            var result = await _familyService.UpdateFamilyAsync(updateDto);

            // Assert
            result.Should().BeTrue();

            // Verify family was updated in database
            var updatedFamily = await _context.Families.FindAsync(family.Id);
            updatedFamily.Should().NotBeNull();
            updatedFamily!.Name.Should().Be("Updated Family Name");
            updatedFamily.Description.Should().Be("Updated description");
        }

        [Fact]
        public async Task UpdateFamilyAsync_WithInvalidId_ShouldReturnFalse()
        {
            // Arrange
            var updateDto = UpdateFamilyDtoBuilder.CreateValid();
            updateDto.Id = Guid.NewGuid(); // Non-existent ID

            // Act
            var result = await _familyService.UpdateFamilyAsync(updateDto);

            // Assert
            result.Should().BeFalse();
        }

        #endregion

        #region DeleteFamilyAsync Tests

        [Fact]
        public async Task DeleteFamilyAsync_WithValidId_ShouldDeleteFamilyAndReturnTrue()
        {
            // Arrange
            var family = FamilyBuilder.CreateValid();
            _context.Families.Add(family);
            await _context.SaveChangesAsync();

            // Act
            var result = await _familyService.DeleteFamilyAsync(family.Id);

            // Assert
            result.Should().BeTrue();

            // Verify family was deleted from database
            var deletedFamily = await _context.Families.FindAsync(family.Id);
            deletedFamily.Should().BeNull();
        }

        [Fact]
        public async Task DeleteFamilyAsync_WithInvalidId_ShouldReturnFalse()
        {
            // Arrange
            var nonExistentId = Guid.NewGuid();

            // Act
            var result = await _familyService.DeleteFamilyAsync(nonExistentId);

            // Assert
            result.Should().BeFalse();
        }

        #endregion

        #region FamilyExistsAsync Tests

        [Fact]
        public async Task FamilyExistsAsync_WhenFamilyExists_ShouldReturnTrue()
        {
            // Arrange
            var family = FamilyBuilder.CreateWithName("Woody");
            _context.Families.Add(family);
            await _context.SaveChangesAsync();

            // Act
            var result = await _familyService.FamilyExistsAsync("Woody");

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task FamilyExistsAsync_WhenFamilyDoesNotExist_ShouldReturnFalse()
        {
            // Arrange
            var family = FamilyBuilder.CreateWithName("Woody");
            _context.Families.Add(family);
            await _context.SaveChangesAsync();

            // Act
            var result = await _familyService.FamilyExistsAsync("NonExistent");

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task FamilyExistsAsync_WithExcludeId_ShouldIgnoreSpecifiedFamily()
        {
            // Arrange
            var family = FamilyBuilder.CreateWithName("Woody");
            _context.Families.Add(family);
            await _context.SaveChangesAsync();

            // Act
            var result = await _familyService.FamilyExistsAsync("Woody", family.Id);

            // Assert
            result.Should().BeFalse(); // Should return false because we're excluding this family
        }

        [Fact]
        public async Task FamilyExistsAsync_WithExcludeIdButOtherExists_ShouldReturnTrue()
        {
            // Arrange
            var family1 = FamilyBuilder.CreateWithName("Woody");
            var family2 = FamilyBuilder.CreateWithName("Woody"); // Same name, different ID
            _context.Families.AddRange(new[] { family1, family2 });
            await _context.SaveChangesAsync();

            // Act
            var result = await _familyService.FamilyExistsAsync("Woody", family1.Id);

            // Assert
            result.Should().BeTrue(); // Should return true because family2 still exists
        }

        #endregion

        #region Integration Tests

        [Fact]
        public async Task CompleteWorkflow_CreateUpdateDelete_ShouldWorkCorrectly()
        {
            // Arrange
            var createDto = CreateFamilyDtoBuilder.CreateWithName("Test Family");

            // Act & Assert - Create
            var createdId = await _familyService.CreateFamilyAsync(createDto);
            createdId.Should().NotBe(Guid.Empty);

            // Act & Assert - Read
            var family = await _familyService.GetFamilyByIdAsync(createdId);
            family.Should().NotBeNull();
            family!.Name.Should().Be("Test Family");

            // Act & Assert - Update
            var updateDto = UpdateFamilyDtoBuilder.CreateWithId(createdId);
            updateDto.Name = "Updated Test Family";
            var updateResult = await _familyService.UpdateFamilyAsync(updateDto);
            updateResult.Should().BeTrue();

            // Verify update
            var updatedFamily = await _familyService.GetFamilyByIdAsync(createdId);
            updatedFamily!.Name.Should().Be("Updated Test Family");

            // Act & Assert - Delete
            var deleteResult = await _familyService.DeleteFamilyAsync(createdId);
            deleteResult.Should().BeTrue();

            // Verify deletion
            var deletedFamily = await _familyService.GetFamilyByIdAsync(createdId);
            deletedFamily.Should().BeNull();
        }

        #endregion
    }
}