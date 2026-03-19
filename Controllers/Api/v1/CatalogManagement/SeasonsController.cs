using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ALOud.Services.Season;
using ALOud.DTOs.Seasons;

namespace ALOud.Controllers.Api.v1.CatalogManagement
{
    [ApiController]
    [Authorize]
    [Route("api/v1/admin/seasons")]
    public class SeasonsController : BaseApiController
    {
        private readonly ISeasonService _seasonService;
        private readonly ILogger<SeasonsController> _logger;

        public SeasonsController(ISeasonService seasonService, ILogger<SeasonsController> logger)
        {
            _seasonService = seasonService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int pageIndex = 1, [FromQuery] int pageSize = 10, [FromQuery] string? searchTerm = null)
        {
            try
            {
                var result = await _seasonService.GetAllSeasonsAsync(pageIndex, pageSize, searchTerm);
                return SuccessResponse(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading seasons");
                return ErrorResponse("Failed to load seasons", 500);
            }
        }

        [HttpGet("select")]
        public async Task<IActionResult> GetForSelect()
        {
            try
            {
                var result = await _seasonService.GetAllSeasonsForSelectAsync();
                return SuccessResponse(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading seasons");
                return ErrorResponse("Failed to load seasons", 500);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateSeasonDto dto)
        {
            if (!ModelState.IsValid) return ValidationErrorResponse(new Dictionary<string, string[]>());

            try
            {
                if (await _seasonService.SeasonExistsAsync(dto.Name))
                    return BadRequest(new { success = false, error = "A season with this name already exists" });

                await _seasonService.CreateSeasonAsync(dto);
                return StatusCode(201, new { success = true, message = "Season created successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating season");
                return ErrorResponse("Failed to create season", 500);
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetForEdit(Guid id)
        {
            try
            {
                var result = await _seasonService.GetSeasonForEditAsync(id);
                return result == null ? NotFoundResponse() : SuccessResponse(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading season");
                return ErrorResponse("Failed to load season", 500);
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateSeasonDto dto)
        {
            if (id != dto.Id) return BadRequest(new { success = false, error = "ID mismatch" });
            if (!ModelState.IsValid) return ValidationErrorResponse(new Dictionary<string, string[]>());

            try
            {
                if (await _seasonService.SeasonExistsAsync(dto.Name, dto.Id))
                    return BadRequest(new { success = false, error = "A season with this name already exists" });

                var success = await _seasonService.UpdateSeasonAsync(dto);
                return success ? SuccessResponse(new { message = "Season updated successfully" }) : NotFoundResponse();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating season");
                return ErrorResponse("Failed to update season", 500);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var success = await _seasonService.DeleteSeasonAsync(id);
                return success ? SuccessResponse(new { message = "Season deleted successfully" }) : NotFoundResponse();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting season");
                return ErrorResponse("Failed to delete season", 500);
            }
        }

        [HttpPost("validate-exists")]
        public async Task<IActionResult> ValidateExists([FromBody] ValidateExistsDto dto)
        {
            try
            {
                var exists = await _seasonService.SeasonExistsAsync(dto.Name, dto.ExcludeId);
                return SuccessResponse(new { exists });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating season");
                return ErrorResponse("Failed to validate", 500);
            }
        }
    }
}
