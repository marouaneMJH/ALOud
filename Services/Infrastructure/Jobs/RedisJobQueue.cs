using StackExchange.Redis;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;

namespace ALOud.Services.Infrastructure.Jobs
{
    /// <summary>
    /// Redis-based implementation of job queue
    /// </summary>
    public class RedisJobQueue<T> : IJobQueue<T> where T : class, IJob
    {
        private readonly IDatabase _db;
        private readonly ILogger<RedisJobQueue<T>> _logger;
        private readonly JobQueueOptions _options;
        private readonly string _nodeId;

        // Redis key patterns
        private const string QUEUE_KEY = "queue:{0}"; // queue:orders:processing
        private const string JOB_KEY = "job:{0}"; // job:12345
        private const string PROCESSING_KEY = "processing:{0}"; // processing:orders:processing
        private const string COMPLETED_KEY = "completed:{0}"; // completed:orders:processing
        private const string FAILED_KEY = "failed:{0}"; // failed:orders:processing
        private const string STATS_KEY = "stats:{0}"; // stats:orders:processing
        private const string LOCK_KEY = "lock:job:{0}"; // lock:job:12345

        public RedisJobQueue(
            IConnectionMultiplexer redis,
            ILogger<RedisJobQueue<T>> logger,
            IOptions<JobQueueOptions> options)
        {
            _db = redis.GetDatabase();
            _logger = logger;
            _options = options.Value;
            _nodeId = _options.NodeId;
        }

        public async Task<string> EnqueueAsync(T job, string? queueName = null)
        {
            var envelope = new JobEnvelope<T>
            {
                Job = job,
                Status = JobStatus.Pending,
                EnqueuedAt = DateTime.UtcNow
            };

            envelope.ProcessingHistory.Add(new JobProcessingEvent
            {
                Status = JobStatus.Pending,
                Message = "Job enqueued",
                NodeId = _nodeId
            });

            var queueKey = string.Format(QUEUE_KEY, queueName ?? GetDefaultQueueName());
            var jobKey = string.Format(JOB_KEY, job.Id);

            // Store job details
            await _db.StringSetAsync(jobKey, JsonSerializer.Serialize(envelope));

            // Add to queue
            await _db.ListLeftPushAsync(queueKey, job.Id);

            // Update statistics
            await UpdateStatsAsync(queueName ?? GetDefaultQueueName(), "enqueued");

            _logger.LogInformation("Enqueued job {JobId} of type {JobType} to queue {QueueName}",
                job.Id, job.JobType, queueName ?? GetDefaultQueueName());

            return job.Id;
        }

        public Task<List<string>> EnqueueBatchAsync(IEnumerable<T> jobs, string? queueName = null)
        {
            var jobIds = new List<string>();
            var batch = _db.CreateBatch();
            
            foreach (var job in jobs)
            {
                var envelope = new JobEnvelope<T>
                {
                    Job = job,
                    Status = JobStatus.Pending,
                    EnqueuedAt = DateTime.UtcNow
                };

                envelope.ProcessingHistory.Add(new JobProcessingEvent
                {
                    Status = JobStatus.Pending,
                    Message = "Job enqueued (batch)",
                    NodeId = _nodeId
                });

                var queueKey = string.Format(QUEUE_KEY, queueName ?? GetDefaultQueueName());
                var jobKey = string.Format(JOB_KEY, job.Id);

                // Store job details
                _ = batch.StringSetAsync(jobKey, JsonSerializer.Serialize(envelope));

                // Add to queue
                _ = batch.ListLeftPushAsync(queueKey, job.Id);

                jobIds.Add(job.Id);
            }

            batch.Execute();

            _logger.LogInformation("Enqueued {Count} jobs to queue {QueueName}", jobIds.Count, queueName ?? GetDefaultQueueName());

            return Task.FromResult(jobIds);
        }

        public async Task<List<JobEnvelope<T>>> DequeueAsync(string queueName, int batchSize = 1, TimeSpan? visibilityTimeout = null)
        {
            var jobs = new List<JobEnvelope<T>>();
            var queueKey = string.Format(QUEUE_KEY, queueName);
            var processingKey = string.Format(PROCESSING_KEY, queueName);
            var timeout = visibilityTimeout ?? TimeSpan.FromMinutes(_options.MaxProcessingTimeMinutes);

            for (int i = 0; i < batchSize; i++)
            {
                // Atomically move job from queue to processing
                var jobId = await _db.ListRightPopLeftPushAsync(queueKey, processingKey);
                
                if (!jobId.HasValue)
                    break; // No more jobs available

                var jobKey = string.Format(JOB_KEY, jobId!);
                var envelopeJson = await _db.StringGetAsync(jobKey);
                
                if (!envelopeJson.HasValue)
                {
                    _logger.LogWarning("Job {JobId} not found, removing from processing queue", jobId);
                    await _db.ListRemoveAsync(processingKey, jobId!);
                    continue;
                }

                try
                {
                    var envelope = JsonSerializer.Deserialize<JobEnvelope<T>>(envelopeJson!);
                    if (envelope?.Job == null)
                    {
                        _logger.LogWarning("Invalid job envelope for {JobId}, skipping", jobId);
                        continue;
                    }

                    // Check if job is scheduled for future
                    if (envelope.Job.ScheduledFor.HasValue && envelope.Job.ScheduledFor > DateTime.UtcNow)
                    {
                        // Put back in queue and skip
                        await _db.ListRightPopLeftPushAsync(processingKey, queueKey);
                        continue;
                    }

                    // Update envelope for processing
                    envelope.Status = JobStatus.Processing;
                    envelope.ProcessingStartedAt = DateTime.UtcNow;
                    envelope.ProcessingNodeId = _nodeId;
                    envelope.ProcessingHistory.Add(new JobProcessingEvent
                    {
                        Status = JobStatus.Processing,
                        Message = "Job started processing",
                        NodeId = _nodeId
                    });

                    // Set processing lock with timeout
                    var lockKey = string.Format(LOCK_KEY, jobId);
                    await _db.StringSetAsync(lockKey, _nodeId, timeout);

                    // Update job envelope
                    await _db.StringSetAsync(jobKey, JsonSerializer.Serialize(envelope));

                    jobs.Add(envelope);

                    _logger.LogDebug("Dequeued job {JobId} for processing", jobId);
                }
                catch (JsonException ex)
                {
                    _logger.LogError(ex, "Failed to deserialize job {JobId}, moving to failed queue", jobId);
                    await MoveToFailedQueue(jobId!, queueName, "Deserialization error");
                }
            }

            return jobs;
        }

        public async Task CompleteAsync(string jobId, JobResult result)
        {
            var jobKey = string.Format(JOB_KEY, jobId);
            var lockKey = string.Format(LOCK_KEY, jobId);

            var envelopeJson = await _db.StringGetAsync(jobKey);
            if (!envelopeJson.HasValue)
            {
                _logger.LogWarning("Job {JobId} not found for completion", jobId);
                return;
            }

            var envelope = JsonSerializer.Deserialize<JobEnvelope<T>>(envelopeJson!);
            if (envelope?.Job == null)
            {
                _logger.LogWarning("Invalid job envelope for {JobId} during completion", jobId);
                return;
            }

            // Update envelope
            envelope.Status = JobStatus.Completed;
            envelope.CompletedAt = DateTime.UtcNow;
            envelope.ProcessingHistory.Add(new JobProcessingEvent
            {
                Status = JobStatus.Completed,
                Message = result.Success ? "Job completed successfully" : $"Job failed: {result.ErrorMessage}",
                NodeId = _nodeId,
                ExecutionTimeMs = (long)result.ExecutionTime.TotalMilliseconds
            });

            // Store completed job
            await _db.StringSetAsync(jobKey, JsonSerializer.Serialize(envelope));

            // Remove from processing queues
            await RemoveFromProcessingQueues(jobId);

            // Remove processing lock
            await _db.KeyDeleteAsync(lockKey);

            // Move to completed queue
            var queueName = GetQueueNameFromJobType(envelope.Job.JobType);
            var completedKey = string.Format(COMPLETED_KEY, queueName);
            await _db.ListLeftPushAsync(completedKey, jobId);

            // Update statistics
            await UpdateStatsAsync(queueName, result.Success ? "completed" : "failed");

            _logger.LogInformation("Job {JobId} completed successfully in {ExecutionTime}ms",
                jobId, result.ExecutionTime.TotalMilliseconds);
        }

        public async Task FailAsync(string jobId, JobResult result)
        {
            var jobKey = string.Format(JOB_KEY, jobId);
            
            var envelopeJson = await _db.StringGetAsync(jobKey);
            if (!envelopeJson.HasValue)
            {
                _logger.LogWarning("Job {JobId} not found for failure handling", jobId);
                return;
            }

            var envelope = JsonSerializer.Deserialize<JobEnvelope<T>>(envelopeJson!);
            if (envelope?.Job == null)
            {
                _logger.LogWarning("Invalid job envelope for {JobId} during failure handling", jobId);
                return;
            }

            envelope.LastError = result.ErrorMessage;
            envelope.Job.RetryCount++;
            
            envelope.ProcessingHistory.Add(new JobProcessingEvent
            {
                Status = JobStatus.Failed,
                Message = $"Job failed: {result.ErrorMessage}",
                NodeId = _nodeId,
                ExecutionTimeMs = (long)result.ExecutionTime.TotalMilliseconds
            });

            var queueName = GetQueueNameFromJobType(envelope.Job.JobType);

            // Check if we should retry
            if (envelope.Job.RetryCount <= envelope.Job.MaxRetries)
            {
                // Schedule retry
                envelope.Status = JobStatus.Retrying;
                envelope.Job.ScheduledFor = DateTime.UtcNow.AddMinutes(_options.RetryDelayMinutes * envelope.Job.RetryCount);
                
                envelope.ProcessingHistory.Add(new JobProcessingEvent
                {
                    Status = JobStatus.Retrying,
                    Message = $"Scheduled for retry #{envelope.Job.RetryCount} at {envelope.Job.ScheduledFor}",
                    NodeId = _nodeId
                });

                // Put back in queue for retry
                var queueKey = string.Format(QUEUE_KEY, queueName);
                await _db.ListLeftPushAsync(queueKey, jobId);

                _logger.LogWarning("Job {JobId} failed, scheduled for retry #{RetryCount} at {RetryTime}: {Error}",
                    jobId, envelope.Job.RetryCount, envelope.Job.ScheduledFor, result.ErrorMessage);
            }
            else
            {
                // Move to dead letter queue
                envelope.Status = JobStatus.DeadLetter;
                
                envelope.ProcessingHistory.Add(new JobProcessingEvent
                {
                    Status = JobStatus.DeadLetter,
                    Message = "Max retries exceeded, moved to dead letter queue",
                    NodeId = _nodeId
                });

                var deadLetterKey = string.Format(FAILED_KEY, queueName);
                await _db.ListLeftPushAsync(deadLetterKey, jobId);

                _logger.LogError("Job {JobId} moved to dead letter queue after {RetryCount} retries: {Error}",
                    jobId, envelope.Job.RetryCount, result.ErrorMessage);
            }

            // Update job envelope
            await _db.StringSetAsync(jobKey, JsonSerializer.Serialize(envelope));

            // Remove from processing queues
            await RemoveFromProcessingQueues(jobId);

            // Remove processing lock
            var lockKey = string.Format(LOCK_KEY, jobId);
            await _db.KeyDeleteAsync(lockKey);

            // Update statistics
            await UpdateStatsAsync(queueName, "failed");
        }

        public async Task<JobEnvelope<T>?> GetJobAsync(string jobId)
        {
            var jobKey = string.Format(JOB_KEY, jobId);
            var envelopeJson = await _db.StringGetAsync(jobKey);
            
            if (!envelopeJson.HasValue)
                return null;

            try
            {
                return JsonSerializer.Deserialize<JobEnvelope<T>>(envelopeJson!);
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Failed to deserialize job {JobId}", jobId);
                return null;
            }
        }

        public async Task DeleteAsync(string jobId)
        {
            var jobKey = string.Format(JOB_KEY, jobId);
            var lockKey = string.Format(LOCK_KEY, jobId);

            // Remove from all possible queues
            await RemoveFromProcessingQueues(jobId);

            // Delete job data and lock
            await _db.KeyDeleteAsync(new RedisKey[] { jobKey, lockKey });

            _logger.LogInformation("Deleted job {JobId}", jobId);
        }

        public async Task<QueueStatistics> GetQueueStatisticsAsync(string queueName)
        {
            var queueKey = string.Format(QUEUE_KEY, queueName);
            var processingKey = string.Format(PROCESSING_KEY, queueName);
            var completedKey = string.Format(COMPLETED_KEY, queueName);
            var failedKey = string.Format(FAILED_KEY, queueName);
            var statsKey = string.Format(STATS_KEY, queueName);

            var batch = _db.CreateBatch();
            var pendingTask = batch.ListLengthAsync(queueKey);
            var processingTask = batch.ListLengthAsync(processingKey);
            var completedTask = batch.ListLengthAsync(completedKey);
            var failedTask = batch.ListLengthAsync(failedKey);
            var statsTask = batch.HashGetAllAsync(statsKey);

            batch.Execute();

            var stats = new QueueStatistics
            {
                QueueName = queueName,
                PendingJobs = (int)await pendingTask,
                ProcessingJobs = (int)await processingTask,
                CompletedJobs = (int)await completedTask,
                DeadLetterJobs = (int)await failedTask,
                LastActivity = DateTime.UtcNow
            };

            var statsHash = await statsTask;
            if (statsHash.Length > 0)
            {
                var statsDict = statsHash.ToDictionary();
                if (statsDict.TryGetValue("totalProcessed", out var totalProcessed) &&
                    statsDict.TryGetValue("totalProcessingTime", out var totalProcessingTime) &&
                    int.TryParse(totalProcessed, out var processed) && processed > 0 &&
                    long.TryParse(totalProcessingTime, out var processingTime))
                {
                    stats.AverageProcessingTime = TimeSpan.FromMilliseconds(processingTime / processed);
                }
            }

            return stats;
        }

        public async Task<List<JobEnvelope<T>>> GetFailedJobsAsync(string queueName, int limit = 100)
        {
            var failedKey = string.Format(FAILED_KEY, queueName);
            var jobIds = await _db.ListRangeAsync(failedKey, 0, limit - 1);
            
            var jobs = new List<JobEnvelope<T>>();
            
            foreach (var jobId in jobIds)
            {
                var job = await GetJobAsync(jobId!);
                if (job != null)
                    jobs.Add(job);
            }

            return jobs;
        }

        public async Task<bool> RetryJobAsync(string jobId)
        {
            var envelope = await GetJobAsync(jobId);
            if (envelope?.Job == null || envelope.Status != JobStatus.DeadLetter)
                return false;

            // Reset retry count and reschedule
            envelope.Job.RetryCount = 0;
            envelope.Job.ScheduledFor = null;
            envelope.Status = JobStatus.Pending;
            envelope.ProcessingHistory.Add(new JobProcessingEvent
            {
                Status = JobStatus.Pending,
                Message = "Job manually retried",
                NodeId = _nodeId
            });

            var queueName = GetQueueNameFromJobType(envelope.Job.JobType);
            var queueKey = string.Format(QUEUE_KEY, queueName);
            var failedKey = string.Format(FAILED_KEY, queueName);
            var jobKey = string.Format(JOB_KEY, jobId);

            // Remove from failed queue and add back to main queue
            await _db.ListRemoveAsync(failedKey, jobId);
            await _db.ListLeftPushAsync(queueKey, jobId);
            await _db.StringSetAsync(jobKey, JsonSerializer.Serialize(envelope));

            _logger.LogInformation("Job {JobId} manually retried", jobId);
            return true;
        }

        public async Task CleanupAsync(string queueName, TimeSpan maxAge)
        {
            var cutoff = DateTime.UtcNow - maxAge;
            var completedKey = string.Format(COMPLETED_KEY, queueName);
            var failedKey = string.Format(FAILED_KEY, queueName);

            // Get old completed jobs
            var completedJobs = await _db.ListRangeAsync(completedKey);
            var jobsToDelete = new List<string>();

            foreach (var jobId in completedJobs)
            {
                var envelope = await GetJobAsync(jobId!);
                if (envelope?.CompletedAt.HasValue == true && envelope.CompletedAt.Value < cutoff)
                {
                    jobsToDelete.Add(jobId!);
                }
            }

            // Delete old jobs
            foreach (var jobId in jobsToDelete)
            {
                await DeleteAsync(jobId);
                await _db.ListRemoveAsync(completedKey, jobId);
            }

            _logger.LogInformation("Cleaned up {Count} old jobs from queue {QueueName}", jobsToDelete.Count, queueName);
        }

        private async Task RemoveFromProcessingQueues(string jobId)
        {
            // This is a bit brute force, but ensures job is removed from all processing queues
            var allQueues = await GetAllQueueNames();
            
            foreach (var queueName in allQueues)
            {
                var processingKey = string.Format(PROCESSING_KEY, queueName);
                await _db.ListRemoveAsync(processingKey, jobId);
            }
        }

        private Task<List<string>> GetAllQueueNames()
        {
            // In a real implementation, you might maintain a set of active queue names
            // For now, we'll use the job type to infer queue names
            return Task.FromResult(new List<string> { "orders:processing", "sync:status", "stock:monitoring", "maintenance:cleanup" });
        }

        private async Task UpdateStatsAsync(string queueName, string operation)
        {
            var statsKey = string.Format(STATS_KEY, queueName);
            
            switch (operation)
            {
                case "enqueued":
                    await _db.HashIncrementAsync(statsKey, "totalEnqueued");
                    break;
                case "completed":
                    await _db.HashIncrementAsync(statsKey, "totalCompleted");
                    break;
                case "failed":
                    await _db.HashIncrementAsync(statsKey, "totalFailed");
                    break;
            }

            await _db.HashSetAsync(statsKey, "lastActivity", DateTimeOffset.UtcNow.ToUnixTimeSeconds());
        }

        private async Task MoveToFailedQueue(string jobId, string queueName, string reason)
        {
            var processingKey = string.Format(PROCESSING_KEY, queueName);
            var failedKey = string.Format(FAILED_KEY, queueName);
            
            await _db.ListRemoveAsync(processingKey, jobId);
            await _db.ListLeftPushAsync(failedKey, jobId);

            _logger.LogError("Moved job {JobId} to failed queue: {Reason}", jobId, reason);
        }

        private string GetDefaultQueueName()
        {
            return "default";
        }

        private string GetQueueNameFromJobType(string jobType)
        {
            // Map job types to queue names
            return jobType.ToLowerInvariant() switch
            {
                "orderprocessingjob" => "orders:processing",
                "statussyncjob" => "sync:status",
                "stockmonitoringjob" => "stock:monitoring",
                "cleanupjob" => "maintenance:cleanup",
                _ => "default"
            };
        }
    }
}