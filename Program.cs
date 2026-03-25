using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using ALOud.Data;
using ALOud.Services;
using ALOud.Services.Security;
using ALOud.Services.Rag;
using ALOud.Services.Rag.Clients;
using ALOud.Services.Infrastructure.ExpertSystem;
using ALOud.Services.Infrastructure.Rag.Clients;
using ALOud.Services.Infrastructure.Rag.ChatAPI;
using ALOud.Services.Infrastructure.Rag.IndexingJob;
using ALOud.Services.Brand;
using ALOud.Services.Perfume;
using ALOud.Services.Family;
using ALOud.Services.Note;
using ALOud.Services.Accord;
using ALOud.Services.Tag;
using ALOud.Services.Season;
using ALOud.Services.Occasion;
using ALOud.Services.Data;
using ALOud.Services.Cart;
using ALOud.Repositories;

using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Net;
using ALOud.Services.Rag.IndexingJob;
using Microsoft.Extensions.Options;
using ALOud.Services.Infrastructure.Rag.Models;
using ALOud.Services.Infrastructure.Cache;
using System.Text.Json.Serialization;
using System.Text;
using ALOud.Common;

var builder = WebApplication.CreateBuilder(args);

// Load environment variables
builder.LoadEnvironmentVariables();

// Configure SMTP
builder.Services.ConfigureSmtp(builder.Configuration);

// Configure Redis
builder.Services.ConfigureRedis(builder.Configuration);

// HTTP context accessor for services that use request/response context (e.g., cart cookie key)
builder.Services.AddHttpContextAccessor();

// Cache abstraction used by CartService and dependent RAG/cart components
builder.Services.AddScoped<ICacheService, RedisCacheService>();

// JWT configuration
var jwtSecretKey = Environment.GetEnvironmentVariable("JWT_SECRET_KEY") ?? "default-secret-key-change-in-production";
var jwtIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER") ?? "ALOudAPI";
var jwtAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE") ?? "ALOudAPI";

builder.Services
    .AddAuthentication(options =>
    {
        // Use a custom handler that can switch between Cookie and JWT
        options.DefaultAuthenticateScheme = "MultiAuth";
        options.DefaultChallengeScheme = "MultiAuth";
    })
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.ExpireTimeSpan = TimeSpan.FromHours(2);
    })
    .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecretKey)),
            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,
            ValidateAudience = true,
            ValidAudience = jwtAudience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };

        options.Events = new JwtBearerEvents
        {
            OnChallenge = context =>
            {
                context.HandleResponse();
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                context.Response.ContentType = "application/json";
                return context.Response.WriteAsJsonAsync(new
                {
                    error = "Unauthorized",
                    message = "Invalid or missing authorization token"
                });
            }
        };
    })
    .AddPolicyScheme("MultiAuth", "Cookie or JWT", options =>
    {
        options.ForwardDefaultSelector = context =>
        {
            var authHeader = context.Request.Headers.Authorization.FirstOrDefault();
            if (authHeader?.StartsWith("Bearer ") == true)
            {
                return JwtBearerDefaults.AuthenticationScheme;
            }

            return CookieAuthenticationDefaults.AuthenticationScheme;
        };
    });

// Register JWT Token Service
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();

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
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<LLMConfigService>();

// Cart
builder.Services.AddScoped<ICartContextBuilder, CartContextBuilder>();


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
builder.Services.AddScoped<ICartService, CartService>();

// =====================================================
// Expert System – CORE
// =====================================================
builder.Services.AddSingleton<IExpertSystemService, ExpertSystemService>();
builder.Services.AddScoped<IHybridExpertSystemService, HybridExpertSystemService>();


// =====================================================
// RAG – CORE
// =====================================================
builder.Services.AddScoped<IRagAnswerService, RagAnswerService>();
builder.Services.AddScoped<RagToolDispatcher>();
builder.Services.AddScoped<RagCartService>();

// =====================
// RAG – Indexing pipeline
// =====================
builder.Services.AddScoped<IProductDataExtractor, ProductDataExtractor>();
builder.Services.AddScoped<IDocumentBuilderService, DocumentBuilderService>();
builder.Services.AddScoped<IChunkingService, ChunkingService>();
builder.Services.AddScoped<IEmbeddingIndexService, EmbeddingIndexService>();
builder.Services.AddScoped<IVectorIndexService, VectorIndexService>();

// --------------------
// Qdrant settings
// --------------------
builder.Services.Configure<QdrantSettings>(
    builder.Configuration.GetSection("Qdrant"));

builder.Services.AddSingleton(sp =>
    sp.GetRequiredService<IOptions<QdrantSettings>>().Value);

// --------------------
// Qdrant clients
// --------------------
builder.Services.AddHttpClient<QdrantVectorDbClient>();
builder.Services.AddScoped<IVectorDbClient, QdrantVectorDbClient>();

builder.Services.AddHttpClient<QdrantVectorSearchClient>();
builder.Services.AddScoped<IVectorSearchClient, QdrantVectorSearchClient>();

builder.Services.AddHttpClient<QdrantPayloadSearchClient>();
builder.Services.AddScoped<IPayloadSearchClient, QdrantPayloadSearchClient>();

// --------------------
// Qdrant bootstrap
// --------------------
builder.Services.AddHttpClient<QdrantBootstrapService>();
builder.Services.AddScoped<QdrantBootstrapService>();

builder.Services.AddHostedService<RagIndexingHostedService>();

// =====================
// RAG – Runtime (Chat)
// =====================
builder.Services.AddScoped<IQueryEmbeddingService, QueryEmbeddingService>();
builder.Services.AddScoped<IRetrievalService, RetrievalService>();
builder.Services.AddScoped<IContextBuilderService, ContextBuilderService>();
builder.Services.AddScoped<ILlmGenerationService, LlmGenerationService>();
builder.Services.AddScoped<IChatOrchestratorService, ChatOrchestratorService>();
builder.Services.AddHttpClient<OllamaEmbeddingClient>();
builder.Services.AddScoped<IEmbeddingClient, OllamaEmbeddingClient>();

// =====================================================
// LLM CLIENT (FACTORY PATTERN - ENV CONFIGURED)
// =====================================================
builder.Services.AddHttpClient("LLMClient")
    .ConfigurePrimaryHttpMessageHandler(() =>
        new HttpClientHandler
        {
            AutomaticDecompression = DecompressionMethods.All
        });
builder.Services.AddScoped<LLMClientFactory>();
builder.Services.AddScoped<IRagLLMClient>(sp =>
{
    var factory = sp.GetRequiredService<LLMClientFactory>();
    return factory.CreateClient();
});

// =====================================================
// DATA ACCESS (EF CORE – SQL SERVER)
// =====================================================
builder.Services.ConfigureDatabase(builder.Configuration);

// =====================================================
// REPOSITORY LAYER (DATA ACCESS ABSTRACTION)
// =====================================================
// Generic repository
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

// Specific repositories
builder.Services.AddScoped<IPerfumeRepository, PerfumeRepository>();

// Unit of Work pattern
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// =====================================================
// CORS CONFIGURATION
// =====================================================
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy
            .WithOrigins(
                "http://localhost:5173",   // Vite dev server (typical)
                "http://localhost:3000",   // Alternative frontend port
                "http://localhost:5174",   // Alternative Vite port
                "http://localhost:5175"    // Another alternative
            )
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

// =====================================================
// AUTO MAPPER - String to Enum
// =====================================================
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(
            new JsonStringEnumConverter()
        );
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
await app.PerformStartupChecksAsync();

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

// Enable CORS middleware (must be after UseRouting and before UseAuthentication)
app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseAuthorization();

// Configure routing for MVC and Razor Pages
// MVC routes for controllers
app.MapControllerRoute(
    name: "perfumeDetails",
    pattern: "Perfume/Details/{id:guid}",
    defaults: new { controller = "Perfume", action = "Details" });

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Perfume}/{action=Index}/{id?}");

// =====================================================
// RUN
// =====================================================
await app.RunAsync();
