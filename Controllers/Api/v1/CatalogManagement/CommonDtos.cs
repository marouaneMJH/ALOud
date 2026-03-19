namespace ALOud.Controllers.Api.v1.CatalogManagement
{
    /// <summary>
    /// Common DTOs for Catalog Management Controllers
    /// </summary>

    /// <summary>
    /// DTO for validating entity existence
    /// </summary>
    public class ValidateExistsDto
    {
        public string Name { get; set; } = string.Empty;
        public Guid? ExcludeId { get; set; }
    }
}
