using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using ALOud.Data;
using Services;
using ALOud.Services;
using ALOud.Services.Security;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// Load .env file into environment variables (simple loader)
var envPath = Path.Combine(builder.Environment.ContentRootPath, ".env");
if (File.Exists(envPath))
{
    foreach (var line in File.ReadAllLines(envPath))
    {
        var trimmed = line.Trim();
        if (string.IsNullOrEmpty(trimmed) || trimmed.StartsWith("#")) continue;

        var idx = trimmed.IndexOf('=');
        if (idx <= 0) continue;

        var key = trimmed.Substring(0, idx).Trim();
        var value = trimmed.Substring(idx + 1).Trim();

        // remove optional surrounding quotes
        if ((value.StartsWith("\"") && value.EndsWith("\"")) || (value.StartsWith("\'") && value.EndsWith("\'")))
        {
            value = value.Substring(1, value.Length - 2);
        }

        Environment.SetEnvironmentVariable(key, value);
    }
}




// =====================================================
// SERVICES
// =====================================================

// -----------------------------------------------------
// User Management Service
// -----------------------------------------------------
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<PasswordHasherService>();
// Email & Verification
builder.Services.AddScoped<IEmailService, SmtpEmailService>();
builder.Services.AddScoped<IVerificationService, VerificationService>();


// Razor Pages (MVVM)
builder.Services.AddRazorPages();

// -----------------------------------------------------
// SQL SERVER - EF CORE
// -----------------------------------------------------
builder.Services.AddDbContext<ALOudDbContext>(options =>
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

// Auth Cookies
builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";

        options.ExpireTimeSpan = TimeSpan.FromHours(2);
    });

// MVC
builder.Services.AddControllersWithViews();



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
        var db = services.GetRequiredService<ALOudDbContext>();

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
app.UseAuthentication();
app.UseAuthorization();


app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
);
app.MapRazorPages();

// =====================================================
// RUN
// =====================================================
app.Run();
