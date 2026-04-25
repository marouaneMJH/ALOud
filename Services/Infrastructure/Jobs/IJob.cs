using System.Text.Json.Serialization;

namespace ALOud.Services.Infrastructure.Jobs
{
    /// <summary>
    /// Represents a job that can be queued and processed
    /// </summary>
    public interface IJob
    {
        /// <summary>
        /// Unique identifier for the job
        /// </summary>
        string Id { get; set; }
        
        /// <summary>
        /// Job type identifier
        /// </summary>
        string JobType { get; }
        
        /// <summary>
        /// When the job was created
        /// </summary>
        DateTime CreatedAt { get; set; }
        
        /// <summary>
        /// When the job should be processed (null for immediate)
        /// </summary>
        DateTime? ScheduledFor { get; set; }
        
        /// <summary>
        /// Number of times this job has been retried
        /// </summary>
        int RetryCount { get; set; }
        
        /// <summary>
        /// Maximum number of retries allowed
        /// </summary>
        int MaxRetries { get; set; }
        
        /// <summary>
        /// Additional metadata for the job
        /// </summary>
        Dictionary<string, string> Metadata { get; set; }
    }

    /// <summary>
    /// Base implementation of IJob
    /// </summary>
    public abstract class BaseJob : IJob
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        
        [JsonPropertyName("jobType")]
        public abstract string JobType { get; }
        
        [JsonPropertyName("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        [JsonPropertyName("scheduledFor")]
        public DateTime? ScheduledFor { get; set; }
        
        [JsonPropertyName("retryCount")]
        public int RetryCount { get; set; } = 0;
        
        [JsonPropertyName("maxRetries")]
        public int MaxRetries { get; set; } = 3;
        
        [JsonPropertyName("metadata")]
        public Dictionary<string, string> Metadata { get; set; } = new();
    }

    /// <summary>
    /// Result of job execution
    /// </summary>
    public class JobResult
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public TimeSpan ExecutionTime { get; set; }
        public Dictionary<string, object> Data { get; set; } = new();
        
        public static JobResult Successful(TimeSpan executionTime, Dictionary<string, object>? data = null)
        {
            return new JobResult
            {
                Success = true,
                ExecutionTime = executionTime,
                Data = data ?? new Dictionary<string, object>()
            };
        }
        
        public static JobResult Failed(string errorMessage, TimeSpan executionTime)
        {
            return new JobResult
            {
                Success = false,
                ErrorMessage = errorMessage,
                ExecutionTime = executionTime
            };
        }
    }

    /// <summary>
    /// Job processing status
    /// </summary>
    public enum JobStatus
    {
        Pending,
        Processing,
        Completed,
        Failed,
        Retrying,
        DeadLetter
    }

    /// <summary>
    /// Envelope containing job and processing metadata
    /// </summary>
    public class JobEnvelope<T> where T : class, IJob
    {
        [JsonPropertyName("job")]
        public T Job { get; set; } = default!;
        
        [JsonPropertyName("status")]
        public JobStatus Status { get; set; } = JobStatus.Pending;
        
        [JsonPropertyName("enqueuedAt")]
        public DateTime EnqueuedAt { get; set; } = DateTime.UtcNow;
        
        [JsonPropertyName("processingStartedAt")]
        public DateTime? ProcessingStartedAt { get; set; }
        
        [JsonPropertyName("completedAt")]
        public DateTime? CompletedAt { get; set; }
        
        [JsonPropertyName("lastError")]
        public string? LastError { get; set; }
        
        [JsonPropertyName("processingHistory")]
        public List<JobProcessingEvent> ProcessingHistory { get; set; } = new();
        
        [JsonPropertyName("processingNodeId")]
        public string? ProcessingNodeId { get; set; }
    }

    /// <summary>
    /// Processing event in job history
    /// </summary>
    public class JobProcessingEvent
    {
        [JsonPropertyName("timestamp")]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        
        [JsonPropertyName("status")]
        public JobStatus Status { get; set; }
        
        [JsonPropertyName("message")]
        public string? Message { get; set; }
        
        [JsonPropertyName("nodeId")]
        public string? NodeId { get; set; }
        
        [JsonPropertyName("executionTimeMs")]
        public long? ExecutionTimeMs { get; set; }
    }
}