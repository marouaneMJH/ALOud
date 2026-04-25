using System.Text.RegularExpressions;
using ALOud.Models.Cart;
using ALOud.Services.Infrastructure.Rag.Agent;
using ALOud.Services.Cart;
using ALOud.Services.Rag.Clients;
using ALOud.Services.Infrastructure.Rag.Models;

namespace ALOud.Services.Rag;

public sealed class RagCartService
{
    private const int MaxToolCalls = 10;

    private readonly RagToolDispatcher _dispatcher;
    private readonly IRagLLMClient _agentLlm;
    private readonly IRagAnswerService _ragAnswerService;
    private readonly ICartContextBuilder _cartContextBuilder;
    private readonly ILogger<RagCartService> _logger;

    public RagCartService(
        RagToolDispatcher dispatcher,
        IRagLLMClient agentLlm,
        IRagAnswerService ragAnswerService,
        ICartContextBuilder cartContextBuilder,
        ILogger<RagCartService> logger)
    {
        _dispatcher = dispatcher;
        _agentLlm = agentLlm;
        _ragAnswerService = ragAnswerService;
        _cartContextBuilder = cartContextBuilder;
        _logger = logger;
    }

    public async Task<RagResponse> HandleAsync(string userMessage)
    {
        if (string.IsNullOrWhiteSpace(userMessage))
        {
            return new RagResponse
            {
                Answer = "Please enter a message.",
                CartSnapshot = null
            };
        }

        // Greetings shortcut (no LLM)
        if (IsSimpleGreeting(userMessage))
        {
            return new RagResponse
            {
                Answer =
                    "Hello. I can help you search perfumes, get recommendations, or manage your cart.",
                CartSnapshot = null
            };
        }

        // TODO: replace with authenticated user id
        var userId = Guid.NewGuid();

        var intent = DetectIntent(userMessage);

        // -----------------------------
        // PURE RAG (knowledge only)
        // -----------------------------
        if (intent is UserIntent.Search or UserIntent.Recommend)
        {
            _logger.LogInformation("Routing to pure RAG. Intent={Intent}", intent);

            var ragResult = await _ragAnswerService.AnswerAsync(
                userId,
                userMessage,
                new RagAnswerOptions
                {
                    Debug = false,
                    TopK = 5
                });

            return new RagResponse
            {
                Answer = ragResult.Answer,
                CartSnapshot = null
            };
        }

        // -----------------------------
        // AGENT (tools + cart)
        // -----------------------------
        _logger.LogInformation("Routing to agent. Intent={Intent}", intent);

        var cartContext = await _cartContextBuilder.BuildAsync(userId);

        var systemPrompt = SelectPromptForIntent(intent);
        var tools = SelectToolsForIntent(intent);
        var conversationHistory = new List<ConversationTurn>();

        for (var step = 0; step < MaxToolCalls; step++)
        {
            var llmResult = await _agentLlm.ExecuteAsync(new RagLLMRequest
            {
                SystemPrompt = systemPrompt,
                UserMessage = userMessage,
                Context = cartContext, // agent gets LIVE cart context
                ConversationHistory = conversationHistory,
                Tools = tools
            });

            _logger.LogInformation(
                "[Agent Step {Step}/{Max}] Result={Result}",
                step + 1,
                MaxToolCalls,
                llmResult.ToolCall?.Name ?? "FINAL"
            );

            // Final answer
            if (!llmResult.IsToolCall)
            {
                return new RagResponse
                {
                    Answer = llmResult.FinalAnswer
                        ?? GenerateFallback(intent),
                    CartSnapshot = cartContext
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

        return new RagResponse
        {
            Answer = GenerateFallback(intent),
            CartSnapshot = cartContext
        };
    }

    // ============================
    // Intent detection
    // ============================

    private enum UserIntent
    {
        Search,
        Recommend,
        Cart,
        General
    }

    private static bool IsSimpleGreeting(string message)
    {
        var normalized = message.Trim().ToLower();
        return normalized is "hi" or "hello" or "hey"
            || normalized.StartsWith("good ");
    }

    private static UserIntent DetectIntent(string message)
    {
        var lower = message.ToLower();

        if (Regex.IsMatch(lower, @"\b(add|remove|increase|decrease|cart|checkout|clear)\b"))
            return UserIntent.Cart;

        if (Regex.IsMatch(lower, @"\b(recommend|suggest|woody|fresh|oriental|floral)\b"))
            return UserIntent.Recommend;

        if (Regex.IsMatch(lower, @"\b(search|find|price|available|which|compare)\b"))
            return UserIntent.Search;

        return UserIntent.General;
    }

    // ============================
    // Prompt & tool selection
    // ============================

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
            UserIntent.Recommend => SystemPrompts.ShoppingAssistant,
            _ => SystemPrompts.CartAssistant
        };
    }

    private static string GenerateFallback(UserIntent intent)
    {
        return intent switch
        {
            UserIntent.Cart =>
                "You can manage your cart by adding, removing, or updating products.",

            UserIntent.Search =>
                "You can search for perfumes by name, style, or preference.",

            UserIntent.Recommend =>
                "Tell me your preferences and I can recommend perfumes.",

            _ =>
                "I can help you search perfumes or manage your cart."
        };
    }
}
