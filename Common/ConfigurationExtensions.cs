using ALOud.Services;
using StackExchange.Redis;
using ALOud.Services.Rag.Clients;
using ALOud.Services.Infrastructure.Rag.Models;
using ALOud.Data;
using Microsoft.EntityFrameworkCore;
using ALOud.Services.Data;
using ALOud.Services.Infrastructure.ExpertSystem;
using ALOud.Services.Rag.IndexingJob;

namespace ALOud.Common
{
    public static class ConfigurationExtensions
    {
        public static void LoadEnvironmentVariables(this WebApplicationBuilder builder)
        {
            var envPath = Path.Combine(builder.Environment.ContentRootPath, ".env");
            if (!File.Exists(envPath))
                return;

            foreach (var line in File.ReadAllLines(envPath))
            {
                var trimmed = line.Trim();
                if (string.IsNullOrEmpty(trimmed) || trimmed.StartsWith('#'))
                    continue;

                var idx = trimmed.IndexOf('=');
                if (idx <= 0)
                    continue;

                var key = trimmed[..idx].Trim();
                var value = trimmed[(idx + 1)..].Trim();

                // Remove optional surrounding quotes
                if ((value.StartsWith('"') && value.EndsWith('"')) ||
                    (value.StartsWith('\'') && value.EndsWith('\'')))
                {
                    value = value[1..^1];
                }

                Environment.SetEnvironmentVariable(key, value);
            }
        }

        public static void ConfigureSmtp(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<SmtpOptions>(options =>
            {
                var smtpSection = configuration.GetSection("Smtp");

                options.Host = smtpSection["Host"] ?? "smtp.gmail.com";
                options.Port = int.TryParse(smtpSection["Port"], out var port) ? port : 587;

                // Credentials from environment variables (preferred)
                options.User = Environment.GetEnvironmentVariable("SMTP_USER")
                              ?? smtpSection["User"]
                              ?? string.Empty;

                options.Password = Environment.GetEnvironmentVariable("SMTP_PASSWORD")
                                  ?? smtpSection["Password"]
                                  ?? string.Empty;

                options.From = Environment.GetEnvironmentVariable("SMTP_FROM")
                              ?? smtpSection["From"]
                              ?? string.Empty;
            });
        }

        public static void ConfigureRedis(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<IConnectionMultiplexer>(sp =>
            {
                var logger = sp.GetRequiredService<ILogger<Program>>();
                var redisConnection = configuration.GetSection("Redis")["ConnectionString"];

                if (string.IsNullOrWhiteSpace(redisConnection))
                    throw new InvalidOperationException("Redis connection string is missing");

                try
                {
                    var options = ConfigurationOptions.Parse(redisConnection);
                    options.AbortOnConnectFail = false;
                    options.ConnectRetry = 3;
                    options.ConnectTimeout = 3000;

                    var connection = ConnectionMultiplexer.Connect(options);
                    logger.LogInformation("[+] Redis connection established");
                    return connection;
                }
                catch (Exception ex)
                {
                    logger.LogCritical(ex, "[-] Redis connection failed: {Message}", ex.Message);
                    throw new InvalidOperationException("Failed to establish Redis connection. Ensure Redis is running and accessible.", ex);
                }
            });
        }

        public static void ConfigureDatabase(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<ALOudDbContext>(options =>
            {
                // Use environment variable if available, fallback to appsettings
                var connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING")
                                      ?? configuration.GetConnectionString("DefaultConnection");
                
                if (string.IsNullOrWhiteSpace(connectionString))
                    throw new InvalidOperationException("Database connection string is missing");
                
                options.UseSqlServer(
                    connectionString,
                    sql =>
                    {
                        sql.EnableRetryOnFailure(
                            maxRetryCount: 5,
                            maxRetryDelay: TimeSpan.FromSeconds(5),
                            errorNumbersToAdd: null);
                    });
            });
        }

        public static async Task PerformStartupChecksAsync(this WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var services = scope.ServiceProvider;
            var logger = services.GetRequiredService<ILogger<Program>>();

            // SQL Server connectivity check
            await CheckDatabaseConnectionAsync(services, logger);

            // Redis connectivity check
            await CheckRedisConnectionAsync(services, logger);

            // Ensure Qdrant collection exists
            await EnsureQdrantCollectionAsync(services);
        }

        private static async Task CheckDatabaseConnectionAsync(IServiceProvider services, ILogger logger)
        {
            try
            {
                var db = services.GetRequiredService<ALOudDbContext>();
                if (!await db.Database.CanConnectAsync())
                    throw new InvalidOperationException("Database not reachable");

                logger.LogInformation("[+] SQL Server connection OK");

                // Seed perfume data if database is empty
                await PerfumeSeeder.SeedAsync(db);
            }
            catch (Exception ex)
            {
                logger.LogCritical(ex, "[-] Database startup check failed: {Message}", ex.Message);
                throw new InvalidOperationException("Database startup check failed. Ensure SQL Server is running and accessible.", ex);
            }
        }

        private static async Task CheckRedisConnectionAsync(IServiceProvider services, ILogger logger)
        {
            try
            {
                var redis = services.GetRequiredService<IConnectionMultiplexer>();
                var ping = await redis.GetDatabase().PingAsync();

                logger.LogInformation("[+] Redis ping OK ({Ping} ms)", ping.TotalMilliseconds);
            }
            catch (Exception ex)
            {
                logger.LogCritical(ex, "[-] Redis startup check failed: {Message}", ex.Message);
                throw new InvalidOperationException("Redis startup check failed. Ensure Redis is running and accessible.", ex);
            }
        }

        private static async Task EnsureQdrantCollectionAsync(IServiceProvider services)
        {
            var bootstrap = services.GetRequiredService<QdrantBootstrapService>();
            await bootstrap.EnsureCollectionExistsAsync();
        }
    }
}