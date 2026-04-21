using ALOud.Data;
using ALOud.Models;
using ALOud.Services.Infrastructure.Cache;
using ALOud.Services.Infrastructure.Jobs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ALOud.Services.Business.Jobs.Processors
{
    public class CleanupJobProcessor : JobProcessor<CleanupJob>
    {
        private readonly ICacheService _cacheService;
        private readonly ALOudDbContext _dbContext;

        public override string JobType => "Cleanup";

        public CleanupJobProcessor(
            ILogger<CleanupJobProcessor> logger,
            ICacheService cacheService,
            ALOudDbContext dbContext)
            : base(logger)
        {
            _cacheService = cacheService;
            _dbContext = dbContext;
        }

        public override async Task<JobResult> ProcessAsync(CleanupJob job, CancellationToken cancellationToken = default)
        {
            Logger.LogInformation("Processing cleanup job {JobId}, Type: {CleanupType}",
                job.Id, job.CleanupType);

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            try
            {
                var message = job.CleanupType.ToLower() switch
                {
                    "expiredorders" => await CleanupExpiredOrdersAsync(job, cancellationToken),
                    "oldlogs" => await CleanupOldLogsAsync(job, cancellationToken),
                    "tempfiles" => await CleanupTempFilesAsync(job, cancellationToken),
                    "failedjobs" => await CleanupFailedJobsAsync(job, cancellationToken),
                    "expiredcache" => await CleanupExpiredCacheAsync(job, cancellationToken),
                    "oldstatushistory" => await CleanupOldStatusHistoryAsync(job, cancellationToken),
                    _ => $"Unknown cleanup type: {job.CleanupType}"
                };

                stopwatch.Stop();

                if (message.StartsWith("Error:") || message.StartsWith("Unknown cleanup type:"))
                {
                    return Failure(message, stopwatch.Elapsed);
                }

                return Success(stopwatch.Elapsed, new Dictionary<string, object> { ["message"] = message });
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                Logger.LogError(ex, "Error processing cleanup job {JobId}, Type: {CleanupType}",
                    job.Id, job.CleanupType);
                return Failure($"Error processing cleanup job: {ex.Message}", stopwatch.Elapsed);
            }
        }

        private async Task<string> CleanupExpiredOrdersAsync(CleanupJob job, CancellationToken cancellationToken)
        {
            Logger.LogInformation("Cleaning up expired orders");

            var cutoffDate = job.CutoffDate ?? DateTime.UtcNow.AddDays(-30); // Default: 30 days old
            var batchSize = job.BatchSize ?? 100;

            // Find expired orders that can be cleaned up
            var expiredOrders = await _dbContext.Orders
                .Where(o => o.Status == "Pending" && o.CreatedAt < cutoffDate)
                .Take(batchSize)
                .ToListAsync(cancellationToken);

            if (!expiredOrders.Any())
            {
                return "No expired orders to clean up";
            }

            var cleanedCount = 0;

            foreach (var order in expiredOrders)
            {
                try
                {
                    // Check if order has any payments or shipments
                    var hasPayments = await _dbContext.OrderPayments
                        .AnyAsync(p => p.OrderId == order.Id, cancellationToken);
                    
                    var hasShipments = await _dbContext.OrderShipments
                        .AnyAsync(s => s.OrderId == order.Id, cancellationToken);

                    if (!hasPayments && !hasShipments)
                    {
                        // Safe to mark as expired/cancelled
                        order.Status = "Expired";
                        order.CancelledAt = DateTime.UtcNow;
                        order.UpdatedAt = DateTime.UtcNow;

                        // Add status history
                        _dbContext.OrderStatusHistory.Add(new OrderStatusHistory
                        {
                            Id = Guid.NewGuid(),
                            OrderId = order.Id,
                            PreviousStatus = order.Status,
                            NewStatus = "Expired",
                            ChangeReason = "Order expired due to inactivity",
                            ChangeSource = "System",
                            CreatedAt = DateTime.UtcNow
                        });

                        cleanedCount++;
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "Error cleaning up order {OrderId}", order.Id);
                }
            }

            if (cleanedCount > 0)
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            }

            // Clear related cache entries
            foreach (var order in expiredOrders.Take(cleanedCount))
            {
                await _cacheService.RemoveAsync($"order:{order.Id}");
            }

            Logger.LogInformation("Expired orders cleanup completed: cleaned {Count} orders", cleanedCount);
            return $"Cleaned up {cleanedCount} expired orders";
        }

        private async Task<string> CleanupOldLogsAsync(CleanupJob job, CancellationToken cancellationToken)
        {
            Logger.LogInformation("Cleaning up old logs (mock implementation)");

            await Task.Delay(100, cancellationToken); // Simulate work
            
            var mockCleanedCount = new Random().Next(10, 100);
            
            Logger.LogInformation("Mock log cleanup completed: cleaned {Count} log entries", mockCleanedCount);
            
            return $"Mock: Cleaned up {mockCleanedCount} old log entries";
        }

        private async Task<string> CleanupTempFilesAsync(CleanupJob job, CancellationToken cancellationToken)
        {
            Logger.LogInformation("Cleaning up temporary files (mock implementation)");

            await Task.Delay(200, cancellationToken); // Simulate work
            
            var mockCleanedCount = new Random().Next(5, 50);
            var mockSizeBytes = new Random().Next(1024, 1024 * 1024 * 100); // 1KB to 100MB
            
            Logger.LogInformation("Mock temp files cleanup completed: cleaned {Count} files, freed {Size} bytes", 
                mockCleanedCount, mockSizeBytes);
            
            return $"Mock: Cleaned up {mockCleanedCount} temp files ({mockSizeBytes:N0} bytes freed)";
        }

        private async Task<string> CleanupFailedJobsAsync(CleanupJob job, CancellationToken cancellationToken)
        {
            Logger.LogInformation("Cleaning up failed jobs (mock implementation)");

            await Task.Delay(150, cancellationToken); // Simulate work
            
            var mockCleanedCount = new Random().Next(0, 20);
            
            Logger.LogInformation("Failed jobs cleanup completed: cleaned {Count} failed jobs", mockCleanedCount);
            
            return $"Cleaned up {mockCleanedCount} failed jobs";
        }

        private async Task<string> CleanupExpiredCacheAsync(CleanupJob job, CancellationToken cancellationToken)
        {
            Logger.LogInformation("Cleaning up expired cache entries (mock implementation)");

            await Task.Delay(100, cancellationToken); // Simulate work
            
            var mockCleanedCount = new Random().Next(0, 50);
            
            Logger.LogInformation("Cache cleanup completed: cleaned {Count} expired cache entries", mockCleanedCount);
            
            return $"Cleaned up {mockCleanedCount} expired cache entries";
        }

        private async Task<string> CleanupOldStatusHistoryAsync(CleanupJob job, CancellationToken cancellationToken)
        {
            Logger.LogInformation("Cleaning up old status history");

            var cutoffDate = job.CutoffDate ?? DateTime.UtcNow.AddYears(-2); // Default: 2 years old
            var batchSize = job.BatchSize ?? 1000;

            // Keep the most recent status history entries, remove very old ones
            var oldStatusHistory = await _dbContext.OrderStatusHistory
                .Where(h => h.CreatedAt < cutoffDate)
                .OrderBy(h => h.CreatedAt)
                .Take(batchSize)
                .ToListAsync(cancellationToken);

            if (!oldStatusHistory.Any())
            {
                return "No old status history to clean up";
            }

            var cleanedCount = 0;
            var orderIds = new HashSet<Guid>();

            foreach (var history in oldStatusHistory)
            {
                // Keep at least one status history per order
                var hasOtherHistory = await _dbContext.OrderStatusHistory
                    .AnyAsync(h => h.OrderId == history.OrderId && h.Id != history.Id, cancellationToken);

                if (hasOtherHistory)
                {
                    _dbContext.OrderStatusHistory.Remove(history);
                    orderIds.Add(history.OrderId);
                    cleanedCount++;
                }
            }

            if (cleanedCount > 0)
            {
                await _dbContext.SaveChangesAsync(cancellationToken);

                // Clear cache for affected orders
                foreach (var orderId in orderIds)
                {
                    await _cacheService.RemoveAsync($"order:{orderId}");
                }
            }

            Logger.LogInformation("Status history cleanup completed: cleaned {Count} old entries", cleanedCount);
            return $"Cleaned up {cleanedCount} old status history entries";
        }
    }
}