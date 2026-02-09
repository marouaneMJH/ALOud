namespace ALOud.Services.Rag.Models;

/// <summary>
/// Configuration for LLM provider selection - reads from environment variables
/// </summary>
public class LLMProviderConfig
{
    /// <summary>
    /// Active provider: "gemini", "groq", "grok"
    /// Read from LLM_PROVIDER environment variable
    /// </summary>
    public string Provider { get; }

    /// <summary>
    /// Gemini-specific settings
    /// </summary>
    public GeminiSettings Gemini { get; }

    /// <summary>
    /// Groq-specific settings
    /// </summary>
    public GroqSettings Groq { get; }

    /// <summary>
    /// Grok (xAI) specific settings
    /// </summary>
    public GrokSettings Grok { get; }

    public LLMProviderConfig()
    {
        // Read provider from environment variable, default to gemini
        Provider = Environment.GetEnvironmentVariable("LLM_PROVIDER")?.ToLower() ?? "gemini";

        // Initialize provider-specific settings
        Gemini = new GeminiSettings();
        Groq = new GroqSettings();
        Grok = new GrokSettings();
    }
}

public class GeminiSettings
{
    public string Model { get; }
    public string ApiKeyEnvVar { get; }

    public GeminiSettings()
    {
        Model = Environment.GetEnvironmentVariable("GEMINI_MODEL") ?? "gemini-1.5-flash";
        ApiKeyEnvVar = "GEMINI_API_KEY";
    }
}

public class GroqSettings
{
    public string Model { get; }
    public string ApiKeyEnvVar { get; }

    public GroqSettings()
    {
        // Use llama-3.3-70b-versatile for better tool following (8b hallucinates brave_search)
        Model = Environment.GetEnvironmentVariable("GROQ_MODEL") ?? "llama-3.3-70b-versatile";
        ApiKeyEnvVar = "GROQ_API_KEY";
    }
}

public class GrokSettings
{
    public string Model { get; }
    public string ApiKeyEnvVar { get; }

    public GrokSettings()
    {
        Model = Environment.GetEnvironmentVariable("GROK_MODEL") ?? "grok-beta";
        ApiKeyEnvVar = "GROK_API_KEY";
    }
}
public class QdrantSettings
{
    public string BaseUrl { get; }
    public string Collection { get; }
    public int VectorSize { get; }


    public QdrantSettings()
    {
        BaseUrl = Environment.GetEnvironmentVariable("QDRANT_BASE_URL") ?? "http://localhost:6333";
        Collection = Environment.GetEnvironmentVariable("QDRANT_COLLECTION") ?? "perfumes";
        VectorSize = 768;
    }
}


