using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Diagnostics;

namespace ALOud.Services.Infrastructure.Jobs
{
    /// <summary>
    /// Base class for background job processing services
    /// </summary>
    public abstract class BackgroundJobService<T> : BackgroundService where T : class, IJob
    {
        protected readonly IServiceProvider ServiceProvider;
        protected readonly ILogger Logger;
        protected readonly JobTypeConfig Config;
        
        private readonly string _nodeId;
        private volatile bool _isProcessing = false;

        protected BackgroundJobService(
            IServiceProvider serviceProvider,
            ILogger logger,
            JobTypeConfig config)
        {
            ServiceProvider = serviceProvider;
            Logger = logger;
            Config = config;
            _nodeId = Environment.MachineName;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (!Config.Enabled)
            {
                Logger.LogInformation("Job processing disabled for {JobType}", typeof(T).Name);
                return;
            }

            Logger.LogInformation("Starting background job service for {JobType} with queue {QueueName}", 
                typeof(T).Name, Config.QueueName);

            // Wait a bit before starting to allow other services to initialize
            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessJobsAsync(stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    Logger.LogInformation("Job processing cancelled for {JobType}", typeof(T).Name);
                    break;
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "Error in job processing loop for {JobType}", typeof(T).Name);
                    
                    // Wait before retrying to avoid tight error loops
                    await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
                }

                if (!stoppingToken.IsCancellationRequested)
                {
                    await Task.Delay(TimeSpan.FromSeconds(Config.PollingIntervalSeconds), stoppingToken);
                }
            }

            Logger.LogInformation("Background job service stopped for {JobType}", typeof(T).Name);
        }

        private async Task ProcessJobsAsync(CancellationToken stoppingToken)
        {
            if (_isProcessing)
            {
                Logger.LogDebug("Job processing already in progress for {JobType}, skipping", typeof(T).Name);
                return;
            }

            _isProcessing = true;
            
            try
            {
                using var scope = ServiceProvider.CreateScope();
                var jobQueue = scope.ServiceProvider.GetRequiredService<IJobQueue<T>>();
                var processor = scope.ServiceProvider.GetRequiredService<IJobProcessor<T>>();

                // Dequeue jobs for processing
                var jobs = await jobQueue.DequeueAsync(
                    Config.QueueName, 
                    Config.BatchSize, 
                    TimeSpan.FromMinutes(30));

                if (jobs.Count == 0)
                {
                    Logger.LogDebug("No jobs available in queue {QueueName}", Config.QueueName);
                    return;
                }

                Logger.LogDebug("Processing {JobCount} jobs from queue {QueueName}", jobs.Count, Config.QueueName);

                // Process jobs in parallel (with concurrency limit)
                var semaphore = new SemaphoreSlim(Math.Min(Config.BatchSize, Environment.ProcessorCount));
                var tasks = jobs.Select(async jobEnvelope =>
                {
                    await semaphore.WaitAsync(stoppingToken);
                    try
                    {
                        await ProcessSingleJobAsync(jobEnvelope, jobQueue, processor, stoppingToken);
                    }
                    finally
                    {
                        semaphore.Release();
                    }
                });

                await Task.WhenAll(tasks);
            }
            finally
            {
                _isProcessing = false;
            }
        }

        private async Task ProcessSingleJobAsync(
            JobEnvelope<T> jobEnvelope, 
            IJobQueue<T> jobQueue, 
            IJobProcessor<T> processor, 
            CancellationToken stoppingToken)
        {
            var job = jobEnvelope.Job;
            var stopwatch = Stopwatch.StartNew();

            try
            {
                Logger.LogInformation("Processing job {JobId} of type {JobType}", job.Id, job.JobType);

                // Check if processor can handle this job
                if (!processor.CanProcess(job))
                {
                    Logger.LogWarning("Processor cannot handle job {JobId} of type {JobType}", job.Id, job.JobType);
                    
                    var unsupportedResult = JobResult.Failed("Processor cannot handle this job type", stopwatch.Elapsed);
                    await jobQueue.FailAsync(job.Id, unsupportedResult);
                    return;
                }

                // Process the job
                var result = await processor.ProcessAsync(job, stoppingToken);
                stopwatch.Stop();

                if (result.Success)
                {
                    await jobQueue.CompleteAsync(job.Id, result);
                    Logger.LogInformation("Successfully processed job {JobId} in {ExecutionTime}ms", 
                        job.Id, stopwatch.ElapsedMilliseconds);
                }
                else
                {
                    await jobQueue.FailAsync(job.Id, result);
                    Logger.LogWarning("Job {JobId} failed: {ErrorMessage}", job.Id, result.ErrorMessage);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                Logger.LogInformation("Job {JobId} processing cancelled", job.Id);
                
                var cancelledResult = JobResult.Failed("Processing cancelled", stopwatch.Elapsed);
                await jobQueue.FailAsync(job.Id, cancelledResult);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                Logger.LogError(ex, "Unexpected error processing job {JobId}", job.Id);
                
                var errorResult = JobResult.Failed($"Unexpected error: {ex.Message}", stopwatch.Elapsed);
                await jobQueue.FailAsync(job.Id, errorResult);
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            Logger.LogInformation("Stopping background job service for {JobType}", typeof(T).Name);
            
            // Wait for current processing to complete
            var timeout = TimeSpan.FromSeconds(30);
            var startTime = DateTime.UtcNow;
            
            while (_isProcessing && DateTime.UtcNow - startTime < timeout)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(100), cancellationToken);
            }

            if (_isProcessing)
            {
                Logger.LogWarning("Job processing did not stop gracefully within {Timeout}s for {JobType}", 
                    timeout.TotalSeconds, typeof(T).Name);
            }

            await base.StopAsync(cancellationToken);
        }
    }

    /// <summary>
    /// Base class for job processors
    /// </summary>
    public abstract class JobProcessor<T> : IJobProcessor<T> where T : class, IJob
    {
        protected readonly ILogger Logger;
        
        protected JobProcessor(ILogger logger)
        {
            Logger = logger;
        }

        public abstract string JobType { get; }
        
        public virtual bool CanProcess(T job)
        {
            return job.JobType == JobType;
        }

        public abstract Task<JobResult> ProcessAsync(T job, CancellationToken cancellationToken = default);

        protected JobResult Success(TimeSpan executionTime, Dictionary<string, object>? data = null)
        {
            return JobResult.Successful(executionTime, data);
        }

        protected JobResult Failure(string errorMessage, TimeSpan executionTime)
        {
            return JobResult.Failed(errorMessage, executionTime);
        }
    }

    /// <summary>
    /// Distributed lock implementation using Redis
    /// </summary>
    public interface IDistributedLock
    {
        Task<bool> AcquireLockAsync(string key, TimeSpan expiry, string? value = null);
        Task<bool> ReleaseLockAsync(string key, string? value = null);
        Task<bool> ExtendLockAsync(string key, TimeSpan expiry, string? value = null);
    }

    public class RedisDistributedLock : IDistributedLock
    {
        private readonly StackExchange.Redis.IDatabase _db;
        private readonly ILogger<RedisDistributedLock> _logger;

        public RedisDistributedLock(
            StackExchange.Redis.IConnectionMultiplexer redis,
            ILogger<RedisDistributedLock> logger)
        {
            _db = redis.GetDatabase();
            _logger = logger;
        }

        public async Task<bool> AcquireLockAsync(string key, TimeSpan expiry, string? value = null)
        {
            try
            {
                value ??= Environment.MachineName + ":" + Guid.NewGuid().ToString("N")[..8];
                var acquired = await _db.StringSetAsync($"lock:{key}", value, expiry, StackExchange.Redis.When.NotExists);
                
                if (acquired)
                {
                    _logger.LogDebug("Acquired lock {LockKey} with value {Value} for {Expiry}", key, value, expiry);
                }
                else
                {
                    _logger.LogDebug("Failed to acquire lock {LockKey}", key);
                }

                return acquired;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error acquiring lock {LockKey}", key);
                return false;
            }
        }

        public async Task<bool> ReleaseLockAsync(string key, string? value = null)
        {
            try
            {
                if (value == null)
                {
                    // Force release (dangerous - use only for cleanup)
                    var deleted = await _db.KeyDeleteAsync($"lock:{key}");
                    _logger.LogDebug("Force released lock {LockKey}: {Success}", key, deleted);
                    return deleted;
                }

                // Lua script to ensure we only release our own lock
                const string script = @"
                    if redis.call('GET', KEYS[1]) == ARGV[1] then
                        return redis.call('DEL', KEYS[1])
                    else
                        return 0
                    end
                ";

                var result = await _db.ScriptEvaluateAsync(script, new StackExchange.Redis.RedisKey[] { $"lock:{key}" }, new StackExchange.Redis.RedisValue[] { value });
                var released = (int)result == 1;
                
                _logger.LogDebug("Released lock {LockKey} with value {Value}: {Success}", key, value, released);
                return released;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error releasing lock {LockKey}", key);
                return false;
            }
        }

        public async Task<bool> ExtendLockAsync(string key, TimeSpan expiry, string? value = null)
        {
            try
            {
                if (value == null)
                {
                    // Simple expiry extension without ownership check
                    var simpleExtended = await _db.KeyExpireAsync($"lock:{key}", expiry);
                    _logger.LogDebug("Extended lock {LockKey} for {Expiry}: {Success}", key, expiry, simpleExtended);
                    return simpleExtended;
                }

                // Lua script to extend only if we own the lock
                const string script = @"
                    if redis.call('GET', KEYS[1]) == ARGV[1] then
                        return redis.call('EXPIRE', KEYS[1], ARGV[2])
                    else
                        return 0
                    end
                ";

                var expirySeconds = (int)expiry.TotalSeconds;
                var result = await _db.ScriptEvaluateAsync(script, 
                    new StackExchange.Redis.RedisKey[] { $"lock:{key}" }, 
                    new StackExchange.Redis.RedisValue[] { value, expirySeconds });
                
                var extended = (int)result == 1;
                _logger.LogDebug("Extended lock {LockKey} with value {Value} for {Expiry}: {Success}", key, value, expiry, extended);
                return extended;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extending lock {LockKey}", key);
                return false;
            }
        }
    }
}