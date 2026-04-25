using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ALOud.Services.Brand;
using ALOud.DTOs.Brands;

namespace ALOud.Controllers.Api.v1.CatalogManagement
{
    /// <summary>
    /// REST API Controller for Brand management in admin panel
    /// Converts MVC PerfumeAdminController.Brands* methods to REST endpoints
    /// </summary>
    [ApiController]
    [Authorize]
    [Route("api/v1/admin/brands")]
    public class BrandsController : BaseApiController
    {
        private readonly IBrandService _brandService;
        private readonly ILogger<BrandsController> _logger;
        
        private static readonly string[] BrandNameUniqueError = { "Brand name must be unique" };

        public BrandsController(
            IBrandService brandService,
            ILogger<BrandsController> logger)
        {
            _brandService = brandService;
            _logger = logger;
        }

        /// <summary>
        /// GET /api/v1/admin/brands
        /// Lists all brands with pagination and search
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] int pageIndex = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? searchTerm = null)
        {
            try
            {
                var result = await _brandService.GetAllBrandsAsync(pageIndex, pageSize, searchTerm);
                _logger.LogInformation("Brands list retrieved. Page: {PageIndex}, Search: {SearchTerm}", pageIndex, searchTerm);
                return SuccessResponse(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading brands");
                return ErrorResponse("Failed to load brands", 500);
            }
        }

        /// <summary>
        /// GET /api/v1/admin/brands/select
        /// Returns list of brands for dropdown/select UI
        /// </summary>
        [HttpGet("select")]
        public async Task<IActionResult> GetForSelect()
        {
            try
            {
                var result = await _brandService.GetAllBrandsForSelectAsync();
                _logger.LogInformation("Brands select list retrieved");
                return SuccessResponse(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading brands for select");
                return ErrorResponse("Failed to load brands", 500);
            }
        }

        /// <summary>
        /// POST /api/v1/admin/brands
        /// Creates a new brand
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateBrandDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return ValidationErrorResponse(new Dictionary<string, string[]>
                {
                    { "validation", errors.ToArray() }
                });
            }

            try
            {
                if (await _brandService.BrandExistsAsync(dto.Name))
                {
                    _logger.LogWarning("Attempted to create duplicate brand: {BrandName}", dto.Name);
                    return BadRequest(new
                    {
                        success = false,
                        error = "A brand with this name already exists",
                        validationErrors = new { Name = BrandNameUniqueError }
                    });
                }

                await _brandService.CreateBrandAsync(dto);
                _logger.LogInformation("Brand created successfully: {BrandName}", dto.Name);
                return StatusCode(201, new { success = true, message = "Brand created successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating brand: {BrandName}", dto.Name);
                return ErrorResponse("Failed to create brand", 500);
            }
        }

        /// <summary>
        /// GET /api/v1/admin/brands/{id}
        /// Gets brand for editing
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetForEdit(Guid id)
        {
            try
            {
                var result = await _brandService.GetBrandForEditAsync(id);
                if (result == null)
                {
                    _logger.LogWarning("Brand not found for edit: {BrandId}", id);
                    return NotFoundResponse($"Brand with ID {id} not found");
                }
                return SuccessResponse(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading brand for edit: {BrandId}", id);
                return ErrorResponse("Failed to load brand", 500);
            }
        }

        /// <summary>
        /// PUT /api/v1/admin/brands/{id}
        /// Updates an existing brand
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateBrandDto dto)
        {
            if (id != dto.Id)
            {
                _logger.LogWarning("Route ID does not match DTO ID: {RouteId} vs {DtoId}", id, dto.Id);
                return BadRequest(new { success = false, error = "Route ID does not match DTO ID" });
            }

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return ValidationErrorResponse(new Dictionary<string, string[]>
                {
                    { "validation", errors.ToArray() }
                });
            }

            try
            {
                if (await _brandService.BrandExistsAsync(dto.Name, dto.Id))
                {
                    _logger.LogWarning("Attempted to update brand with duplicate name: {BrandName}", dto.Name);
                    return BadRequest(new
                    {
                        success = false,
                        error = "A brand with this name already exists",
                        validationErrors = new { Name = BrandNameUniqueError }
                    });
                }

                var success = await _brandService.UpdateBrandAsync(dto);
                if (!success)
                {
                    _logger.LogWarning("Brand not found for update: {BrandId}", id);
                    return NotFoundResponse($"Brand with ID {id} not found");
                }

                _logger.LogInformation("Brand updated successfully: {BrandId}", id);
                return SuccessResponse(new { message = "Brand updated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating brand: {BrandId}", id);
                return ErrorResponse("Failed to update brand", 500);
            }
        }

        /// <summary>
        /// DELETE /api/v1/admin/brands/{id}
        /// Deletes a brand
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var success = await _brandService.DeleteBrandAsync(id);
                if (!success)
                {
                    _logger.LogWarning("Brand not found for deletion: {BrandId}", id);
                    return NotFoundResponse($"Brand with ID {id} not found");
                }

                _logger.LogInformation("Brand deleted successfully: {BrandId}", id);
                return SuccessResponse(new { message = "Brand deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting brand: {BrandId}", id);
                return ErrorResponse("Failed to delete brand", 500);
            }
        }

        /// <summary>
        /// POST /api/v1/admin/brands/validate-exists
        /// Validates if a brand name already exists
        /// </summary>
        [HttpPost("validate-exists")]
        public async Task<IActionResult> ValidateExists([FromBody] ValidateExistsDto dto)
        {
            try
            {
                var exists = await _brandService.BrandExistsAsync(dto.Name, dto.ExcludeId);
                return SuccessResponse(new { exists });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating brand existence");
                return ErrorResponse("Failed to validate brand", 500);
            }
        }
    }
}
