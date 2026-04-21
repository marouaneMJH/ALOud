using ALOud.Services.Infrastructure.Jobs;

namespace ALOud.Services.Business.Jobs
{
    /// <summary>
    /// Job for processing new orders through the complete order lifecycle
    /// </summary>
    public class OrderProcessingJob : BaseJob
    {
        public override string JobType => "OrderProcessing";
        
        public Guid OrderId { get; set; }
        public string Action { get; set; } = string.Empty; // "ProcessPayment", "CreateShipment", "SendNotification"
        public Dictionary<string, object> Parameters { get; set; } = new();

        public OrderProcessingJob() { }

        public OrderProcessingJob(Guid orderId, string action, Dictionary<string, object>? parameters = null)
        {
            OrderId = orderId;
            Action = action;
            Parameters = parameters ?? new Dictionary<string, object>();
        }
    }

    /// <summary>
    /// Job for synchronizing order and payment status with external services
    /// </summary>
    public class StatusSyncJob : BaseJob
    {
        public override string JobType => "StatusSync";
        
        public Guid OrderId { get; set; }
        public string EntityType { get; set; } = string.Empty; // "Order", "Payment", "Shipment"
        public string EntityId { get; set; } = string.Empty;
        public string SyncType { get; set; } = string.Empty; // "PaymentStatus", "ShipmentTracking", "OrderStatus"
        public DateTime? LastSyncAt { get; set; }
        public Dictionary<string, object> JobMetadata { get; set; } = new();

        public StatusSyncJob() { }

        public StatusSyncJob(Guid orderId, string entityType, string entityId, string syncType, Dictionary<string, object>? metadata = null)
        {
            OrderId = orderId;
            EntityType = entityType;
            EntityId = entityId;
            SyncType = syncType;
            JobMetadata = metadata ?? new Dictionary<string, object>();
        }
    }

    /// <summary>
    /// Job for monitoring inventory levels and updating stock status
    /// </summary>
    public class StockMonitoringJob : BaseJob
    {
        public override string JobType => "StockMonitoring";
        
        public Guid ProductId { get; set; }
        public string Action { get; set; } = string.Empty; // "CheckStock", "UpdateStock", "NotifyLowStock"
        public int? Threshold { get; set; }
        public Dictionary<string, object> Parameters { get; set; } = new();

        public StockMonitoringJob() { }

        public StockMonitoringJob(Guid productId, string action, int? threshold = null, Dictionary<string, object>? parameters = null)
        {
            ProductId = productId;
            Action = action;
            Threshold = threshold;
            Parameters = parameters ?? new Dictionary<string, object>();
        }
    }

    /// <summary>
    /// Job for cleaning up expired data and maintaining system health
    /// </summary>
    public class CleanupJob : BaseJob
    {
        public override string JobType => "Cleanup";
        
        public string CleanupType { get; set; } = string.Empty; // "ExpiredOrders", "OldLogs", "TempFiles", "FailedJobs"
        public DateTime? CutoffDate { get; set; }
        public int? BatchSize { get; set; } = 100;
        public Dictionary<string, object> Parameters { get; set; } = new();

        public CleanupJob() { }

        public CleanupJob(string cleanupType, DateTime? cutoffDate = null, int? batchSize = null, Dictionary<string, object>? parameters = null)
        {
            CleanupType = cleanupType;
            CutoffDate = cutoffDate;
            BatchSize = batchSize;
            Parameters = parameters ?? new Dictionary<string, object>();
        }
    }
}