using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ALOud.Services.Note;
using ALOud.DTOs.Notes;

namespace ALOud.Controllers.Api.v1.CatalogManagement
{
    /// <summary>
    /// REST API Controller for Note management with category support
    /// More complex than other catalog controllers due to category filtering
    /// </summary>
    [ApiController]
    [Authorize]
    [Route("api/v1/admin/notes")]
    public class NotesController : BaseApiController
    {
        private readonly INoteService _noteService;
        private readonly ILogger<NotesController> _logger;

        public NotesController(INoteService noteService, ILogger<NotesController> logger)
        {
            _noteService = noteService;
            _logger = logger;
        }

        /// <summary>
        /// GET /api/v1/admin/notes
        /// Lists all notes with pagination, search, and category filtering
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] int pageIndex = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? searchTerm = null,
            [FromQuery] string? category = null)
        {
            try
            {
                var result = await _noteService.GetAllNotesAsync(pageIndex, pageSize, searchTerm, category);
                _logger.LogInformation("Notes list retrieved. Category: {Category}", category);
                return SuccessResponse(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading notes");
                return ErrorResponse("Failed to load notes", 500);
            }
        }

        /// <summary>
        /// GET /api/v1/admin/notes/categories
        /// Returns list of available note categories
        /// Unique to NotesController - other resources don't have this
        /// </summary>
        [HttpGet("categories")]
        public async Task<IActionResult> GetCategories()
        {
            try
            {
                var categories = await _noteService.GetNoteCategoriesAsync();
                _logger.LogInformation("Note categories retrieved");
                return SuccessResponse(new { categories });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading note categories");
                return ErrorResponse("Failed to load categories", 500);
            }
        }

        /// <summary>
        /// GET /api/v1/admin/notes/select
        /// Returns list of notes for dropdown/select UI
        /// </summary>
        [HttpGet("select")]
        public async Task<IActionResult> GetForSelect()
        {
            try
            {
                var result = await _noteService.GetAllNotesForSelectAsync();
                return SuccessResponse(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading notes for select");
                return ErrorResponse("Failed to load notes", 500);
            }
        }

        /// <summary>
        /// POST /api/v1/admin/notes
        /// Creates a new note
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateNoteDto dto)
        {
            if (!ModelState.IsValid) return ValidationErrorResponse(new Dictionary<string, string[]>());

            try
            {
                if (await _noteService.NoteExistsAsync(dto.Name))
                {
                    _logger.LogWarning("Attempted to create duplicate note: {NoteName}", dto.Name);
                    return BadRequest(new { success = false, error = "A note with this name already exists" });
                }

                await _noteService.CreateNoteAsync(dto);
                _logger.LogInformation("Note created: {NoteName}", dto.Name);
                return StatusCode(201, new { success = true, message = "Note created successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating note");
                return ErrorResponse("Failed to create note", 500);
            }
        }

        /// <summary>
        /// GET /api/v1/admin/notes/{id}
        /// Gets note for editing
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetForEdit(Guid id)
        {
            try
            {
                var result = await _noteService.GetNoteForEditAsync(id);
                return result == null ? NotFoundResponse() : SuccessResponse(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading note: {NoteId}", id);
                return ErrorResponse("Failed to load note", 500);
            }
        }

        /// <summary>
        /// PUT /api/v1/admin/notes/{id}
        /// Updates an existing note
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateNoteDto dto)
        {
            if (id != dto.Id) return BadRequest(new { success = false, error = "ID mismatch" });
            if (!ModelState.IsValid) return ValidationErrorResponse(new Dictionary<string, string[]>());

            try
            {
                if (await _noteService.NoteExistsAsync(dto.Name, dto.Id))
                {
                    _logger.LogWarning("Attempted to update note with duplicate name: {NoteName}", dto.Name);
                    return BadRequest(new { success = false, error = "A note with this name already exists" });
                }

                var success = await _noteService.UpdateNoteAsync(dto);
                return success ? SuccessResponse(new { message = "Note updated successfully" }) : NotFoundResponse();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating note: {NoteId}", id);
                return ErrorResponse("Failed to update note", 500);
            }
        }

        /// <summary>
        /// DELETE /api/v1/admin/notes/{id}
        /// Deletes a note
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var success = await _noteService.DeleteNoteAsync(id);
                return success ? SuccessResponse(new { message = "Note deleted successfully" }) : NotFoundResponse();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting note: {NoteId}", id);
                return ErrorResponse("Failed to delete note", 500);
            }
        }

        /// <summary>
        /// POST /api/v1/admin/notes/validate-exists
        /// Validates if a note name already exists
        /// </summary>
        [HttpPost("validate-exists")]
        public async Task<IActionResult> ValidateExists([FromBody] ValidateExistsDto dto)
        {
            try
            {
                var exists = await _noteService.NoteExistsAsync(dto.Name, dto.ExcludeId);
                return SuccessResponse(new { exists });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating note");
                return ErrorResponse("Failed to validate", 500);
            }
        }
    }
}
