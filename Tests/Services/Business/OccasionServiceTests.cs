using System;
using Xunit;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using ALOud.Services.Occasion;
using ALOud.DTOs.Occasions;
using Tests.Common.TestDataBuilders;
using ALOud.Tests.Helpers;
using ALOud.Data;

namespace Tests.Services.Business
{
    public class OccasionServiceTests : IDisposable
    {
        private readonly ALOudDbContext _context;
        private readonly OccasionService _occasionService;

        public OccasionServiceTests()
        {
            _context = DbContextFactory.CreateAndEnsureCreated();
            _occasionService = new OccasionService(_context);
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            _context.Dispose();
        }

        #region GetAllOccasionsAsync Tests

        [Fact]
        public async Task GetAllOccasionsAsync_WithDefaultParameters_ShouldReturnPaginatedResults()
        {
            // Arrange
            var occasions = OccasionBuilder.CreateValidList(15);
            _context.Occasions.AddRange(occasions);
            await _context.SaveChangesAsync();

            // Act
            var result = await _occasionService.GetAllOccasionsAsync();

            // Assert
            result.Should().NotBeNull();
            result.Items.Should().HaveCount(10); // Default page size
            result.PageIndex.Should().Be(1);
            result.PageSize.Should().Be(10);
            result.TotalCount.Should().Be(15);
            result.TotalPages.Should().Be(2);
            result.Items.Should().BeInAscendingOrder(o => o.Name);
        }

        [Fact]
        public async Task GetAllOccasionsAsync_WithCustomPagination_ShouldReturnCorrectPage()
        {
            // Arrange
            var occasions = OccasionBuilder.CreateValidList(25);
            _context.Occasions.AddRange(occasions);
            await _context.SaveChangesAsync();

            // Act
            var result = await _occasionService.GetAllOccasionsAsync(pageIndex: 2, pageSize: 5);

            // Assert
            result.Should().NotBeNull();
            result.Items.Should().HaveCount(5);
            result.PageIndex.Should().Be(2);
            result.PageSize.Should().Be(5);
            result.TotalCount.Should().Be(25);
            result.TotalPages.Should().Be(5);
        }

        [Fact]
        public async Task GetAllOccasionsAsync_WithSearchTerm_ShouldReturnFilteredResults()
        {
            // Arrange
            var searchOccasion = OccasionBuilder.CreateWithName("Date Night Special");
            var occasions = new List<ALOud.Models.Occasion> { searchOccasion };
            occasions.AddRange(OccasionBuilder.CreateValidList(10));
            _context.Occasions.AddRange(occasions);
            await _context.SaveChangesAsync();

            // Act
            var result = await _occasionService.GetAllOccasionsAsync(searchTerm: "Date");

            // Assert
            result.Should().NotBeNull();
            result.Items.Should().HaveCount(1);
            result.Items.First().Name.Should().Contain("Date");
            result.TotalCount.Should().Be(1);
        }

        [Fact]
        public async Task GetAllOccasionsAsync_WithNonMatchingSearchTerm_ShouldReturnEmptyResults()
        {
            // Arrange
            var occasions = OccasionBuilder.CreateValidList(5);
            _context.Occasions.AddRange(occasions);
            await _context.SaveChangesAsync();

            // Act
            var result = await _occasionService.GetAllOccasionsAsync(searchTerm: "NonExistentOccasion");

            // Assert
            result.Should().NotBeNull();
            result.Items.Should().BeEmpty();
            result.TotalCount.Should().Be(0);
        }

        [Fact]
        public async Task GetAllOccasionsAsync_WhenEmpty_ShouldReturnEmptyPaginatedList()
        {
            // Act
            var result = await _occasionService.GetAllOccasionsAsync();

            // Assert
            result.Should().NotBeNull();
            result.Items.Should().BeEmpty();
            result.TotalCount.Should().Be(0);
            result.TotalPages.Should().Be(0);
        }

        #endregion

        #region GetAllOccasionsForSelectAsync Tests

        [Fact]
        public async Task GetAllOccasionsForSelectAsync_ShouldReturnAllOccasionsOrderedByName()
        {
            // Arrange
            var occasions = OccasionBuilder.CreateWithNames("Wedding", "Office", "Party", "Casual");
            _context.Occasions.AddRange(occasions);
            await _context.SaveChangesAsync();

            // Act
            var result = await _occasionService.GetAllOccasionsForSelectAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(4);
            result.Should().BeInAscendingOrder(o => o.Name);
            result.All(o => !string.IsNullOrEmpty(o.Name)).Should().BeTrue();
            result.All(o => o.Id != Guid.Empty).Should().BeTrue();
            
            // Verify alphabetical ordering
            result[0].Name.Should().Be("Casual");
            result[1].Name.Should().Be("Office");
            result[2].Name.Should().Be("Party");
            result[3].Name.Should().Be("Wedding");
        }

        [Fact]
        public async Task GetAllOccasionsForSelectAsync_WhenEmpty_ShouldReturnEmptyList()
        {
            // Act
            var result = await _occasionService.GetAllOccasionsForSelectAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        #endregion

        #region GetOccasionByIdAsync Tests

        [Fact]
        public async Task GetOccasionByIdAsync_WithValidId_ShouldReturnOccasion()
        {
            // Arrange
            var occasion = OccasionBuilder.CreateValid();
            _context.Occasions.AddRange(new[] { occasion });
            await _context.SaveChangesAsync();

            // Act
            var result = await _occasionService.GetOccasionByIdAsync(occasion.Id);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(occasion.Id);
            result.Name.Should().Be(occasion.Name);
            result.PerfumeCount.Should().Be(0); // No perfume occasions seeded
        }

        [Fact]
        public async Task GetOccasionByIdAsync_WithInvalidId_ShouldReturnNull()
        {
            // Arrange
            var nonExistentId = Guid.NewGuid();

            // Act
            var result = await _occasionService.GetOccasionByIdAsync(nonExistentId);

            // Assert
            result.Should().BeNull();
        }

        #endregion

        #region GetOccasionForEditAsync Tests

        [Fact]
        public async Task GetOccasionForEditAsync_WithValidId_ShouldReturnUpdateDto()
        {
            // Arrange
            var occasion = OccasionBuilder.CreateValid();
            _context.Occasions.AddRange(new[] { occasion });
            await _context.SaveChangesAsync();

            // Act
            var result = await _occasionService.GetOccasionForEditAsync(occasion.Id);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(occasion.Id);
            result.Name.Should().Be(occasion.Name);
        }

        [Fact]
        public async Task GetOccasionForEditAsync_WithInvalidId_ShouldReturnNull()
        {
            // Arrange
            var nonExistentId = Guid.NewGuid();

            // Act
            var result = await _occasionService.GetOccasionForEditAsync(nonExistentId);

            // Assert
            result.Should().BeNull();
        }

        #endregion

        #region CreateOccasionAsync Tests

        [Fact]
        public async Task CreateOccasionAsync_WithValidDto_ShouldCreateOccasionAndReturnId()
        {
            // Arrange
            var createDto = CreateOccasionDtoBuilder.CreateValid();

            // Act
            var result = await _occasionService.CreateOccasionAsync(createDto);

            // Assert
            result.Should().NotBe(Guid.Empty);

            // Verify occasion was created in database
            var createdOccasion = await _context.Occasions.FindAsync(result);
            createdOccasion.Should().NotBeNull();
            createdOccasion!.Name.Should().Be(createDto.Name);
        }

        [Fact]
        public async Task CreateOccasionAsync_WithSpecificOccasionName_ShouldCreateOccasion()
        {
            // Arrange
            var createDto = CreateOccasionDtoBuilder.CreateWithName("Business Meeting");

            // Act
            var result = await _occasionService.CreateOccasionAsync(createDto);

            // Assert
            result.Should().NotBe(Guid.Empty);

            // Verify occasion was created with correct name
            var createdOccasion = await _context.Occasions.FindAsync(result);
            createdOccasion.Should().NotBeNull();
            createdOccasion!.Name.Should().Be("Business Meeting");
        }

        #endregion

        #region UpdateOccasionAsync Tests

        [Fact]
        public async Task UpdateOccasionAsync_WithValidDto_ShouldUpdateOccasionAndReturnTrue()
        {
            // Arrange
            var occasion = OccasionBuilder.CreateValid();
            _context.Occasions.AddRange(new[] { occasion });
            await _context.SaveChangesAsync();

            var updateDto = UpdateOccasionDtoBuilder.CreateWithId(occasion.Id);
            updateDto.Name = "Updated Occasion Name";

            // Act
            var result = await _occasionService.UpdateOccasionAsync(updateDto);

            // Assert
            result.Should().BeTrue();

            // Verify occasion was updated in database
            var updatedOccasion = await _context.Occasions.FindAsync(occasion.Id);
            updatedOccasion.Should().NotBeNull();
            updatedOccasion!.Name.Should().Be("Updated Occasion Name");
        }

        [Fact]
        public async Task UpdateOccasionAsync_WithInvalidId_ShouldReturnFalse()
        {
            // Arrange
            var updateDto = UpdateOccasionDtoBuilder.CreateValid();
            updateDto.Id = Guid.NewGuid(); // Non-existent ID

            // Act
            var result = await _occasionService.UpdateOccasionAsync(updateDto);

            // Assert
            result.Should().BeFalse();
        }

        #endregion

        #region DeleteOccasionAsync Tests

        [Fact]
        public async Task DeleteOccasionAsync_WithValidId_ShouldDeleteOccasionAndReturnTrue()
        {
            // Arrange
            var occasion = OccasionBuilder.CreateValid();
            _context.Occasions.AddRange(new[] { occasion });
            await _context.SaveChangesAsync();

            // Act
            var result = await _occasionService.DeleteOccasionAsync(occasion.Id);

            // Assert
            result.Should().BeTrue();

            // Verify occasion was deleted from database
            var deletedOccasion = await _context.Occasions.FindAsync(occasion.Id);
            deletedOccasion.Should().BeNull();
        }

        [Fact]
        public async Task DeleteOccasionAsync_WithInvalidId_ShouldReturnFalse()
        {
            // Arrange
            var nonExistentId = Guid.NewGuid();

            // Act
            var result = await _occasionService.DeleteOccasionAsync(nonExistentId);

            // Assert
            result.Should().BeFalse();
        }

        #endregion

        #region OccasionExistsAsync Tests

        [Fact]
        public async Task OccasionExistsAsync_WhenOccasionExists_ShouldReturnTrue()
        {
            // Arrange
            var occasion = OccasionBuilder.CreateWithName("Office");
            _context.Occasions.AddRange(new[] { occasion });
            await _context.SaveChangesAsync();

            // Act
            var result = await _occasionService.OccasionExistsAsync("Office");

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task OccasionExistsAsync_WhenOccasionDoesNotExist_ShouldReturnFalse()
        {
            // Arrange
            var occasion = OccasionBuilder.CreateWithName("Office");
            _context.Occasions.AddRange(new[] { occasion });
            await _context.SaveChangesAsync();

            // Act
            var result = await _occasionService.OccasionExistsAsync("NonExistent");

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task OccasionExistsAsync_WithExcludeId_ShouldIgnoreSpecifiedOccasion()
        {
            // Arrange
            var occasion = OccasionBuilder.CreateWithName("Office");
            _context.Occasions.AddRange(new[] { occasion });
            await _context.SaveChangesAsync();

            // Act
            var result = await _occasionService.OccasionExistsAsync("Office", occasion.Id);

            // Assert
            result.Should().BeFalse(); // Should return false because we're excluding this occasion
        }

        [Fact]
        public async Task OccasionExistsAsync_WithExcludeIdButOtherExists_ShouldReturnTrue()
        {
            // Arrange
            var occasion1 = OccasionBuilder.CreateWithName("Office");
            var occasion2 = OccasionBuilder.CreateWithName("Office"); // Same name, different ID
            _context.Occasions.AddRange(new[] { occasion1, occasion2 });
            await _context.SaveChangesAsync();

            // Act
            var result = await _occasionService.OccasionExistsAsync("Office", occasion1.Id);

            // Assert
            result.Should().BeTrue(); // Should return true because occasion2 still exists
        }

        #endregion

        #region Integration Tests

        [Fact]
        public async Task CompleteWorkflow_CreateUpdateDelete_ShouldWorkCorrectly()
        {
            // Arrange
            var createDto = CreateOccasionDtoBuilder.CreateWithName("Test Occasion");

            // Act & Assert - Create
            var createdId = await _occasionService.CreateOccasionAsync(createDto);
            createdId.Should().NotBe(Guid.Empty);

            // Act & Assert - Read
            var occasion = await _occasionService.GetOccasionByIdAsync(createdId);
            occasion.Should().NotBeNull();
            occasion!.Name.Should().Be("Test Occasion");

            // Act & Assert - Update
            var updateDto = UpdateOccasionDtoBuilder.CreateWithId(createdId);
            updateDto.Name = "Updated Test Occasion";
            var updateResult = await _occasionService.UpdateOccasionAsync(updateDto);
            updateResult.Should().BeTrue();

            // Verify update
            var updatedOccasion = await _occasionService.GetOccasionByIdAsync(createdId);
            updatedOccasion!.Name.Should().Be("Updated Test Occasion");

            // Act & Assert - Delete
            var deleteResult = await _occasionService.DeleteOccasionAsync(createdId);
            deleteResult.Should().BeTrue();

            // Verify deletion
            var deletedOccasion = await _occasionService.GetOccasionByIdAsync(createdId);
            deletedOccasion.Should().BeNull();
        }

        [Fact]
        public async Task SearchAndPagination_WithMixedOccasions_ShouldWorkCorrectly()
        {
            // Arrange
            var occasions = new List<ALOud.Models.Occasion>
            {
                OccasionBuilder.CreateWithName("Office Meeting"),
                OccasionBuilder.CreateWithName("Evening Party"),
                OccasionBuilder.CreateWithName("Casual Dinner"),
                OccasionBuilder.CreateWithName("Formal Event"),
                OccasionBuilder.CreateWithName("Office Presentation")
            };
            _context.Occasions.AddRange(occasions);
            await _context.SaveChangesAsync();

            // Act
            var allOccasions = await _occasionService.GetAllOccasionsAsync();
            var officeOccasions = await _occasionService.GetAllOccasionsAsync(searchTerm: "Office");
            var firstThreeOccasions = await _occasionService.GetAllOccasionsAsync(pageSize: 3);

            // Assert
            allOccasions.TotalCount.Should().Be(5);
            officeOccasions.TotalCount.Should().Be(2);
            officeOccasions.Items.Should().OnlyContain(o => o.Name.Contains("Office"));
            
            firstThreeOccasions.Items.Should().HaveCount(3);
            firstThreeOccasions.TotalPages.Should().Be(2);
        }

        [Fact]
        public async Task GetAllOccasionsForSelect_WithVariousOccasions_ShouldMaintainCorrectOrdering()
        {
            // Arrange
            var occasions = OccasionBuilder.CreateWithNames("Wedding", "Office", "Party", "Casual", "Evening");
            _context.Occasions.AddRange(occasions);
            await _context.SaveChangesAsync();

            // Act
            var result = await _occasionService.GetAllOccasionsForSelectAsync();

            // Assert
            result.Should().HaveCount(5);
            result.Should().BeInAscendingOrder(o => o.Name);
            
            // Verify specific ordering
            var names = result.Select(o => o.Name).ToList();
            names.Should().Equal("Casual", "Evening", "Office", "Party", "Wedding");
        }

        #endregion
    }
}