using System;
using Xunit;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using ALOud.Services.Season;
using ALOud.DTOs.Seasons;
using Tests.Common.TestDataBuilders;
using ALOud.Tests.Helpers;
using ALOud.Data;

namespace Tests.Services.Business
{
    public class SeasonServiceTests : IDisposable
    {
        private readonly ALOudDbContext _context;
        private readonly SeasonService _seasonService;

        public SeasonServiceTests()
        {
            _context = DbContextFactory.CreateAndEnsureCreated();
            _seasonService = new SeasonService(_context);
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            _context.Dispose();
        }

        #region GetAllSeasonsAsync Tests

        [Fact]
        public async Task GetAllSeasonsAsync_WithDefaultParameters_ShouldReturnPaginatedResults()
        {
            // Arrange
            var seasons = SeasonBuilder.CreateValidList(15);
            _context.Seasons.AddRange(seasons);
            await _context.SaveChangesAsync();

            // Act
            var result = await _seasonService.GetAllSeasonsAsync();

            // Assert
            result.Should().NotBeNull();
            result.Items.Should().HaveCount(10); // Default page size
            result.PageIndex.Should().Be(1);
            result.PageSize.Should().Be(10);
            result.TotalCount.Should().Be(15);
            result.TotalPages.Should().Be(2);
            result.Items.Should().BeInAscendingOrder(s => s.Name);
        }

        [Fact]
        public async Task GetAllSeasonsAsync_WithCustomPagination_ShouldReturnCorrectPage()
        {
            // Arrange
            var seasons = SeasonBuilder.CreateValidList(25);
            _context.Seasons.AddRange(seasons);
            await _context.SaveChangesAsync();

            // Act
            var result = await _seasonService.GetAllSeasonsAsync(pageIndex: 2, pageSize: 5);

            // Assert
            result.Should().NotBeNull();
            result.Items.Should().HaveCount(5);
            result.PageIndex.Should().Be(2);
            result.PageSize.Should().Be(5);
            result.TotalCount.Should().Be(25);
            result.TotalPages.Should().Be(5);
        }

        [Fact]
        public async Task GetAllSeasonsAsync_WithSearchTerm_ShouldReturnFilteredResults()
        {
            // Arrange
            var searchSeason = SeasonBuilder.CreateWithName("Spring Bloom");
            var nonMatchingSeasons = new List<ALOud.Models.Season>
            {
                SeasonBuilder.CreateWithName("Summer"),
                SeasonBuilder.CreateWithName("Autumn"),
                SeasonBuilder.CreateWithName("Winter"),
                SeasonBuilder.CreateWithName("All Season")
            };
            
            var seasons = new List<ALOud.Models.Season> { searchSeason };
            seasons.AddRange(nonMatchingSeasons);
            
            _context.Seasons.AddRange(seasons);
            await _context.SaveChangesAsync();

            // Act
            var result = await _seasonService.GetAllSeasonsAsync(searchTerm: "Spring");

            // Assert
            result.Should().NotBeNull();
            result.Items.Should().HaveCount(1);
            result.Items.First().Name.Should().Contain("Spring");
            result.TotalCount.Should().Be(1);
        }

        [Fact]
        public async Task GetAllSeasonsAsync_WithNonMatchingSearchTerm_ShouldReturnEmptyResults()
        {
            // Arrange
            var seasons = SeasonBuilder.CreateValidList(5);
            _context.Seasons.AddRange(seasons);
            await _context.SaveChangesAsync();

            // Act
            var result = await _seasonService.GetAllSeasonsAsync(searchTerm: "NonExistentSeason");

            // Assert
            result.Should().NotBeNull();
            result.Items.Should().BeEmpty();
            result.TotalCount.Should().Be(0);
        }

        [Fact]
        public async Task GetAllSeasonsAsync_WhenEmpty_ShouldReturnEmptyPaginatedList()
        {
            // Act
            var result = await _seasonService.GetAllSeasonsAsync();

            // Assert
            result.Should().NotBeNull();
            result.Items.Should().BeEmpty();
            result.TotalCount.Should().Be(0);
            result.TotalPages.Should().Be(0);
        }

        #endregion

        #region GetAllSeasonsForSelectAsync Tests

        [Fact]
        public async Task GetAllSeasonsForSelectAsync_ShouldReturnAllSeasonsOrderedByName()
        {
            // Arrange
            var seasons = SeasonBuilder.CreateWithNames("Winter", "Spring", "Summer", "Autumn");
            _context.Seasons.AddRange(seasons);
            await _context.SaveChangesAsync();

            // Act
            var result = await _seasonService.GetAllSeasonsForSelectAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(4);
            result.Should().BeInAscendingOrder(s => s.Name);
            result.All(s => !string.IsNullOrEmpty(s.Name)).Should().BeTrue();
            result.All(s => s.Id != Guid.Empty).Should().BeTrue();
            
            // Verify alphabetical ordering
            result[0].Name.Should().Be("Autumn");
            result[1].Name.Should().Be("Spring");
            result[2].Name.Should().Be("Summer");
            result[3].Name.Should().Be("Winter");
        }

        [Fact]
        public async Task GetAllSeasonsForSelectAsync_WhenEmpty_ShouldReturnEmptyList()
        {
            // Act
            var result = await _seasonService.GetAllSeasonsForSelectAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        #endregion

        #region GetSeasonByIdAsync Tests

        [Fact]
        public async Task GetSeasonByIdAsync_WithValidId_ShouldReturnSeason()
        {
            // Arrange
            var season = SeasonBuilder.CreateValid();
            _context.Seasons.AddRange(new[] { season });
            await _context.SaveChangesAsync();

            // Act
            var result = await _seasonService.GetSeasonByIdAsync(season.Id);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(season.Id);
            result.Name.Should().Be(season.Name);
            result.PerfumeCount.Should().Be(0); // No perfume seasons seeded
        }

        [Fact]
        public async Task GetSeasonByIdAsync_WithInvalidId_ShouldReturnNull()
        {
            // Arrange
            var nonExistentId = Guid.NewGuid();

            // Act
            var result = await _seasonService.GetSeasonByIdAsync(nonExistentId);

            // Assert
            result.Should().BeNull();
        }

        #endregion

        #region GetSeasonForEditAsync Tests

        [Fact]
        public async Task GetSeasonForEditAsync_WithValidId_ShouldReturnUpdateDto()
        {
            // Arrange
            var season = SeasonBuilder.CreateValid();
            _context.Seasons.AddRange(new[] { season });
            await _context.SaveChangesAsync();

            // Act
            var result = await _seasonService.GetSeasonForEditAsync(season.Id);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(season.Id);
            result.Name.Should().Be(season.Name);
        }

        [Fact]
        public async Task GetSeasonForEditAsync_WithInvalidId_ShouldReturnNull()
        {
            // Arrange
            var nonExistentId = Guid.NewGuid();

            // Act
            var result = await _seasonService.GetSeasonForEditAsync(nonExistentId);

            // Assert
            result.Should().BeNull();
        }

        #endregion

        #region CreateSeasonAsync Tests

        [Fact]
        public async Task CreateSeasonAsync_WithValidDto_ShouldCreateSeasonAndReturnId()
        {
            // Arrange
            var createDto = CreateSeasonDtoBuilder.CreateValid();

            // Act
            var result = await _seasonService.CreateSeasonAsync(createDto);

            // Assert
            result.Should().NotBe(Guid.Empty);

            // Verify season was created in database
            var createdSeason = await _context.Seasons.FindAsync(result);
            createdSeason.Should().NotBeNull();
            createdSeason!.Name.Should().Be(createDto.Name);
        }

        [Fact]
        public async Task CreateSeasonAsync_WithSpecificSeasonName_ShouldCreateSeason()
        {
            // Arrange
            var createDto = CreateSeasonDtoBuilder.CreateWithName("Spring");

            // Act
            var result = await _seasonService.CreateSeasonAsync(createDto);

            // Assert
            result.Should().NotBe(Guid.Empty);

            // Verify season was created with correct name
            var createdSeason = await _context.Seasons.FindAsync(result);
            createdSeason.Should().NotBeNull();
            createdSeason!.Name.Should().Be("Spring");
        }

        #endregion

        #region UpdateSeasonAsync Tests

        [Fact]
        public async Task UpdateSeasonAsync_WithValidDto_ShouldUpdateSeasonAndReturnTrue()
        {
            // Arrange
            var season = SeasonBuilder.CreateValid();
            _context.Seasons.AddRange(new[] { season });
            await _context.SaveChangesAsync();

            var updateDto = UpdateSeasonDtoBuilder.CreateWithId(season.Id);
            updateDto.Name = "Updated Season Name";

            // Act
            var result = await _seasonService.UpdateSeasonAsync(updateDto);

            // Assert
            result.Should().BeTrue();

            // Verify season was updated in database
            var updatedSeason = await _context.Seasons.FindAsync(season.Id);
            updatedSeason.Should().NotBeNull();
            updatedSeason!.Name.Should().Be("Updated Season Name");
        }

        [Fact]
        public async Task UpdateSeasonAsync_WithInvalidId_ShouldReturnFalse()
        {
            // Arrange
            var updateDto = UpdateSeasonDtoBuilder.CreateValid();
            updateDto.Id = Guid.NewGuid(); // Non-existent ID

            // Act
            var result = await _seasonService.UpdateSeasonAsync(updateDto);

            // Assert
            result.Should().BeFalse();
        }

        #endregion

        #region DeleteSeasonAsync Tests

        [Fact]
        public async Task DeleteSeasonAsync_WithValidId_ShouldDeleteSeasonAndReturnTrue()
        {
            // Arrange
            var season = SeasonBuilder.CreateValid();
            _context.Seasons.AddRange(new[] { season });
            await _context.SaveChangesAsync();

            // Act
            var result = await _seasonService.DeleteSeasonAsync(season.Id);

            // Assert
            result.Should().BeTrue();

            // Verify season was deleted from database
            var deletedSeason = await _context.Seasons.FindAsync(season.Id);
            deletedSeason.Should().BeNull();
        }

        [Fact]
        public async Task DeleteSeasonAsync_WithInvalidId_ShouldReturnFalse()
        {
            // Arrange
            var nonExistentId = Guid.NewGuid();

            // Act
            var result = await _seasonService.DeleteSeasonAsync(nonExistentId);

            // Assert
            result.Should().BeFalse();
        }

        #endregion

        #region SeasonExistsAsync Tests

        [Fact]
        public async Task SeasonExistsAsync_WhenSeasonExists_ShouldReturnTrue()
        {
            // Arrange
            var season = SeasonBuilder.CreateWithName("Spring");
            _context.Seasons.AddRange(new[] { season });
            await _context.SaveChangesAsync();

            // Act
            var result = await _seasonService.SeasonExistsAsync("Spring");

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task SeasonExistsAsync_WhenSeasonDoesNotExist_ShouldReturnFalse()
        {
            // Arrange
            var season = SeasonBuilder.CreateWithName("Spring");
            _context.Seasons.AddRange(new[] { season });
            await _context.SaveChangesAsync();

            // Act
            var result = await _seasonService.SeasonExistsAsync("NonExistent");

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task SeasonExistsAsync_WithExcludeId_ShouldIgnoreSpecifiedSeason()
        {
            // Arrange
            var season = SeasonBuilder.CreateWithName("Spring");
            _context.Seasons.AddRange(new[] { season });
            await _context.SaveChangesAsync();

            // Act
            var result = await _seasonService.SeasonExistsAsync("Spring", season.Id);

            // Assert
            result.Should().BeFalse(); // Should return false because we're excluding this season
        }

        [Fact]
        public async Task SeasonExistsAsync_WithExcludeIdButOtherExists_ShouldReturnTrue()
        {
            // Arrange
            var season1 = SeasonBuilder.CreateWithName("Spring");
            var season2 = SeasonBuilder.CreateWithName("Spring"); // Same name, different ID
            _context.Seasons.AddRange(new[] { season1, season2 });
            await _context.SaveChangesAsync();

            // Act
            var result = await _seasonService.SeasonExistsAsync("Spring", season1.Id);

            // Assert
            result.Should().BeTrue(); // Should return true because season2 still exists
        }

        #endregion

        #region Integration Tests

        [Fact]
        public async Task CompleteWorkflow_CreateUpdateDelete_ShouldWorkCorrectly()
        {
            // Arrange
            var createDto = CreateSeasonDtoBuilder.CreateWithName("Test Season");

            // Act & Assert - Create
            var createdId = await _seasonService.CreateSeasonAsync(createDto);
            createdId.Should().NotBe(Guid.Empty);

            // Act & Assert - Read
            var season = await _seasonService.GetSeasonByIdAsync(createdId);
            season.Should().NotBeNull();
            season!.Name.Should().Be("Test Season");

            // Act & Assert - Update
            var updateDto = UpdateSeasonDtoBuilder.CreateWithId(createdId);
            updateDto.Name = "Updated Test Season";
            var updateResult = await _seasonService.UpdateSeasonAsync(updateDto);
            updateResult.Should().BeTrue();

            // Verify update
            var updatedSeason = await _seasonService.GetSeasonByIdAsync(createdId);
            updatedSeason!.Name.Should().Be("Updated Test Season");

            // Act & Assert - Delete
            var deleteResult = await _seasonService.DeleteSeasonAsync(createdId);
            deleteResult.Should().BeTrue();

            // Verify deletion
            var deletedSeason = await _seasonService.GetSeasonByIdAsync(createdId);
            deletedSeason.Should().BeNull();
        }

        [Fact]
        public async Task SearchAndPagination_WithMixedSeasons_ShouldWorkCorrectly()
        {
            // Arrange
            var seasons = new List<ALOud.Models.Season>
            {
                SeasonBuilder.CreateWithName("Spring Blossom"),
                SeasonBuilder.CreateWithName("Summer Heat"),
                SeasonBuilder.CreateWithName("Autumn Leaves"),
                SeasonBuilder.CreateWithName("Winter Chill"),
                SeasonBuilder.CreateWithName("Spring Garden")
            };
            _context.Seasons.AddRange(seasons);
            await _context.SaveChangesAsync();

            // Act
            var allSeasons = await _seasonService.GetAllSeasonsAsync();
            var springSeasons = await _seasonService.GetAllSeasonsAsync(searchTerm: "Spring");
            var firstThreeSeasons = await _seasonService.GetAllSeasonsAsync(pageSize: 3);

            // Assert
            allSeasons.TotalCount.Should().Be(5);
            springSeasons.TotalCount.Should().Be(2);
            springSeasons.Items.Should().OnlyContain(s => s.Name.Contains("Spring"));
            
            firstThreeSeasons.Items.Should().HaveCount(3);
            firstThreeSeasons.TotalPages.Should().Be(2);
        }

        [Fact]
        public async Task GetAllSeasonsForSelect_WithVariousSeasons_ShouldMaintainCorrectOrdering()
        {
            // Arrange
            var seasons = SeasonBuilder.CreateWithNames("Winter", "Autumn", "Summer", "Spring", "All Season");
            _context.Seasons.AddRange(seasons);
            await _context.SaveChangesAsync();

            // Act
            var result = await _seasonService.GetAllSeasonsForSelectAsync();

            // Assert
            result.Should().HaveCount(5);
            result.Should().BeInAscendingOrder(s => s.Name);
            
            // Verify specific ordering
            var names = result.Select(s => s.Name).ToList();
            names.Should().Equal("All Season", "Autumn", "Spring", "Summer", "Winter");
        }

        #endregion
    }
}