using Microsoft.Extensions.Hosting;
using ALOud.Services.Rag.Models;
using static ALOud.Models.JobsConfig;

namespace ALOud.Services.Rag.IndexingJob;

public sealed class RagIndexingHostedService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<RagIndexingHostedService> _logger;
    private readonly IndexingJobConfig _config;

    public RagIndexingHostedService(
        IServiceProvider serviceProvider,
        ILogger<RagIndexingHostedService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;

        _config = new IndexingJobConfig();
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        if (!_config.OnStartIndexing)
            return; // exit the function


        _logger.LogInformation("RAG indexing job started");
        // Delay a bit to let app +Qdrant fully start
        await Task.Delay(TimeSpan.FromSeconds(_config.DelaySeconds), stoppingToken);

        try
        {
            using var scope = _serviceProvider.CreateScope();

            var extractor = scope.ServiceProvider.GetRequiredService<IProductDataExtractor>();
            var docBuilder = scope.ServiceProvider.GetRequiredService<IDocumentBuilderService>();
            var chunker = scope.ServiceProvider.GetRequiredService<IChunkingService>();
            var embedder = scope.ServiceProvider.GetRequiredService<IEmbeddingIndexService>();
            var vectorIndex = scope.ServiceProvider.GetRequiredService<IVectorIndexService>();

            // 1. Extract from SQL
            var perfumes = await extractor.ExtractAllAsync(stoppingToken);
            _logger.LogInformation("Extracted {Count} perfumes from database", perfumes.Count);

            foreach (var perfume in perfumes)
            {
                stoppingToken.ThrowIfCancellationRequested();

                // 2. Delete old vectors (re-index safety)
                await vectorIndex.DeleteBySourceIdAsync(perfume.Id, stoppingToken);

                // 3. Build document
                var document = docBuilder.BuildDocument(perfume);

                // 4. Chunk document
                var chunks = chunker.Chunk(perfume, document);
                if (chunks.Count == 0)
                    continue;

                // 5. Embed chunks
                var vectors = await embedder.EmbedAsync(chunks, stoppingToken);

                // 6. Store in Qdrant
                await vectorIndex.UpsertAsync(vectors, stoppingToken);

                _logger.LogInformation(
                    "Indexed perfume '{Name}' ({Chunks} chunks)",
                    perfume.Name,
                    chunks.Count
                );
            }

            _logger.LogInformation("RAG indexing job completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "RAG indexing job failed");
        }
    }
}
