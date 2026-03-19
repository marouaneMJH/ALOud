using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ALOud.Services.Tag;
using ALOud.DTOs.Tags;

namespace ALOud.Controllers.Api.v1.CatalogManagement
{
    [ApiController]
    [Authorize]
    [Route("api/v1/admin/tags")]
    public class TagsController : BaseApiController
    {
        private readonly ITagService _tagService;
        private readonly ILogger<TagsController> _logger;

        public TagsController(ITagService tagService, ILogger<TagsController> logger)
        {
            _tagService = tagService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int pageIndex = 1, [FromQuery] int pageSize = 10, [FromQuery] string? searchTerm = null)
        {
            try
            {
                var result = await _tagService.GetAllTagsAsync(pageIndex, pageSize, searchTerm);
                return SuccessResponse(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading tags");
                return ErrorResponse("Failed to load tags", 500);
            }
        }

        [HttpGet("select")]
        public async Task<IActionResult> GetForSelect()
        {
            try
            {
                var result = await _tagService.GetAllTagsForSelectAsync();
                return SuccessResponse(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading tags for select");
                return ErrorResponse("Failed to load tags", 500);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateTagDto dto)
        {
            if (!ModelState.IsValid) return ValidationErrorResponse(new Dictionary<string, string[]>());

            try
            {
                if (await _tagService.TagExistsAsync(dto.Name))
                    return BadRequest(new { success = false, error = "A tag with this name already exists" });

                await _tagService.CreateTagAsync(dto);
                return StatusCode(201, new { success = true, message = "Tag created successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating tag");
                return ErrorResponse("Failed to create tag", 500);
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetForEdit(Guid id)
        {
            try
            {
                var result = await _tagService.GetTagForEditAsync(id);
                return result == null ? NotFoundResponse() : SuccessResponse(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading tag: {TagId}", id);
                return ErrorResponse("Failed to load tag", 500);
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTagDto dto)
        {
            if (id != dto.Id) return BadRequest(new { success = false, error = "ID mismatch" });
            if (!ModelState.IsValid) return ValidationErrorResponse(new Dictionary<string, string[]>());

            try
            {
                if (await _tagService.TagExistsAsync(dto.Name, dto.Id))
                    return BadRequest(new { success = false, error = "A tag with this name already exists" });

                var success = await _tagService.UpdateTagAsync(dto);
                return success ? SuccessResponse(new { message = "Tag updated successfully" }) : NotFoundResponse();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating tag: {TagId}", id);
                return ErrorResponse("Failed to update tag", 500);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var success = await _tagService.DeleteTagAsync(id);
                return success ? SuccessResponse(new { message = "Tag deleted successfully" }) : NotFoundResponse();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting tag: {TagId}", id);
                return ErrorResponse("Failed to delete tag", 500);
            }
        }

        [HttpPost("validate-exists")]
        public async Task<IActionResult> ValidateExists([FromBody] ValidateExistsDto dto)
        {
            try
            {
                var exists = await _tagService.TagExistsAsync(dto.Name, dto.ExcludeId);
                return SuccessResponse(new { exists });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating tag");
                return ErrorResponse("Failed to validate", 500);
            }
        }
    }
}
