using ALOud.Services.Business.Jobs.Processors;
using ALOud.Services.Infrastructure.Jobs;
using Microsoft.Extensions.Options;

namespace ALOud.Services.Business.Jobs.HostedServices
{
    /// <summary>
    /// Background service for processing order-related jobs
    /// </summary>
    public class OrderProcessingHostedService : BackgroundJobService<OrderProcessingJob>
    {
        public OrderProcessingHostedService(
            ILogger<OrderProcessingHostedService> logger,
            IServiceProvider serviceProvider,
            IJobQueue<OrderProcessingJob> jobQueue,
            IOptions<JobQueueOptions> options)
            : base(serviceProvider, logger, HostedServiceHelper.CreateConfig(options.Value, "OrderProcessing"))
        {
        }
    }

    /// <summary>
    /// Background service for processing status synchronization jobs
    /// </summary>
    public class StatusSyncHostedService : BackgroundJobService<StatusSyncJob>
    {
        public StatusSyncHostedService(
            ILogger<StatusSyncHostedService> logger,
            IServiceProvider serviceProvider,
            IJobQueue<StatusSyncJob> jobQueue,
            IOptions<JobQueueOptions> options)
            : base(serviceProvider, logger, HostedServiceHelper.CreateConfig(options.Value, "StatusSync"))
        {
        }
    }

    /// <summary>
    /// Background service for processing stock monitoring jobs
    /// </summary>
    public class StockMonitoringHostedService : BackgroundJobService<StockMonitoringJob>
    {
        public StockMonitoringHostedService(
            ILogger<StockMonitoringHostedService> logger,
            IServiceProvider serviceProvider,
            IJobQueue<StockMonitoringJob> jobQueue,
            IOptions<JobQueueOptions> options)
            : base(serviceProvider, logger, HostedServiceHelper.CreateConfig(options.Value, "StockMonitoring"))
        {
        }
    }

    /// <summary>
    /// Background service for processing cleanup jobs
    /// </summary>
    public class CleanupHostedService : BackgroundJobService<CleanupJob>
    {
        public CleanupHostedService(
            ILogger<CleanupHostedService> logger,
            IServiceProvider serviceProvider,
            IJobQueue<CleanupJob> jobQueue,
            IOptions<JobQueueOptions> options)
            : base(serviceProvider, logger, HostedServiceHelper.CreateConfig(options.Value, "Cleanup"))
        {
        }
    }

    internal static class HostedServiceHelper
    {
        public static JobTypeConfig CreateConfig(JobQueueOptions options, string jobType)
        {
            return new JobTypeConfig
            {
                // TODO: Make sure to enable it
                Enabled = false,
                QueueName = GetQueueName(jobType),
                PollingIntervalSeconds = GetPollingInterval(jobType),
                BatchSize = GetBatchSize(jobType)
            };
        }

        private static string GetQueueName(string jobType)
        {
            return jobType.ToLower() switch
            {
                "orderprocessing" => "orders:processing",
                "statussync" => "sync:status", 
                "stockmonitoring" => "stock:monitoring",
                "cleanup" => "maintenance:cleanup",
                _ => $"{jobType.ToLower()}:queue"
            };
        }

        private static int GetPollingInterval(string jobType)
        {
            return jobType.ToLower() switch
            {
                "orderprocessing" => 30,
                "statussync" => 60,
                "stockmonitoring" => 300,
                "cleanup" => 3600,
                _ => 60
            };
        }

        private static int GetBatchSize(string jobType)
        {
            return jobType.ToLower() switch
            {
                "orderprocessing" => 10,
                "statussync" => 5,
                "stockmonitoring" => 20,
                "cleanup" => 50,
                _ => 10
            };
        }
    }
}