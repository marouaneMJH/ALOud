using System;
using Xunit;
using FluentAssertions;
using ALOud.Services.Accord;
using ALOud.Tests.Helpers;
using ALOud.DTOs.Accords;
using ALOud.Models;

namespace ALOud.Tests.Services.Business
{
    /// <summary>
    /// Unit tests for AccordService using Strategy B (EF InMemory).
    /// Tests the accord business logic with in-memory database.
    /// 
    /// Test naming convention: MethodName_Scenario_ExpectedBehavior
    /// </summary>
    public class AccordServiceTests : IDisposable
    {
        private readonly AccordService _accordService;
        private readonly Data.ALOudDbContext _context;

        public AccordServiceTests()
        {
            _context = DbContextFactory.CreateAndEnsureCreated();
            _accordService = new AccordService(_context);
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            _context.Dispose();
        }

        #region GetAllAccordsAsync Tests

        [Fact]
        public async Task GetAllAccordsAsync_WhenHasData_ReturnsCorrectPageWithTotalCount()
        {
            // Arrange
            var accords = new List<Accord>
            {
                new() { Id = Guid.NewGuid(), Name = "Sweet", Description = "Sweet accord" },
                new() { Id = Guid.NewGuid(), Name = "Woody", Description = "Woody accord" },
                new() { Id = Guid.NewGuid(), Name = "Fresh", Description = "Fresh accord" }
            };

            _context.Accords.AddRange(accords);
            await _context.SaveChangesAsync();

            // Act
            var result = await _accordService.GetAllAccordsAsync(pageIndex: 1, pageSize: 2);

            // Assert
            result.Should().NotBeNull();
            result.Items.Should().HaveCount(2);
            result.TotalCount.Should().Be(3);
            result.TotalPages.Should().Be(2);
            result.HasNextPage.Should().BeTrue();
            result.HasPreviousPage.Should().BeFalse();
            
            // Items should be ordered by name
            result.Items[0].Name.Should().Be("Fresh");
            result.Items[1].Name.Should().Be("Sweet");
        }

        [Fact]
        public async Task GetAllAccordsAsync_WhenSearchTermProvided_ReturnsOnlyMatchingAccords()
        {
            // Arrange
            var accords = new List<Accord>
            {
                new() { Id = Guid.NewGuid(), Name = "Sweet", Description = "Sweet accord" },
                new() { Id = Guid.NewGuid(), Name = "Sweety", Description = "Another sweet accord" },
                new() { Id = Guid.NewGuid(), Name = "Woody", Description = "Woody accord" }
            };

            _context.Accords.AddRange(accords);
            await _context.SaveChangesAsync();

            // Act
            var result = await _accordService.GetAllAccordsAsync(searchTerm: "Sweet");

            // Assert
            result.Should().NotBeNull();
            result.Items.Should().HaveCount(2);
            result.TotalCount.Should().Be(2);
            result.Items.Should().OnlyContain(a => a.Name.Contains("Sweet"));
        }

        [Fact]
        public async Task GetAllAccordsAsync_WhenEmptyDatabase_ReturnsEmptyPaginatedList()
        {
            // Act
            var result = await _accordService.GetAllAccordsAsync();

            // Assert
            result.Should().NotBeNull();
            result.Items.Should().BeEmpty();
            result.TotalCount.Should().Be(0);
        }

        #endregion

        #region GetAccordByIdAsync Tests

        [Fact]
        public async Task GetAccordByIdAsync_WhenAccordExists_ReturnsCorrectAccordDto()
        {
            // Arrange
            var accordId = Guid.NewGuid();
            var accord = new Accord 
            { 
                Id = accordId, 
                Name = "Test Accord", 
                Description = "Test Description" 
            };

            _context.Accords.Add(accord);
            await _context.SaveChangesAsync();

            // Act
            var result = await _accordService.GetAccordByIdAsync(accordId);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(accordId);
            result.Name.Should().Be("Test Accord");
            result.Description.Should().Be("Test Description");
        }

        [Fact]
        public async Task GetAccordByIdAsync_WhenAccordNotFound_ReturnsNull()
        {
            // Arrange
            var nonExistentId = Guid.NewGuid();

            // Act
            var result = await _accordService.GetAccordByIdAsync(nonExistentId);

            // Assert
            result.Should().BeNull();
        }

        #endregion

        #region CreateAccordAsync Tests

        [Fact]
        public async Task CreateAccordAsync_WhenValidDto_ReturnsNewGuidAndSavesToDatabase()
        {
            // Arrange
            var createDto = new CreateAccordDto
            {
                Name = "New Accord",
                Description = "New Description"
            };

            // Act
            var result = await _accordService.CreateAccordAsync(createDto);

            // Assert
            result.Should().NotBeEmpty();
            
            var savedAccord = await _context.Accords.FindAsync(result);
            savedAccord.Should().NotBeNull();
            savedAccord!.Name.Should().Be("New Accord");
            savedAccord.Description.Should().Be("New Description");
        }

        #endregion

        #region UpdateAccordAsync Tests

        [Fact]
        public async Task UpdateAccordAsync_WhenAccordExists_ReturnsTrueAndUpdatesDatabase()
        {
            // Arrange
            var accordId = Guid.NewGuid();
            var originalAccord = new Accord 
            { 
                Id = accordId, 
                Name = "Original Name", 
                Description = "Original Description" 
            };

            _context.Accords.Add(originalAccord);
            await _context.SaveChangesAsync();

            var updateDto = new UpdateAccordDto
            {
                Id = accordId,
                Name = "Updated Name",
                Description = "Updated Description"
            };

            // Act
            var result = await _accordService.UpdateAccordAsync(updateDto);

            // Assert
            result.Should().BeTrue();
            
            var updatedAccord = await _context.Accords.FindAsync(accordId);
            updatedAccord!.Name.Should().Be("Updated Name");
            updatedAccord.Description.Should().Be("Updated Description");
        }

        [Fact]
        public async Task UpdateAccordAsync_WhenAccordNotFound_ReturnsFalse()
        {
            // Arrange
            var updateDto = new UpdateAccordDto
            {
                Id = Guid.NewGuid(),
                Name = "Updated Name"
            };

            // Act
            var result = await _accordService.UpdateAccordAsync(updateDto);

            // Assert
            result.Should().BeFalse();
        }

        #endregion

        #region DeleteAccordAsync Tests

        [Fact]
        public async Task DeleteAccordAsync_WhenAccordExists_ReturnsTrueAndDeletesFromDatabase()
        {
            // Arrange
            var accordId = Guid.NewGuid();
            var accord = new Accord { Id = accordId, Name = "Accord to Delete" };

            _context.Accords.Add(accord);
            await _context.SaveChangesAsync();

            // Act
            var result = await _accordService.DeleteAccordAsync(accordId);

            // Assert
            result.Should().BeTrue();
            
            var deletedAccord = await _context.Accords.FindAsync(accordId);
            deletedAccord.Should().BeNull();
        }

        [Fact]
        public async Task DeleteAccordAsync_WhenAccordNotFound_ReturnsFalse()
        {
            // Arrange
            var nonExistentId = Guid.NewGuid();

            // Act
            var result = await _accordService.DeleteAccordAsync(nonExistentId);

            // Assert
            result.Should().BeFalse();
        }

        #endregion

        #region AccordExistsAsync Tests

        [Fact]
        public async Task AccordExistsAsync_WhenAccordNameExists_ReturnsTrue()
        {
            // Arrange
            var accord = new Accord { Id = Guid.NewGuid(), Name = "Existing Accord" };
            _context.Accords.Add(accord);
            await _context.SaveChangesAsync();

            // Act
            var result = await _accordService.AccordExistsAsync("Existing Accord");

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task AccordExistsAsync_WhenAccordNameDoesNotExist_ReturnsFalse()
        {
            // Act
            var result = await _accordService.AccordExistsAsync("Non-Existing Accord");

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task AccordExistsAsync_WhenExcludingCurrentAccord_ReturnsFalseForSameName()
        {
            // Arrange
            var accordId = Guid.NewGuid();
            var accord = new Accord { Id = accordId, Name = "Test Accord" };
            _context.Accords.Add(accord);
            await _context.SaveChangesAsync();

            // Act
            var result = await _accordService.AccordExistsAsync("Test Accord", excludeId: accordId);

            // Assert
            result.Should().BeFalse();
        }

        #endregion

        #region Integration Tests

        [Fact]
        public async Task CreateThenUpdateThenDelete_FullWorkflow_Success()
        {
            // Arrange & Act - Create
            var createDto = new CreateAccordDto 
            { 
                Name = "Workflow Accord", 
                Description = "Workflow Description" 
            };
            var createdId = await _accordService.CreateAccordAsync(createDto);
            
            // Assert - Create
            var createdAccord = await _accordService.GetAccordByIdAsync(createdId);
            createdAccord.Should().NotBeNull();
            createdAccord!.Name.Should().Be("Workflow Accord");

            // Act - Update
            var updateDto = new UpdateAccordDto 
            { 
                Id = createdId, 
                Name = "Updated Workflow Accord",
                Description = "Updated Description"
            };
            var updateResult = await _accordService.UpdateAccordAsync(updateDto);

            // Assert - Update
            updateResult.Should().BeTrue();
            var updatedAccord = await _accordService.GetAccordByIdAsync(createdId);
            updatedAccord!.Name.Should().Be("Updated Workflow Accord");

            // Act - Delete
            var deleteResult = await _accordService.DeleteAccordAsync(createdId);

            // Assert - Delete
            deleteResult.Should().BeTrue();
            var deletedAccord = await _accordService.GetAccordByIdAsync(createdId);
            deletedAccord.Should().BeNull();
        }

        #endregion
    }
}