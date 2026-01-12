using ALOud.Services.Rag.Models;
using System.Text.RegularExpressions;

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
                Answer = "Bonjour! Je suis l'assistant ALOud. Je peux vous aider à:\n" +
                         "- Chercher des parfums\n" +
                         "- Recommander selon vos goûts\n" +
                         "- Gérer votre panier\n\n" +
                         "Que puis-je faire pour vous?",
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
                    var answer = llmResult.FinalAnswer ?? "Je n'ai pas pu traiter votre demande.";

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
            Answer = "Je n'ai pas pu compléter toutes les étapes. " + GenerateSmartFallback(userMessage, intent),
            CartSnapshot = await _contextBuilder.BuildAsync()
        };
    }

    private enum UserIntent { Search, Cart, Recommend, General }

    private static bool IsSimpleGreeting(string message)
    {
        var greetings = new[] { "hi", "hello", "salut", "bonjour", "salam", "hey", "coucou", "bonsoir" };
        var normalized = message.Trim().ToLower();
        return greetings.Any(g => normalized == g || normalized.StartsWith(g + " ") || normalized.StartsWith(g + "!"));
    }

    private static UserIntent DetectIntent(string message)
    {
        var lower = message.ToLower();

        // Cart operations
        if (Regex.IsMatch(lower, @"\b(panier|cart|ajoute|retire|supprime|augmente|diminue|quantity|total|vider)\b"))
            return UserIntent.Cart;

        // Recommendations
        if (Regex.IsMatch(lower, @"\b(recommande|suggère|propose|conseille|idée|goût|préférence|boisé|frais|oriental|floral)\b"))
            return UserIntent.Recommend;

        // Search
        if (Regex.IsMatch(lower, @"\b(cherche|trouve|search|où|avez.vous|dispo|stock|prix|quel)\b"))
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
            "désolé", "sorry", "je ne peux pas", "i can't", "impossible",
            "erreur", "error", "pas disponible", "unavailable"
        };
        var lower = answer.ToLower();
        return genericPhrases.Any(p => lower.Contains(p));
    }

    private static string GenerateSmartFallback(string userMessage, UserIntent intent)
    {
        return intent switch
        {
            UserIntent.Cart =>
                "Pour gérer votre panier, vous pouvez:\n" +
                "- \"Voir mon panier\"\n" +
                "- \"Ajouter [nom du produit]\"\n" +
                "- \"Analyser mon panier\" pour un résumé complet",

            UserIntent.Search =>
                "Pour trouver un parfum, essayez:\n" +
                "- \"Cherche Dior Sauvage\"\n" +
                "- \"Parfums boisés pour homme\"\n" +
                "- \"Quels parfums avez-vous?\"",

            UserIntent.Recommend =>
                "Pour des recommandations, dites-moi:\n" +
                "- Votre style préféré (frais, boisé, oriental...)\n" +
                "- L'occasion (quotidien, soirée...)\n" +
                "- Votre budget",

            _ =>
                "Je suis votre assistant parfumerie ALOud. Je peux:\n" +
                "- Chercher des parfums\n" +
                "- Faire des recommandations\n" +
                "- Gérer votre panier\n\n" +
                "Comment puis-je vous aider?"
        };
    }
}
