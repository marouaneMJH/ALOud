using ALOud.Services.Rag.Models;

public interface IDocumentBuilderService
{
    string BuildDocument(PerfumeRagSource perfume);
}
