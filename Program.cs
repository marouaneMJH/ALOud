using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using ALOud.Data;
using ALOud.Services;
using ALOud.Services.Security;
using ALOud.Services.Rag;
using ALOud.Services.Rag.Models;
using ALOud.Services.Brand;
using ALOud.Services.Perfume;
using ALOud.Services.Family;
using ALOud.Services.Note;
using ALOud.Services.Accord;
using ALOud.Services.Tag;
using ALOud.Services.Season;
using ALOud.Services.Occasion;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// =====================================================
// ENVIRONMENT VARIABLES (.env loader)
// =====================================================
var envPath = Path.Combine(builder.Environment.ContentRootPath, ".env");
if (File.Exists(envPath))
{
    foreach (var line in File.ReadAllLines(envPath))
    {
        var trimmed = line.Trim();
        if (string.IsNullOrEmpty(trimmed) || trimmed.StartsWith("#"))
            continue;

        var idx = trimmed.IndexOf('=');
        if (idx <= 0)
            continue;

        var key = trimmed[..idx].Trim();
        var value = trimmed[(idx + 1)..].Trim();

        // Remove optional surrounding quotes
        if ((value.StartsWith("\"") && value.EndsWith("\"")) ||
            (value.StartsWith("'") && value.EndsWith("'")))
        {
            value = value[1..^1];
        }

        Environment.SetEnvironmentVariable(key, value);
    }
}

// =====================================================
// SMTP CONFIGURATION
// =====================================================
builder.Services.Configure<SmtpOptions>(options =>
{
    var smtpSection = builder.Configuration.GetSection("Smtp");

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

// =====================================================
// REDIS
// =====================================================
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var logger = sp.GetRequiredService<ILogger<Program>>();
    var redisConnection = builder.Configuration.GetSection("Redis")["ConnectionString"];

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
        logger.LogCritical(ex, "[-] Redis connection failed");
        throw;
    }
});

// Redis cache abstraction
builder.Services.AddScoped<ICacheService, RedisCacheService>();

// =====================================================
// HTTP CONTEXT (required for cart & cookies)
// =====================================================
builder.Services.AddHttpContextAccessor();

// =====================================================
// AUTHENTICATION (Cookies)
// =====================================================
builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.ExpireTimeSpan = TimeSpan.FromHours(2);
    });

// =====================================================
// CORE BUSINESS SERVICES
// =====================================================

// User & security
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<PasswordHasherService>();

// Email & verification
builder.Services.AddScoped<IEmailService, SmtpEmailService>();
builder.Services.AddScoped<IVerificationService, VerificationService>();

// Admin / domain services
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<LLMConfigService>();

// Perfume domain services
builder.Services.AddScoped<IBrandService, BrandService>();
builder.Services.AddScoped<IPerfumeService, PerfumeService>();
builder.Services.AddScoped<IFamilyService, FamilyService>();
builder.Services.AddScoped<INoteService, NoteService>();
builder.Services.AddScoped<IAccordService, AccordService>();
builder.Services.AddScoped<ITagService, TagService>();
builder.Services.AddScoped<ISeasonService, SeasonService>();
builder.Services.AddScoped<IOccasionService, OccasionService>();

// Cart (Redis + cookies)
builder.Services.AddScoped<CartService>();

// =====================================================
// RAG – CORE
// =====================================================
builder.Services.AddScoped<RagContextBuilder>();
builder.Services.AddScoped<RagToolDispatcher>();
builder.Services.AddScoped<RagCartService>();

// =====================================================
// LLM CLIENT (FACTORY PATTERN - ENV CONFIGURED)
// =====================================================
builder.Services.AddHttpClient("LLMClient");
builder.Services.AddScoped<LLMClientFactory>();
builder.Services.AddScoped<IRagLLMClient>(sp =>
{
    var factory = sp.GetRequiredService<LLMClientFactory>();
    return factory.CreateClient();
});

// =====================================================
// DATA ACCESS (EF CORE – SQL SERVER)
// =====================================================
builder.Services.AddDbContext<ALOudDbContext>(options =>
{
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sql =>
        {
            sql.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(5),
                errorNumbersToAdd: null);
        });
});

// =====================================================
// MVC / RAZOR
// =====================================================
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// =====================================================
// BUILD APPLICATION
// =====================================================
var app = builder.Build();

// =====================================================
// STARTUP CHECKS (FAIL FAST)
// =====================================================
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();

    // SQL Server connectivity check
    try
    {
        var db = services.GetRequiredService<ALOudDbContext>();
        if (!db.Database.CanConnect())
            throw new Exception("Database not reachable");

        logger.LogInformation("[+] SQL Server connection OK");
    }
    catch (Exception ex)
    {
        logger.LogCritical(ex, "[-] Database startup check failed");
        throw;
    }

    // Redis connectivity check
    try
    {
        var redis = services.GetRequiredService<IConnectionMultiplexer>();
        var ping = redis.GetDatabase().Ping();

        logger.LogInformation("[+] Redis ping OK ({Ping} ms)", ping.TotalMilliseconds);
    }
    catch (Exception ex)
    {
        logger.LogCritical(ex, "[-] Redis startup check failed");
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
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

// =====================================================
// RUN
// =====================================================
app.Run();
