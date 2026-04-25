using ALOud.Data;
using ALOud.Services.Infrastructure.Cache;
using ALOud.Services.Infrastructure.Jobs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ALOud.Services.Business.Jobs.Processors
{
    public class StockMonitoringJobProcessor : JobProcessor<StockMonitoringJob>
    {
        private readonly ICacheService _cacheService;
        private readonly ALOudDbContext _dbContext;

        public override string JobType => "StockMonitoring";

        public StockMonitoringJobProcessor(
            ILogger<StockMonitoringJobProcessor> logger,
            ICacheService cacheService,
            ALOudDbContext dbContext)
            : base(logger)
        {
            _cacheService = cacheService;
            _dbContext = dbContext;
        }

        public override async Task<JobResult> ProcessAsync(StockMonitoringJob job, CancellationToken cancellationToken = default)
        {
            Logger.LogInformation("Processing stock monitoring job {JobId} for Product {ProductId}, Action: {Action}",
                job.Id, job.ProductId, job.Action);

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            try
            {
                var message = job.Action.ToLower() switch
                {
                    "checkstock" => await CheckStockAsync(job, cancellationToken),
                    "updatestock" => await UpdateStockAsync(job, cancellationToken),
                    "notifylowstock" => await NotifyLowStockAsync(job, cancellationToken),
                    "restocknotification" => await RestockNotificationAsync(job, cancellationToken),
                    _ => $"Unknown action: {job.Action}"
                };

                stopwatch.Stop();

                if (message.StartsWith("Error:") || message.StartsWith("Unknown action:"))
                {
                    return Failure(message, stopwatch.Elapsed);
                }

                return Success(stopwatch.Elapsed, new Dictionary<string, object> { ["message"] = message });
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                Logger.LogError(ex, "Error processing stock monitoring job {JobId} for Product {ProductId}, Action: {Action}",
                    job.Id, job.ProductId, job.Action);
                return Failure($"Error processing job: {ex.Message}", stopwatch.Elapsed);
            }
        }

        private async Task<string> CheckStockAsync(StockMonitoringJob job, CancellationToken cancellationToken)
        {
            Logger.LogInformation("Checking stock for Product {ProductId}", job.ProductId);

            // Mock stock check - in real implementation, this would check actual inventory
            var currentStock = new Random().Next(0, 100);
            var lowStockThreshold = job.Threshold ?? 10;

            Logger.LogInformation("Product {ProductId} current stock: {CurrentStock}, threshold: {Threshold}", 
                job.ProductId, currentStock, lowStockThreshold);

            var stockStatus = DetermineStockStatus(currentStock, lowStockThreshold);

            // Cache stock status
            var cacheKey = $"stock_status:{job.ProductId}";
            await _cacheService.SetAsync(cacheKey, new
            {
                ProductId = job.ProductId,
                CurrentStock = currentStock,
                Threshold = lowStockThreshold,
                Status = stockStatus,
                CheckedAt = DateTime.UtcNow
            }, TimeSpan.FromHours(1));

            if (stockStatus == "Low" || stockStatus == "OutOfStock")
            {
                Logger.LogWarning("Product {ProductId} has {Status} stock: {CurrentStock}", 
                    job.ProductId, stockStatus, currentStock);
            }

            var message = $"Stock check completed: {currentStock} units available (Status: {stockStatus})";
            Logger.LogInformation("Stock check completed for Product {ProductId}: {Message}", job.ProductId, message);
            
            return message;
        }

        private async Task<string> UpdateStockAsync(StockMonitoringJob job, CancellationToken cancellationToken)
        {
            Logger.LogInformation("Updating stock for Product {ProductId}", job.ProductId);

            if (!job.Parameters.TryGetValue("Adjustment", out var adjustmentObj) ||
                !int.TryParse(adjustmentObj?.ToString(), out var adjustment))
            {
                return "Error: Stock adjustment parameter is required";
            }

            // Mock stock update
            await Task.Delay(100, cancellationToken);

            var oldStock = new Random().Next(0, 100);
            var newStock = Math.Max(0, oldStock + adjustment);

            // Clear stock cache
            await _cacheService.RemoveAsync($"stock_status:{job.ProductId}");
            await _cacheService.RemoveAsync($"product:{job.ProductId}");

            var reason = job.Parameters.TryGetValue("Reason", out var reasonObj) 
                ? reasonObj?.ToString() : "Stock monitoring adjustment";

            Logger.LogInformation("Stock updated for Product {ProductId}: {OldStock} -> {NewStock} (Adjustment: {Adjustment}, Reason: {Reason})", 
                job.ProductId, oldStock, newStock, adjustment, reason);

            return $"Stock updated: {oldStock} -> {newStock} (Adjustment: {adjustment:+#;-#;0})";
        }

        private async Task<string> NotifyLowStockAsync(StockMonitoringJob job, CancellationToken cancellationToken)
        {
            Logger.LogInformation("Processing low stock notification for Product {ProductId}", job.ProductId);

            var currentStock = new Random().Next(0, 20);
            var threshold = job.Threshold ?? 10;

            // Check if we should send notification based on last notification time
            var lastNotificationKey = $"last_low_stock_notification:{job.ProductId}";
            var lastNotification = await _cacheService.GetAsync<DateTime?>(lastNotificationKey);
            
            var notificationCooldown = TimeSpan.FromHours(24); // Don't spam notifications
            if (lastNotification.HasValue && DateTime.UtcNow - lastNotification.Value < notificationCooldown)
            {
                return "Notification skipped - still in cooldown period";
            }

            if (currentStock > threshold)
            {
                return "Stock is above threshold - no notification needed";
            }

            // Mock notification implementation
            var notificationMessage = $"LOW STOCK ALERT: Product {job.ProductId} " +
                                    $"has {currentStock} units remaining (Threshold: {threshold})";

            Logger.LogWarning("LOW STOCK NOTIFICATION: {Message}", notificationMessage);

            // Record notification time
            await _cacheService.SetAsync(lastNotificationKey, DateTime.UtcNow, TimeSpan.FromDays(7));

            Logger.LogInformation("Low stock notification sent for Product {ProductId}", job.ProductId);
            return $"Low stock notification sent: {currentStock} units remaining";
        }

        private async Task<string> RestockNotificationAsync(StockMonitoringJob job, CancellationToken cancellationToken)
        {
            Logger.LogInformation("Processing restock notification for Product {ProductId}", job.ProductId);

            var currentStock = new Random().Next(50, 100);
            var restockLevel = job.Parameters.TryGetValue("RestockLevel", out var levelObj) &&
                              int.TryParse(levelObj?.ToString(), out var level) ? level : currentStock;

            // Mock notification implementation
            await Task.Delay(100, cancellationToken);

            var notificationMessage = $"RESTOCK NOTIFICATION: Product {job.ProductId} " +
                                    $"has been restocked to {restockLevel} units";

            Logger.LogInformation("RESTOCK NOTIFICATION: {Message}", notificationMessage);

            // Clear low stock notification cache since we're restocked
            await _cacheService.RemoveAsync($"last_low_stock_notification:{job.ProductId}");

            Logger.LogInformation("Restock notification processed for Product {ProductId}", job.ProductId);
            return $"Restock notification sent: restocked to {restockLevel} units";
        }

        private string DetermineStockStatus(int currentStock, int threshold)
        {
            if (currentStock == 0)
                return "OutOfStock";
            if (currentStock <= threshold)
                return "Low";
            if (currentStock <= threshold * 2)
                return "Normal";
            return "High";
        }
    }
}