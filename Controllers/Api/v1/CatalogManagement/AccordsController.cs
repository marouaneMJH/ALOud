using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ALOud.Services.Accord;
using ALOud.DTOs.Accords;

namespace ALOud.Controllers.Api.v1.CatalogManagement
{
    [ApiController]
    [Authorize]
    [Route("api/v1/admin/accords")]
    public class AccordsController : BaseApiController
    {
        private readonly IAccordService _accordService;
        private readonly ILogger<AccordsController> _logger;

        public AccordsController(IAccordService accordService, ILogger<AccordsController> logger)
        {
            _accordService = accordService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int pageIndex = 1, [FromQuery] int pageSize = 10, [FromQuery] string? searchTerm = null)
        {
            try
            {
                var result = await _accordService.GetAllAccordsAsync(pageIndex, pageSize, searchTerm);
                return SuccessResponse(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading accords");
                return ErrorResponse("Failed to load accords", 500);
            }
        }

        [HttpGet("select")]
        public async Task<IActionResult> GetForSelect()
        {
            try
            {
                var result = await _accordService.GetAllAccordsForSelectAsync();
                return SuccessResponse(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading accords");
                return ErrorResponse("Failed to load accords", 500);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateAccordDto dto)
        {
            if (!ModelState.IsValid) return ValidationErrorResponse(new Dictionary<string, string[]>());

            try
            {
                if (await _accordService.AccordExistsAsync(dto.Name))
                    return BadRequest(new { success = false, error = "An accord with this name already exists" });

                await _accordService.CreateAccordAsync(dto);
                return StatusCode(201, new { success = true, message = "Accord created successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating accord");
                return ErrorResponse("Failed to create accord", 500);
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetForEdit(Guid id)
        {
            try
            {
                var result = await _accordService.GetAccordForEditAsync(id);
                return result == null ? NotFoundResponse() : SuccessResponse(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading accord");
                return ErrorResponse("Failed to load accord", 500);
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateAccordDto dto)
        {
            if (id != dto.Id) return BadRequest(new { success = false, error = "ID mismatch" });
            if (!ModelState.IsValid) return ValidationErrorResponse(new Dictionary<string, string[]>());

            try
            {
                if (await _accordService.AccordExistsAsync(dto.Name, dto.Id))
                    return BadRequest(new { success = false, error = "An accord with this name already exists" });

                var success = await _accordService.UpdateAccordAsync(dto);
                return success ? SuccessResponse(new { message = "Accord updated successfully" }) : NotFoundResponse();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating accord");
                return ErrorResponse("Failed to update accord", 500);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var success = await _accordService.DeleteAccordAsync(id);
                return success ? SuccessResponse(new { message = "Accord deleted successfully" }) : NotFoundResponse();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting accord");
                return ErrorResponse("Failed to delete accord", 500);
            }
        }

        [HttpPost("validate-exists")]
        public async Task<IActionResult> ValidateExists([FromBody] ValidateExistsDto dto)
        {
            try
            {
                var exists = await _accordService.AccordExistsAsync(dto.Name, dto.ExcludeId);
                return SuccessResponse(new { exists });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating accord");
                return ErrorResponse("Failed to validate", 500);
            }
        }
    }
}
