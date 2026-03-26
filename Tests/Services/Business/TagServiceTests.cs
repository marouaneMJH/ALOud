using System;
using Xunit;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using ALOud.Services.Tag;
using ALOud.DTOs.Tags;
using Tests.Common.TestDataBuilders;
using ALOud.Tests.Helpers;
using ALOud.Data;

namespace Tests.Services.Business
{
    public class TagServiceTests : IDisposable
    {
        private readonly ALOudDbContext _context;
        private readonly TagService _tagService;

        public TagServiceTests()
        {
            _context = DbContextFactory.CreateAndEnsureCreated();
            _tagService = new TagService(_context);
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            _context.Dispose();
        }

        #region GetAllTagsAsync Tests

        [Fact]
        public async Task GetAllTagsAsync_WithDefaultParameters_ShouldReturnPaginatedResults()
        {
            // Arrange
            var tags = TagBuilder.CreateValidList(15);
            _context.Tags.AddRange(tags);
            await _context.SaveChangesAsync();

            // Act
            var result = await _tagService.GetAllTagsAsync();

            // Assert
            result.Should().NotBeNull();
            result.Items.Should().HaveCount(10); // Default page size
            result.PageIndex.Should().Be(1);
            result.PageSize.Should().Be(10);
            result.TotalCount.Should().Be(15);
            result.TotalPages.Should().Be(2);
            result.Items.Should().BeInAscendingOrder(t => t.Name);
        }

        [Fact]
        public async Task GetAllTagsAsync_WithSearchTerm_ShouldReturnFilteredResults()
        {
            // Arrange
            var searchTag = TagBuilder.CreateWithName("Luxury Collection");
            var tags = new List<ALOud.Models.Tag> { searchTag };
            tags.AddRange(TagBuilder.CreateValidList(10));
            _context.Tags.AddRange(tags);
            await _context.SaveChangesAsync();

            // Act
            var result = await _tagService.GetAllTagsAsync(searchTerm: "Luxury");

            // Assert
            result.Should().NotBeNull();
            result.Items.Should().HaveCount(1);
            result.Items.First().Name.Should().Contain("Luxury");
            result.TotalCount.Should().Be(1);
        }

        [Fact]
        public async Task GetAllTagsAsync_WhenEmpty_ShouldReturnEmptyPaginatedList()
        {
            // Act
            var result = await _tagService.GetAllTagsAsync();

            // Assert
            result.Should().NotBeNull();
            result.Items.Should().BeEmpty();
            result.TotalCount.Should().Be(0);
            result.TotalPages.Should().Be(0);
        }

        #endregion

        #region GetAllTagsForSelectAsync Tests

        [Fact]
        public async Task GetAllTagsForSelectAsync_ShouldReturnAllTagsOrderedByName()
        {
            // Arrange
            var tags = TagBuilder.CreateWithNames("Vintage", "Modern", "Luxury", "Oriental");
            _context.Tags.AddRange(tags);
            await _context.SaveChangesAsync();

            // Act
            var result = await _tagService.GetAllTagsForSelectAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(4);
            result.Should().BeInAscendingOrder(t => t.Name);
            result.All(t => !string.IsNullOrEmpty(t.Name)).Should().BeTrue();
            result.All(t => t.Id != Guid.Empty).Should().BeTrue();
        }

        [Fact]
        public async Task GetAllTagsForSelectAsync_WhenEmpty_ShouldReturnEmptyList()
        {
            // Act
            var result = await _tagService.GetAllTagsForSelectAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        #endregion

        #region GetTagByIdAsync Tests

        [Fact]
        public async Task GetTagByIdAsync_WithValidId_ShouldReturnTag()
        {
            // Arrange
            var tag = TagBuilder.CreateValid();
            _context.Tags.AddRange(new[] { tag });
            await _context.SaveChangesAsync();

            // Act
            var result = await _tagService.GetTagByIdAsync(tag.Id);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(tag.Id);
            result.Name.Should().Be(tag.Name);
            result.PerfumeCount.Should().Be(0); // No perfume tags seeded
        }

        [Fact]
        public async Task GetTagByIdAsync_WithInvalidId_ShouldReturnNull()
        {
            // Arrange
            var nonExistentId = Guid.NewGuid();

            // Act
            var result = await _tagService.GetTagByIdAsync(nonExistentId);

            // Assert
            result.Should().BeNull();
        }

        #endregion

        #region CreateTagAsync Tests

        [Fact]
        public async Task CreateTagAsync_WithValidDto_ShouldCreateTagAndReturnId()
        {
            // Arrange
            var createDto = CreateTagDtoBuilder.CreateValid();

            // Act
            var result = await _tagService.CreateTagAsync(createDto);

            // Assert
            result.Should().NotBe(Guid.Empty);

            // Verify tag was created in database
            var createdTag = await _context.Tags.FindAsync(result);
            createdTag.Should().NotBeNull();
            createdTag!.Name.Should().Be(createDto.Name);
        }

        #endregion

        #region UpdateTagAsync Tests

        [Fact]
        public async Task UpdateTagAsync_WithValidDto_ShouldUpdateTagAndReturnTrue()
        {
            // Arrange
            var tag = TagBuilder.CreateValid();
            _context.Tags.AddRange(new[] { tag });
            await _context.SaveChangesAsync();

            var updateDto = UpdateTagDtoBuilder.CreateWithId(tag.Id);
            updateDto.Name = "Updated Tag Name";

            // Act
            var result = await _tagService.UpdateTagAsync(updateDto);

            // Assert
            result.Should().BeTrue();

            // Verify tag was updated in database
            var updatedTag = await _context.Tags.FindAsync(tag.Id);
            updatedTag.Should().NotBeNull();
            updatedTag!.Name.Should().Be("Updated Tag Name");
        }

        [Fact]
        public async Task UpdateTagAsync_WithInvalidId_ShouldReturnFalse()
        {
            // Arrange
            var updateDto = UpdateTagDtoBuilder.CreateValid();
            updateDto.Id = Guid.NewGuid(); // Non-existent ID

            // Act
            var result = await _tagService.UpdateTagAsync(updateDto);

            // Assert
            result.Should().BeFalse();
        }

        #endregion

        #region DeleteTagAsync Tests

        [Fact]
        public async Task DeleteTagAsync_WithValidId_ShouldDeleteTagAndReturnTrue()
        {
            // Arrange
            var tag = TagBuilder.CreateValid();
            _context.Tags.AddRange(new[] { tag });
            await _context.SaveChangesAsync();

            // Act
            var result = await _tagService.DeleteTagAsync(tag.Id);

            // Assert
            result.Should().BeTrue();

            // Verify tag was deleted from database
            var deletedTag = await _context.Tags.FindAsync(tag.Id);
            deletedTag.Should().BeNull();
        }

        [Fact]
        public async Task DeleteTagAsync_WithInvalidId_ShouldReturnFalse()
        {
            // Arrange
            var nonExistentId = Guid.NewGuid();

            // Act
            var result = await _tagService.DeleteTagAsync(nonExistentId);

            // Assert
            result.Should().BeFalse();
        }

        #endregion

        #region TagExistsAsync Tests

        [Fact]
        public async Task TagExistsAsync_WhenTagExists_ShouldReturnTrue()
        {
            // Arrange
            var tag = TagBuilder.CreateWithName("Luxury");
            _context.Tags.AddRange(new[] { tag });
            await _context.SaveChangesAsync();

            // Act
            var result = await _tagService.TagExistsAsync("Luxury");

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task TagExistsAsync_WhenTagDoesNotExist_ShouldReturnFalse()
        {
            // Arrange
            var tag = TagBuilder.CreateWithName("Luxury");
            _context.Tags.AddRange(new[] { tag });
            await _context.SaveChangesAsync();

            // Act
            var result = await _tagService.TagExistsAsync("NonExistent");

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task TagExistsAsync_WithExcludeId_ShouldIgnoreSpecifiedTag()
        {
            // Arrange
            var tag = TagBuilder.CreateWithName("Luxury");
            _context.Tags.AddRange(new[] { tag });
            await _context.SaveChangesAsync();

            // Act
            var result = await _tagService.TagExistsAsync("Luxury", tag.Id);

            // Assert
            result.Should().BeFalse(); // Should return false because we're excluding this tag
        }

        #endregion

        #region Integration Tests

        [Fact]
        public async Task CompleteWorkflow_CreateUpdateDelete_ShouldWorkCorrectly()
        {
            // Arrange
            var createDto = CreateTagDtoBuilder.CreateWithName("Test Tag");

            // Act & Assert - Create
            var createdId = await _tagService.CreateTagAsync(createDto);
            createdId.Should().NotBe(Guid.Empty);

            // Act & Assert - Read
            var tag = await _tagService.GetTagByIdAsync(createdId);
            tag.Should().NotBeNull();
            tag!.Name.Should().Be("Test Tag");

            // Act & Assert - Update
            var updateDto = UpdateTagDtoBuilder.CreateWithId(createdId);
            updateDto.Name = "Updated Test Tag";
            var updateResult = await _tagService.UpdateTagAsync(updateDto);
            updateResult.Should().BeTrue();

            // Verify update
            var updatedTag = await _tagService.GetTagByIdAsync(createdId);
            updatedTag!.Name.Should().Be("Updated Test Tag");

            // Act & Assert - Delete
            var deleteResult = await _tagService.DeleteTagAsync(createdId);
            deleteResult.Should().BeTrue();

            // Verify deletion
            var deletedTag = await _tagService.GetTagByIdAsync(createdId);
            deletedTag.Should().BeNull();
        }

        #endregion
    }
}