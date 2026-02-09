public interface IProductDataExtractor
{
    Task<IReadOnlyList<PerfumeRagSource>> ExtractAllAsync(
        CancellationToken cancellationToken = default
    );
}
