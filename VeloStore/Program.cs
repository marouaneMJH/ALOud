using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using VeloStore.Data;
using VeloStore.Services;

var builder = WebApplication.CreateBuilder(args);

// =====================================================
// SERVICES
// =====================================================

// Razor Pages (MVVM)
builder.Services.AddRazorPages();

// -----------------------------------------------------
// SQL SERVER - EF CORE
// -----------------------------------------------------
builder.Services.AddDbContext<VeloStoreDbContext>(options =>
{
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sql =>
        {
            sql.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(5),
                errorNumbersToAdd: null
            );
        });
});

// -----------------------------------------------------
// REDIS
// -----------------------------------------------------
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var logger = sp.GetRequiredService<ILogger<Program>>();
    var redisConnection = builder.Configuration.GetSection("Redis")["ConnectionString"];

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
        logger.LogCritical(ex, "[-] Redis connection failed");
        throw;
    }
});

// Redis abstraction layer
builder.Services.AddScoped<ICacheService, RedisCacheService>();

// -----------------------------------------------------
// HTTP CONTEXT (needed for cart/user scope)
// -----------------------------------------------------
builder.Services.AddHttpContextAccessor();

// Cart service (Redis-based)
builder.Services.AddScoped<CartService>();

// =====================================================
// BUILD APP
// =====================================================
var app = builder.Build();

// =====================================================
// STARTUP CHECKS (FAIL FAST)
// =====================================================
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();

    // ---- SQL Server check
    try
    {
        var db = services.GetRequiredService<VeloStoreDbContext>();

        if (db.Database.CanConnect())
        {
            logger.LogInformation("[+] SQL Server connection OK");
        }
        else
        {
            logger.LogCritical("SQL Server connection FAILED");
            throw new Exception("[-] Database not reachable");
        }
    }
    catch (Exception ex)
    {
        logger.LogCritical(ex, "[-] Database startup check failed");
        throw;
    }

    // ---- Redis check
    try
    {
        var redis = services.GetRequiredService<IConnectionMultiplexer>();
        var ping = redis.GetDatabase().Ping();

        logger.LogInformation("Redis ping OK ({Ping} ms)", ping.TotalMilliseconds);
    }
    catch (Exception ex)
    {
        logger.LogCritical(ex, "Redis startup check failed");
        throw;
    }
}

// =====================================================
// MIDDLEWARE PIPELINE
// =====================================================
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.MapRazorPages();

// =====================================================
// RUN
// =====================================================
app.Run();
