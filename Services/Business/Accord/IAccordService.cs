using ALOud.DTOs.Accords;
using ViewModels;

namespace ALOud.Services.Accord
{
    public interface IAccordService
    {
        Task<PaginatedList<AccordDto>> GetAllAccordsAsync(int pageIndex = 1, int pageSize = 10, string? searchTerm = null);
        Task<List<AccordSelectDto>> GetAllAccordsForSelectAsync();
        Task<AccordDto?> GetAccordByIdAsync(Guid id);
        Task<UpdateAccordDto?> GetAccordForEditAsync(Guid id);
        Task<Guid> CreateAccordAsync(CreateAccordDto dto);
        Task<bool> UpdateAccordAsync(UpdateAccordDto dto);
        Task<bool> DeleteAccordAsync(Guid id);
        Task<bool> AccordExistsAsync(string name, Guid? excludeId = null);
    }
}
