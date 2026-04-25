namespace ALOud.Services.Infrastructure.Jobs
{
    /// <summary>
    /// Interface for job queue operations
    /// </summary>
    public interface IJobQueue<T> where T : class, IJob
    {
        /// <summary>
        /// Add a job to the queue
        /// </summary>
        Task<string> EnqueueAsync(T job, string? queueName = null);
        
        /// <summary>
        /// Add multiple jobs to the queue
        /// </summary>
        Task<List<string>> EnqueueBatchAsync(IEnumerable<T> jobs, string? queueName = null);
        
        /// <summary>
        /// Dequeue jobs for processing
        /// </summary>
        Task<List<JobEnvelope<T>>> DequeueAsync(string queueName, int batchSize = 1, TimeSpan? visibilityTimeout = null);
        
        /// <summary>
        /// Mark a job as completed
        /// </summary>
        Task CompleteAsync(string jobId, JobResult result);
        
        /// <summary>
        /// Mark a job as failed and handle retry logic
        /// </summary>
        Task FailAsync(string jobId, JobResult result);
        
        /// <summary>
        /// Get job status and details
        /// </summary>
        Task<JobEnvelope<T>?> GetJobAsync(string jobId);
        
        /// <summary>
        /// Delete a job from all queues
        /// </summary>
        Task DeleteAsync(string jobId);
        
        /// <summary>
        /// Get queue statistics
        /// </summary>
        Task<QueueStatistics> GetQueueStatisticsAsync(string queueName);
        
        /// <summary>
        /// Get all failed jobs for manual review
        /// </summary>
        Task<List<JobEnvelope<T>>> GetFailedJobsAsync(string queueName, int limit = 100);
        
        /// <summary>
        /// Retry a failed job
        /// </summary>
        Task<bool> RetryJobAsync(string jobId);
        
        /// <summary>
        /// Clean up expired jobs and processing locks
        /// </summary>
        Task CleanupAsync(string queueName, TimeSpan maxAge);
    }

    /// <summary>
    /// Queue statistics
    /// </summary>
    public class QueueStatistics
    {
        public string QueueName { get; set; } = string.Empty;
        public int PendingJobs { get; set; }
        public int ProcessingJobs { get; set; }
        public int CompletedJobs { get; set; }
        public int FailedJobs { get; set; }
        public int DeadLetterJobs { get; set; }
        public DateTime LastActivity { get; set; }
        public TimeSpan AverageProcessingTime { get; set; }
    }

    /// <summary>
    /// Interface for processing jobs
    /// </summary>
    public interface IJobProcessor<T> where T : class, IJob
    {
        /// <summary>
        /// Process a single job
        /// </summary>
        Task<JobResult> ProcessAsync(T job, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Job type that this processor handles
        /// </summary>
        string JobType { get; }
        
        /// <summary>
        /// Whether this processor can handle the given job
        /// </summary>
        bool CanProcess(T job);
    }

    /// <summary>
    /// Configuration for job processing
    /// </summary>
    public class JobQueueOptions
    {
        public int DefaultRetries { get; set; } = 3;
        public int RetryDelayMinutes { get; set; } = 5;
        public int MaxProcessingTimeMinutes { get; set; } = 30;
        public string DeadLetterQueue { get; set; } = "jobs:failed";
        public int HealthCheckIntervalSeconds { get; set; } = 60;
        public string NodeId { get; set; } = Environment.MachineName;
    }

    /// <summary>
    /// Configuration for specific job types
    /// </summary>
    public class JobTypeConfig
    {
        public bool Enabled { get; set; } = true;
        public int PollingIntervalSeconds { get; set; } = 30;
        public int BatchSize { get; set; } = 10;
        public string QueueName { get; set; } = string.Empty;
    }
}