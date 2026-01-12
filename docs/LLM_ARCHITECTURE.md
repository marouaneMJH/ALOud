# LLM Provider Architecture

## Overview

The application uses a generic LLM client architecture that supports multiple AI providers (Gemini, Groq, Grok) with easy switching via configuration.

## Architecture

### Components

1. **IRagLLMClient** - Interface defining LLM operations
2. **BaseLLMClient** - Abstract base class with common functionality
3. **Provider Implementations**:
    - `GeminiLLMClient` - Google Gemini integration
    - `GroqLLMClient` - Groq API integration
    - `GrokLLMClient` - xAI Grok integration
4. **LLMClientFactory** - Factory for creating provider instances
5. **LLMProviderConfig** - Configuration model

## Configuration

### appsettings.json

```json
{
    "LLMProvider": {
        "Provider": "gemini",
        "Gemini": {
            "Model": "gemini-1.5-flash",
            "ApiKeyEnvVar": "GEMINI_API_KEY"
        },
        "Groq": {
            "Model": "llama-3.1-8b-instant",
            "ApiKeyEnvVar": "GROQ_API_KEY"
        },
        "Grok": {
            "Model": "grok-beta",
            "ApiKeyEnvVar": "GROK_API_KEY"
        }
    }
}
```

### Switch Providers

Change the `Provider` value to:

-   `"gemini"` - Use Google Gemini
-   `"groq"` - Use Groq
-   `"grok"` - Use xAI Grok

### Environment Variables

Set the appropriate API key:

```bash
export GEMINI_API_KEY=your-key-here
# or
export GROQ_API_KEY=your-key-here
# or
export GROK_API_KEY=your-key-here
```

## Adding New Providers

1. Create new class inheriting from `BaseLLMClient`:

```csharp
public class NewProviderClient : BaseLLMClient
{
    public NewProviderClient(HttpClient http, ILogger<NewProviderClient> logger, NewProviderSettings settings)
        : base(http, logger, settings.ApiKeyEnvVar)
    {
    }

    protected override string GetProviderName() => "NewProvider";
    protected override string BuildRequestUrl() => "https://api.newprovider.com/...";
    protected override object BuildPayload(RagLLMRequest request) { /* ... */ }
    protected override RagLLMResult ParseResponse(JsonDocument doc) { /* ... */ }
}
```

2. Add to `LLMProviderConfig`:

```csharp
public class NewProviderSettings
{
    public string Model { get; set; } = "default-model";
    public string ApiKeyEnvVar { get; set; } = "NEWPROVIDER_API_KEY";
}
```

3. Update `LLMClientFactory`:

```csharp
"newprovider" => new NewProviderClient(
    httpClient,
    _loggerFactory.CreateLogger<NewProviderClient>(),
    _config.NewProvider),
```

## Admin Control Panel (Future)

To allow admin selection of providers:

1. Create Admin Controller:

```csharp
[ApiController]
[Route("api/admin/llm")]
public class LLMAdminController : ControllerBase
{
    [HttpPost("switch")]
    public IActionResult SwitchProvider([FromBody] string provider)
    {
        // Update configuration
        // Restart services or reload config
    }
}
```

2. Create Admin UI page:

-   Display available providers
-   Show current provider
-   Allow switching with dropdown
-   Display API key status (set/not set)

3. Store configuration in database:

```csharp
public class SystemConfig
{
    public string LLMProvider { get; set; }
    public Dictionary<string, string> ProviderSettings { get; set; }
}
```

## Benefits

-   ✅ **Easy Provider Switching** - Change one config value
-   ✅ **Consistent Interface** - All providers use same interface
-   ✅ **Shared Logic** - Common error handling, logging, retries
-   ✅ **Extensible** - Add new providers easily
-   ✅ **Testable** - Mock factory for unit tests
-   ✅ **Type-Safe** - Compile-time checks for provider methods

## Current Status

-   ✅ Architecture implemented
-   ✅ Factory pattern configured
-   ✅ Gemini fully refactored
-   ⏳ Groq refactoring needed
-   ⏳ Grok refactoring needed
-   ⏳ Admin UI not yet implemented
