using System;
using Xunit;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using ALOud.Services.Note;
using ALOud.DTOs.Notes;
using Tests.Common.TestDataBuilders;
using ALOud.Tests.Helpers;
using ALOud.Data;

namespace Tests.Services.Business
{
    public class NoteServiceTests : IDisposable
    {
        private readonly ALOudDbContext _context;
        private readonly NoteService _noteService;

        public NoteServiceTests()
        {
            _context = DbContextFactory.CreateAndEnsureCreated();
            _noteService = new NoteService(_context);
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            _context.Dispose();
        }

        #region GetAllNotesAsync Tests

        [Fact]
        public async Task GetAllNotesAsync_WithDefaultParameters_ShouldReturnPaginatedResults()
        {
            // Arrange
            var notes = NoteBuilder.CreateValidList(15);
            _context.Notes.AddRange(notes);
            await _context.SaveChangesAsync();

            // Act
            var result = await _noteService.GetAllNotesAsync();

            // Assert
            result.Should().NotBeNull();
            result.Items.Should().HaveCount(10); // Default page size
            result.PageIndex.Should().Be(1);
            result.PageSize.Should().Be(10);
            result.TotalCount.Should().Be(15);
            result.TotalPages.Should().Be(2);
            result.Items.Should().BeInAscendingOrder(n => n.Category).And.ThenBeInAscendingOrder(n => n.Name);
        }

        [Fact]
        public async Task GetAllNotesAsync_WithCustomPagination_ShouldReturnCorrectPage()
        {
            // Arrange
            var notes = NoteBuilder.CreateValidList(25);
            _context.Notes.AddRange(notes);
            await _context.SaveChangesAsync();

            // Act
            var result = await _noteService.GetAllNotesAsync(pageIndex: 2, pageSize: 5);

            // Assert
            result.Should().NotBeNull();
            result.Items.Should().HaveCount(5);
            result.PageIndex.Should().Be(2);
            result.PageSize.Should().Be(5);
            result.TotalCount.Should().Be(25);
            result.TotalPages.Should().Be(5);
        }

        [Fact]
        public async Task GetAllNotesAsync_WithSearchTerm_ShouldReturnFilteredResults()
        {
            // Arrange
            var searchNote = NoteBuilder.CreateWithName("Rose Petal");
            var notes = new List<ALOud.Models.Note> { searchNote };
            notes.AddRange(NoteBuilder.CreateValidList(10));
            _context.Notes.AddRange(notes);
            await _context.SaveChangesAsync();

            // Act
            var result = await _noteService.GetAllNotesAsync(searchTerm: "Rose");

            // Assert
            result.Should().NotBeNull();
            result.Items.Should().HaveCount(1);
            result.Items.First().Name.Should().Contain("Rose");
            result.TotalCount.Should().Be(1);
        }

        [Fact]
        public async Task GetAllNotesAsync_WithCategoryFilter_ShouldReturnFilteredResults()
        {
            // Arrange
            var floralNotes = NoteBuilder.CreateWithCategories("Floral", "Floral");
            var woodyNotes = NoteBuilder.CreateWithCategories("Woody", "Woody", "Woody");
            var allNotes = floralNotes.Concat(woodyNotes).ToList();
            _context.Notes.AddRange(allNotes);
            await _context.SaveChangesAsync();

            // Act
            var result = await _noteService.GetAllNotesAsync(category: "Floral");

            // Assert
            result.Should().NotBeNull();
            result.Items.Should().HaveCount(2);
            result.Items.Should().OnlyContain(n => n.Category == "Floral");
            result.TotalCount.Should().Be(2);
        }

        [Fact]
        public async Task GetAllNotesAsync_WithSearchTermAndCategory_ShouldReturnFilteredResults()
        {
            // Arrange
            var targetNote = NoteBuilder.CreateWithNameAndCategory("Rose Petal", "Floral");
            var otherFloralNote = NoteBuilder.CreateWithNameAndCategory("Jasmine", "Floral");
            var otherRoseNote = NoteBuilder.CreateWithNameAndCategory("Rose Wood", "Woody");
            var notes = new List<ALOud.Models.Note> { targetNote, otherFloralNote, otherRoseNote };
            _context.Notes.AddRange(notes);
            await _context.SaveChangesAsync();

            // Act
            var result = await _noteService.GetAllNotesAsync(searchTerm: "Rose", category: "Floral");

            // Assert
            result.Should().NotBeNull();
            result.Items.Should().HaveCount(1);
            result.Items.First().Name.Should().Be("Rose Petal");
            result.Items.First().Category.Should().Be("Floral");
        }

        [Fact]
        public async Task GetAllNotesAsync_WithNonMatchingSearchTerm_ShouldReturnEmptyResults()
        {
            // Arrange
            var notes = NoteBuilder.CreateValidList(5);
            _context.Notes.AddRange(notes);
            await _context.SaveChangesAsync();

            // Act
            var result = await _noteService.GetAllNotesAsync(searchTerm: "NonExistentNote");

            // Assert
            result.Should().NotBeNull();
            result.Items.Should().BeEmpty();
            result.TotalCount.Should().Be(0);
        }

        [Fact]
        public async Task GetAllNotesAsync_WhenEmpty_ShouldReturnEmptyPaginatedList()
        {
            // Act
            var result = await _noteService.GetAllNotesAsync();

            // Assert
            result.Should().NotBeNull();
            result.Items.Should().BeEmpty();
            result.TotalCount.Should().Be(0);
            result.TotalPages.Should().Be(0);
        }

        #endregion

        #region GetAllNotesForSelectAsync Tests

        [Fact]
        public async Task GetAllNotesForSelectAsync_ShouldReturnAllNotesOrderedByCategoryThenName()
        {
            // Arrange
            var notes = new List<ALOud.Models.Note>
            {
                NoteBuilder.CreateWithNameAndCategory("Vanilla", "Base"),
                NoteBuilder.CreateWithNameAndCategory("Rose", "Floral"),
                NoteBuilder.CreateWithNameAndCategory("Amber", "Base"),
                NoteBuilder.CreateWithNameAndCategory("Jasmine", "Floral")
            };
            _context.Notes.AddRange(notes);
            await _context.SaveChangesAsync();

            // Act
            var result = await _noteService.GetAllNotesForSelectAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(4);
            
            // Should be ordered by category then by name
            result[0].Category.Should().Be("Base");
            result[0].Name.Should().Be("Amber");
            result[1].Category.Should().Be("Base");
            result[1].Name.Should().Be("Vanilla");
            result[2].Category.Should().Be("Floral");
            result[2].Name.Should().Be("Jasmine");
            result[3].Category.Should().Be("Floral");
            result[3].Name.Should().Be("Rose");
        }

        [Fact]
        public async Task GetAllNotesForSelectAsync_WhenEmpty_ShouldReturnEmptyList()
        {
            // Act
            var result = await _noteService.GetAllNotesForSelectAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        #endregion

        #region GetNoteCategoriesAsync Tests

        [Fact]
        public async Task GetNoteCategoriesAsync_ShouldReturnDistinctCategoriesOrderedAlphabetically()
        {
            // Arrange
            var notes = new List<ALOud.Models.Note>
            {
                NoteBuilder.CreateWithCategory("Woody"),
                NoteBuilder.CreateWithCategory("Floral"),
                NoteBuilder.CreateWithCategory("Base"),
                NoteBuilder.CreateWithCategory("Floral"), // Duplicate
                NoteBuilder.CreateWithCategory("Top"),
                NoteBuilder.CreateWithoutCategory() // Should be excluded
            };
            _context.Notes.AddRange(notes);
            await _context.SaveChangesAsync();

            // Act
            var result = await _noteService.GetNoteCategoriesAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(4); // Distinct categories, excluding null
            result.Should().BeInAscendingOrder();
            result.Should().Contain("Base", "Floral", "Top", "Woody");
        }

        [Fact]
        public async Task GetNoteCategoriesAsync_WhenNoNotesHaveCategories_ShouldReturnEmptyList()
        {
            // Arrange
            var notes = new List<ALOud.Models.Note>
            {
                NoteBuilder.CreateWithoutCategory(),
                NoteBuilder.CreateWithoutCategory()
            };
            _context.Notes.AddRange(notes);
            await _context.SaveChangesAsync();

            // Act
            var result = await _noteService.GetNoteCategoriesAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        #endregion

        #region GetNoteByIdAsync Tests

        [Fact]
        public async Task GetNoteByIdAsync_WithValidId_ShouldReturnNote()
        {
            // Arrange
            var note = NoteBuilder.CreateValid();
            _context.Notes.AddRange(new[] { note });
            await _context.SaveChangesAsync();

            // Act
            var result = await _noteService.GetNoteByIdAsync(note.Id);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(note.Id);
            result.Name.Should().Be(note.Name);
            result.Category.Should().Be(note.Category);
            result.Description.Should().Be(note.Description);
            result.PerfumeCount.Should().Be(0); // No perfume notes seeded
        }

        [Fact]
        public async Task GetNoteByIdAsync_WithInvalidId_ShouldReturnNull()
        {
            // Arrange
            var nonExistentId = Guid.NewGuid();

            // Act
            var result = await _noteService.GetNoteByIdAsync(nonExistentId);

            // Assert
            result.Should().BeNull();
        }

        #endregion

        #region GetNoteForEditAsync Tests

        [Fact]
        public async Task GetNoteForEditAsync_WithValidId_ShouldReturnUpdateDto()
        {
            // Arrange
            var note = NoteBuilder.CreateValid();
            _context.Notes.AddRange(new[] { note });
            await _context.SaveChangesAsync();

            // Act
            var result = await _noteService.GetNoteForEditAsync(note.Id);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(note.Id);
            result.Name.Should().Be(note.Name);
            result.Category.Should().Be(note.Category);
            result.Description.Should().Be(note.Description);
        }

        [Fact]
        public async Task GetNoteForEditAsync_WithInvalidId_ShouldReturnNull()
        {
            // Arrange
            var nonExistentId = Guid.NewGuid();

            // Act
            var result = await _noteService.GetNoteForEditAsync(nonExistentId);

            // Assert
            result.Should().BeNull();
        }

        #endregion

        #region CreateNoteAsync Tests

        [Fact]
        public async Task CreateNoteAsync_WithValidDto_ShouldCreateNoteAndReturnId()
        {
            // Arrange
            var createDto = CreateNoteDtoBuilder.CreateValid();

            // Act
            var result = await _noteService.CreateNoteAsync(createDto);

            // Assert
            result.Should().NotBe(Guid.Empty);

            // Verify note was created in database
            var createdNote = await _context.Notes.FindAsync(result);
            createdNote.Should().NotBeNull();
            createdNote!.Name.Should().Be(createDto.Name);
            createdNote.Category.Should().Be(createDto.Category);
            createdNote.Description.Should().Be(createDto.Description);
        }

        [Fact]
        public async Task CreateNoteAsync_WithoutCategoryAndDescription_ShouldCreateNote()
        {
            // Arrange
            var createDto = new CreateNoteDtoBuilder()
                .WithoutCategory()
                .WithoutDescription()
                .Build();

            // Act
            var result = await _noteService.CreateNoteAsync(createDto);

            // Assert
            result.Should().NotBe(Guid.Empty);

            // Verify note was created with null values
            var createdNote = await _context.Notes.FindAsync(result);
            createdNote.Should().NotBeNull();
            createdNote!.Category.Should().BeNull();
            createdNote.Description.Should().BeNull();
        }

        #endregion

        #region UpdateNoteAsync Tests

        [Fact]
        public async Task UpdateNoteAsync_WithValidDto_ShouldUpdateNoteAndReturnTrue()
        {
            // Arrange
            var note = NoteBuilder.CreateValid();
            _context.Notes.AddRange(new[] { note });
            await _context.SaveChangesAsync();

            var updateDto = UpdateNoteDtoBuilder.CreateWithId(note.Id);
            updateDto.Name = "Updated Note Name";
            updateDto.Category = "Updated Category";
            updateDto.Description = "Updated description";

            // Act
            var result = await _noteService.UpdateNoteAsync(updateDto);

            // Assert
            result.Should().BeTrue();

            // Verify note was updated in database
            var updatedNote = await _context.Notes.FindAsync(note.Id);
            updatedNote.Should().NotBeNull();
            updatedNote!.Name.Should().Be("Updated Note Name");
            updatedNote.Category.Should().Be("Updated Category");
            updatedNote.Description.Should().Be("Updated description");
        }

        [Fact]
        public async Task UpdateNoteAsync_WithInvalidId_ShouldReturnFalse()
        {
            // Arrange
            var updateDto = UpdateNoteDtoBuilder.CreateValid();
            updateDto.Id = Guid.NewGuid(); // Non-existent ID

            // Act
            var result = await _noteService.UpdateNoteAsync(updateDto);

            // Assert
            result.Should().BeFalse();
        }

        #endregion

        #region DeleteNoteAsync Tests

        [Fact]
        public async Task DeleteNoteAsync_WithValidId_ShouldDeleteNoteAndReturnTrue()
        {
            // Arrange
            var note = NoteBuilder.CreateValid();
            _context.Notes.AddRange(new[] { note });
            await _context.SaveChangesAsync();

            // Act
            var result = await _noteService.DeleteNoteAsync(note.Id);

            // Assert
            result.Should().BeTrue();

            // Verify note was deleted from database
            var deletedNote = await _context.Notes.FindAsync(note.Id);
            deletedNote.Should().BeNull();
        }

        [Fact]
        public async Task DeleteNoteAsync_WithInvalidId_ShouldReturnFalse()
        {
            // Arrange
            var nonExistentId = Guid.NewGuid();

            // Act
            var result = await _noteService.DeleteNoteAsync(nonExistentId);

            // Assert
            result.Should().BeFalse();
        }

        #endregion

        #region NoteExistsAsync Tests

        [Fact]
        public async Task NoteExistsAsync_WhenNoteExists_ShouldReturnTrue()
        {
            // Arrange
            var note = NoteBuilder.CreateWithName("Rose");
            _context.Notes.AddRange(new[] { note });
            await _context.SaveChangesAsync();

            // Act
            var result = await _noteService.NoteExistsAsync("Rose");

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task NoteExistsAsync_WhenNoteDoesNotExist_ShouldReturnFalse()
        {
            // Arrange
            var note = NoteBuilder.CreateWithName("Rose");
            _context.Notes.AddRange(new[] { note });
            await _context.SaveChangesAsync();

            // Act
            var result = await _noteService.NoteExistsAsync("NonExistent");

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task NoteExistsAsync_WithExcludeId_ShouldIgnoreSpecifiedNote()
        {
            // Arrange
            var note = NoteBuilder.CreateWithName("Rose");
            _context.Notes.AddRange(new[] { note });
            await _context.SaveChangesAsync();

            // Act
            var result = await _noteService.NoteExistsAsync("Rose", note.Id);

            // Assert
            result.Should().BeFalse(); // Should return false because we're excluding this note
        }

        [Fact]
        public async Task NoteExistsAsync_WithExcludeIdButOtherExists_ShouldReturnTrue()
        {
            // Arrange
            var note1 = NoteBuilder.CreateWithName("Rose");
            var note2 = NoteBuilder.CreateWithName("Rose"); // Same name, different ID
            _context.Notes.AddRange(new[] { note1, note2 });
            await _context.SaveChangesAsync();

            // Act
            var result = await _noteService.NoteExistsAsync("Rose", note1.Id);

            // Assert
            result.Should().BeTrue(); // Should return true because note2 still exists
        }

        #endregion

        #region Integration Tests

        [Fact]
        public async Task CompleteWorkflow_CreateUpdateDelete_ShouldWorkCorrectly()
        {
            // Arrange
            var createDto = new CreateNoteDtoBuilder()
                .WithName("Test Note")
                .WithCategory("Test Category")
                .Build();

            // Act & Assert - Create
            var createdId = await _noteService.CreateNoteAsync(createDto);
            createdId.Should().NotBe(Guid.Empty);

            // Act & Assert - Read
            var note = await _noteService.GetNoteByIdAsync(createdId);
            note.Should().NotBeNull();
            note!.Name.Should().Be("Test Note");
            note.Category.Should().Be("Test Category");

            // Act & Assert - Update
            var updateDto = UpdateNoteDtoBuilder.CreateWithId(createdId);
            updateDto.Name = "Updated Test Note";
            updateDto.Category = "Updated Category";
            var updateResult = await _noteService.UpdateNoteAsync(updateDto);
            updateResult.Should().BeTrue();

            // Verify update
            var updatedNote = await _noteService.GetNoteByIdAsync(createdId);
            updatedNote!.Name.Should().Be("Updated Test Note");
            updatedNote.Category.Should().Be("Updated Category");

            // Act & Assert - Delete
            var deleteResult = await _noteService.DeleteNoteAsync(createdId);
            deleteResult.Should().BeTrue();

            // Verify deletion
            var deletedNote = await _noteService.GetNoteByIdAsync(createdId);
            deletedNote.Should().BeNull();
        }

        [Fact]
        public async Task CategoryFiltering_WithMixedCategories_ShouldReturnCorrectCounts()
        {
            // Arrange
            var notes = new List<ALOud.Models.Note>
            {
                NoteBuilder.CreateWithCategory("Floral"),
                NoteBuilder.CreateWithCategory("Floral"),
                NoteBuilder.CreateWithCategory("Woody"),
                NoteBuilder.CreateWithCategory("Fresh"),
                NoteBuilder.CreateWithoutCategory() // null category
            };
            _context.Notes.AddRange(notes);
            await _context.SaveChangesAsync();

            // Act
            var allNotes = await _noteService.GetAllNotesAsync();
            var floralNotes = await _noteService.GetAllNotesAsync(category: "Floral");
            var categories = await _noteService.GetNoteCategoriesAsync();

            // Assert
            allNotes.TotalCount.Should().Be(5);
            floralNotes.TotalCount.Should().Be(2);
            categories.Should().HaveCount(3); // Excludes null category
        }

        #endregion
    }
}