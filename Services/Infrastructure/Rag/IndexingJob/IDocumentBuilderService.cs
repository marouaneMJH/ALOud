using ALOud.Services.Infrastructure.Rag.Models;

namespace ALOud.Services.Infrastructure.Rag.IndexingJob;


public interface IDocumentBuilderService
{
    string BuildDocument(PerfumeRagSource perfume);
}
