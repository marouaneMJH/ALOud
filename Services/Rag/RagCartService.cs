using ALOud.Services.Rag.Models;
using System.Text.Json;
using System.Text.RegularExpressions;
using ALOud.Services.Rag.Clients;

namespace ALOud.Services.Rag;

public sealed class RagCartService
{
    private const int MaxToolCalls = 10;

    private readonly RagContextBuilder _contextBuilder;
    private readonly RagToolDispatcher _dispatcher;
    private readonly IRagLLMClient _llm;
    private readonly ILogger<RagCartService> _logger;

    public RagCartService(
        RagContextBuilder contextBuilder,
        RagToolDispatcher dispatcher,
        IRagLLMClient llm,
        ILogger<RagCartService> logger)
    {
        _contextBuilder = contextBuilder;
        _dispatcher = dispatcher;
        _llm = llm;
        _logger = logger;
    }

    public async Task<RagResponse> HandleAsync(string userMessage)
    {
        // Quick intent check for greetings - no LLM needed
        if (IsSimpleGreeting(userMessage))
        {
            return new RagResponse
            {
                Answer = "Hello! I'm the ALOud assistant. I can help you with:\n" +
                         "- Searching for perfumes\n" +
                         "- Recommending based on your preferences\n" +
                         "- Managing your cart\n\n" +
                         "How can I assist you?",
                CartSnapshot = null
            };
        }

        // Detect intent to select optimal tool subset (saves ~200 tokens)
        var intent = DetectIntent(userMessage);
        var tools = SelectToolsForIntent(intent);
        var systemPrompt = SelectPromptForIntent(intent);

        var cartContext = await _contextBuilder.BuildAsync();
        var conversationHistory = new List<ConversationTurn>();

        for (var step = 0; step < MaxToolCalls; step++)
        {
            try
            {
                var llmResult = await _llm.ExecuteAsync(new RagLLMRequest
                {
                    SystemPrompt = systemPrompt,
                    UserMessage = userMessage,
                    Context = cartContext,
                    ConversationHistory = conversationHistory,
                    Tools = tools
                });

                _logger.LogInformation($"[RAG Step {step + 1}/{MaxToolCalls}] Tool: {llmResult.ToolCall?.Name ?? "FINAL"}");

                // Final answer → stop
                if (!llmResult.IsToolCall)
                {
                    var answer = llmResult.FinalAnswer ?? "I couldn't process your request.";

                    // Smart fallback: if answer is too generic, provide suggestions
                    if (IsGenericResponse(answer))
                    {
                        answer = GenerateSmartFallback(userMessage, intent);
                    }

                    return new RagResponse
                    {
                        Answer = answer,
                        CartSnapshot = await _contextBuilder.BuildAsync()
                    };
                }

                // Tool execution
                var toolCall = llmResult.ToolCall!;

                conversationHistory.Add(new ConversationTurn
                {
                    Role = "assistant",
                    FunctionCallName = toolCall.Name,
                    FunctionCallArgs = toolCall.Arguments
                });

                var toolResult = await _dispatcher.DispatchAsync(
                    toolCall.Name,
                    toolCall.Arguments);

                conversationHistory.Add(new ConversationTurn
                {
                    Role = "tool",
                    FunctionName = toolCall.Name,
                    FunctionResponse = toolResult
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "RAG step {Step} failed", step);
                return new RagResponse
                {
                    Answer = GenerateSmartFallback(userMessage, intent),
                    CartSnapshot = await _contextBuilder.BuildAsync()
                };
            }
        }

        // Max tool calls reached - provide helpful response instead of error
        return new RagResponse
        {
            Answer = "I couldn't complete all the steps. " + GenerateSmartFallback(userMessage, intent),
            CartSnapshot = await _contextBuilder.BuildAsync()
        };
    }

    public async Task<string?> BuildCartContextAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Reuse existing cart context builder
            var cartContext = JsonSerializer.Serialize(await _contextBuilder.BuildAsync());
            ;

            if (string.IsNullOrWhiteSpace(cartContext))
                return null;

            return NormalizeCartContext(cartContext);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to build cart context for user {UserId}", userId);
            return null;
        }
    }


    private static string NormalizeCartContext(string rawContext)
    {
        return
    $"""
    USER CART SUMMARY:
    {rawContext.Trim()}

    Rules:
    - Prices and availability are informational only
    - Cart data reflects the current session
    """;
    }

    private enum UserIntent { Search, Cart, Recommend, General }

    private static bool IsSimpleGreeting(string message)
    {
        var greetings = new[] { "hi", "hello", "hey", "greetings", "good morning", "good afternoon", "good evening" };
        var normalized = message.Trim().ToLower();
        return greetings.Any(g => normalized == g || normalized.StartsWith(g + " ") || normalized.StartsWith(g + "!"));
    }

    private static UserIntent DetectIntent(string message)
    {
        var lower = message.ToLower();

        // Cart operations
        if (Regex.IsMatch(lower, @"\b(cart|add|remove|delete|increase|decrease|quantity|total|clear|checkout)\b"))
            return UserIntent.Cart;

        // Recommendations
        if (Regex.IsMatch(lower, @"\b(recommend|suggest|propose|advise|idea|taste|preference|woody|fresh|oriental|floral)\b"))
            return UserIntent.Recommend;

        // Search
        if (Regex.IsMatch(lower, @"\b(search|find|look|where|available|stock|price|which|show)\b"))
            return UserIntent.Search;

        return UserIntent.General;
    }

    private static IReadOnlyList<RagToolDefinition> SelectToolsForIntent(UserIntent intent)
    {
        return intent switch
        {
            UserIntent.Cart => RagToolCatalog.CartTools,
            UserIntent.Search => RagToolCatalog.SearchTools,
            UserIntent.Recommend => RagToolCatalog.SearchTools,
            _ => RagToolCatalog.All
        };
    }

    private static string SelectPromptForIntent(UserIntent intent)
    {
        return intent switch
        {
            UserIntent.Cart => SystemPrompts.CartOnlyPrompt,
            UserIntent.Search => SystemPrompts.SearchOnlyPrompt,
            UserIntent.Recommend => SystemPrompts.SearchOnlyPrompt,
            _ => SystemPrompts.CartAssistant
        };
    }

    private static bool IsGenericResponse(string answer)
    {
        var genericPhrases = new[] {
            "sorry", "i can't", "impossible", "unable",
            "error", "unavailable", "not found"
        };
        var lower = answer.ToLower();
        return genericPhrases.Any(p => lower.Contains(p));
    }

    private static string GenerateSmartFallback(string userMessage, UserIntent intent)
    {
        return intent switch
        {
            UserIntent.Cart =>
                "To manage your cart, you can:\n" +
                "- \"View my cart\"\n" +
                "- \"Add [product name]\"\n" +
                "- \"Analyze my cart\" for a complete summary",

            UserIntent.Search =>
                "To find a perfume, try:\n" +
                "- \"Search for Dior Sauvage\"\n" +
                "- \"Woody perfumes for men\"\n" +
                "- \"What perfumes do you have?\"",

            UserIntent.Recommend =>
                "For recommendations, tell me:\n" +
                "- Your preferred style (fresh, woody, oriental...)\n" +
                "- The occasion (daily, evening...)\n" +
                "- Your budget",

            _ =>
                "I'm your ALOud perfumery assistant. I can:\n" +
                "- Search for perfumes\n" +
                "- Make recommendations\n" +
                "- Manage your cart\n\n" +
                "How can I help you?"
        };
    }
}
