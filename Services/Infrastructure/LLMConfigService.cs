using System.Text.RegularExpressions;

namespace ALOud.Services;

/// <summary>
/// Service for managing LLM provider configuration in .env file
/// </summary>
public class LLMConfigService
{
    private readonly string _envFilePath;
    private readonly ILogger<LLMConfigService> _logger;

    public LLMConfigService(IWebHostEnvironment env, ILogger<LLMConfigService> logger)
    {
        _envFilePath = Path.Combine(env.ContentRootPath, ".env");
        _logger = logger;
    }

    public string GetCurrentProvider()
    {
        return Environment.GetEnvironmentVariable("LLM_PROVIDER")?.ToLower() ?? "gemini";
    }

    public List<ProviderInfo> GetAvailableProviders()
    {
        var currentProvider = GetCurrentProvider();

        return new List<ProviderInfo>
        {
            new ProviderInfo
            {
                Id = "gemini",
                Name = "Google Gemini",
                Description = "Google's Gemini AI model - Fast and reliable",
                Model = Environment.GetEnvironmentVariable("GEMINI_MODEL") ?? "gemini-1.5-flash",
                IsActive = currentProvider == "gemini",
                HasApiKey = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("GEMINI_API_KEY"))
            },
            new ProviderInfo
            {
                Id = "groq",
                Name = "Groq",
                Description = "Groq's LPU-powered inference - Ultra fast",
                Model = Environment.GetEnvironmentVariable("GROQ_MODEL") ?? "llama-3.1-8b-instant",
                IsActive = currentProvider == "groq",
                HasApiKey = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("GROQ_API_KEY"))
            },
            new ProviderInfo
            {
                Id = "grok",
                Name = "Grok (xAI)",
                Description = "xAI's Grok model - Powerful reasoning",
                Model = Environment.GetEnvironmentVariable("GROK_MODEL") ?? "grok-beta",
                IsActive = currentProvider == "grok",
                HasApiKey = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("GROK_API_KEY"))
            }
        };
    }

    public async Task<bool> SwitchProvider(string providerId)
    {
        try
        {
            if (!File.Exists(_envFilePath))
            {
                _logger.LogError($".env file not found at {_envFilePath}");
                return false;
            }

            var lines = await File.ReadAllLinesAsync(_envFilePath);
            var updated = false;

            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].StartsWith("LLM_PROVIDER="))
                {
                    lines[i] = $"LLM_PROVIDER={providerId}";
                    updated = true;
                    break;
                }
            }

            if (!updated)
            {
                // Add LLM_PROVIDER if it doesn't exist
                var newLines = new List<string>(lines) { $"LLM_PROVIDER={providerId}" };
                lines = newLines.ToArray();
            }

            await File.WriteAllLinesAsync(_envFilePath, lines);

            // Update environment variable in current process
            Environment.SetEnvironmentVariable("LLM_PROVIDER", providerId);

            _logger.LogInformation($"Switched LLM provider to: {providerId}");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to switch LLM provider to {providerId}");
            return false;
        }
    }
}

public class ProviderInfo
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public bool HasApiKey { get; set; }
}
