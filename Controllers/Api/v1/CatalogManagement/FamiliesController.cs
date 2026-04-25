using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ALOud.Services.Family;
using ALOud.DTOs.Families;

namespace ALOud.Controllers.Api.v1.CatalogManagement
{
    [ApiController]
    [Authorize]
    [Route("api/v1/admin/families")]
    public class FamiliesController : BaseApiController
    {
        private readonly IFamilyService _familyService;
        private readonly ILogger<FamiliesController> _logger;

        public FamiliesController(IFamilyService familyService, ILogger<FamiliesController> logger)
        {
            _familyService = familyService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int pageIndex = 1, [FromQuery] int pageSize = 10, [FromQuery] string? searchTerm = null)
        {
            try
            {
                var result = await _familyService.GetAllFamiliesAsync(pageIndex, pageSize, searchTerm);
                _logger.LogInformation("Families list retrieved");
                return SuccessResponse(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading families");
                return ErrorResponse("Failed to load families", 500);
            }
        }

        [HttpGet("select")]
        public async Task<IActionResult> GetForSelect()
        {
            try
            {
                var result = await _familyService.GetAllFamiliesForSelectAsync();
                return SuccessResponse(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading families for select");
                return ErrorResponse("Failed to load families", 500);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateFamilyDto dto)
        {
            if (!ModelState.IsValid) return ValidationErrorResponse(new Dictionary<string, string[]>());

            try
            {
                if (await _familyService.FamilyExistsAsync(dto.Name))
                    return BadRequest(new { success = false, error = "A family with this name already exists" });

                await _familyService.CreateFamilyAsync(dto);
                _logger.LogInformation("Family created: {FamilyName}", dto.Name);
                return StatusCode(201, new { success = true, message = "Family created successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating family");
                return ErrorResponse("Failed to create family", 500);
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetForEdit(Guid id)
        {
            try
            {
                var result = await _familyService.GetFamilyForEditAsync(id);
                return result == null ? NotFoundResponse() : SuccessResponse(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading family: {FamilyId}", id);
                return ErrorResponse("Failed to load family", 500);
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateFamilyDto dto)
        {
            if (id != dto.Id) return BadRequest(new { success = false, error = "ID mismatch" });
            if (!ModelState.IsValid) return ValidationErrorResponse(new Dictionary<string, string[]>());

            try
            {
                if (await _familyService.FamilyExistsAsync(dto.Name, dto.Id))
                    return BadRequest(new { success = false, error = "A family with this name already exists" });

                var success = await _familyService.UpdateFamilyAsync(dto);
                return success ? SuccessResponse(new { message = "Family updated successfully" }) : NotFoundResponse();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating family: {FamilyId}", id);
                return ErrorResponse("Failed to update family", 500);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var success = await _familyService.DeleteFamilyAsync(id);
                return success ? SuccessResponse(new { message = "Family deleted successfully" }) : NotFoundResponse();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting family: {FamilyId}", id);
                return ErrorResponse("Failed to delete family", 500);
            }
        }

        [HttpPost("validate-exists")]
        public async Task<IActionResult> ValidateExists([FromBody] ValidateExistsDto dto)
        {
            try
            {
                var exists = await _familyService.FamilyExistsAsync(dto.Name, dto.ExcludeId);
                return SuccessResponse(new { exists });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating family");
                return ErrorResponse("Failed to validate", 500);
            }
        }
    }
}
