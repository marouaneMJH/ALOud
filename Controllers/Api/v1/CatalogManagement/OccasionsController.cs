using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ALOud.Services.Occasion;
using ALOud.DTOs.Occasions;

namespace ALOud.Controllers.Api.v1.CatalogManagement
{
    [ApiController]
    [Authorize]
    [Route("api/v1/admin/occasions")]
    public class OccasionsController : BaseApiController
    {
        private readonly IOccasionService _occasionService;
        private readonly ILogger<OccasionsController> _logger;

        public OccasionsController(IOccasionService occasionService, ILogger<OccasionsController> logger)
        {
            _occasionService = occasionService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int pageIndex = 1, [FromQuery] int pageSize = 10, [FromQuery] string? searchTerm = null)
        {
            try
            {
                var result = await _occasionService.GetAllOccasionsAsync(pageIndex, pageSize, searchTerm);
                return SuccessResponse(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading occasions");
                return ErrorResponse("Failed to load occasions", 500);
            }
        }

        [HttpGet("select")]
        public async Task<IActionResult> GetForSelect()
        {
            try
            {
                var result = await _occasionService.GetAllOccasionsForSelectAsync();
                return SuccessResponse(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading occasions");
                return ErrorResponse("Failed to load occasions", 500);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateOccasionDto dto)
        {
            if (!ModelState.IsValid) return ValidationErrorResponse(new Dictionary<string, string[]>());

            try
            {
                if (await _occasionService.OccasionExistsAsync(dto.Name))
                    return BadRequest(new { success = false, error = "An occasion with this name already exists" });

                await _occasionService.CreateOccasionAsync(dto);
                return StatusCode(201, new { success = true, message = "Occasion created successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating occasion");
                return ErrorResponse("Failed to create occasion", 500);
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetForEdit(Guid id)
        {
            try
            {
                var result = await _occasionService.GetOccasionForEditAsync(id);
                return result == null ? NotFoundResponse() : SuccessResponse(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading occasion");
                return ErrorResponse("Failed to load occasion", 500);
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateOccasionDto dto)
        {
            if (id != dto.Id) return BadRequest(new { success = false, error = "ID mismatch" });
            if (!ModelState.IsValid) return ValidationErrorResponse(new Dictionary<string, string[]>());

            try
            {
                if (await _occasionService.OccasionExistsAsync(dto.Name, dto.Id))
                    return BadRequest(new { success = false, error = "An occasion with this name already exists" });

                var success = await _occasionService.UpdateOccasionAsync(dto);
                return success ? SuccessResponse(new { message = "Occasion updated successfully" }) : NotFoundResponse();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating occasion");
                return ErrorResponse("Failed to update occasion", 500);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var success = await _occasionService.DeleteOccasionAsync(id);
                return success ? SuccessResponse(new { message = "Occasion deleted successfully" }) : NotFoundResponse();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting occasion");
                return ErrorResponse("Failed to delete occasion", 500);
            }
        }

        [HttpPost("validate-exists")]
        public async Task<IActionResult> ValidateExists([FromBody] ValidateExistsDto dto)
        {
            try
            {
                var exists = await _occasionService.OccasionExistsAsync(dto.Name, dto.ExcludeId);
                return SuccessResponse(new { exists });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating occasion");
                return ErrorResponse("Failed to validate", 500);
            }
        }
    }
}
