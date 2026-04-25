namespace ALOud.DTOs.Rag;

public sealed class RagChatRequest
{
    public string Message { get; set; } = string.Empty;

    // Optional testing knobs
    public int TopK { get; set; } = 5;

    public string? Brand { get; set; }
    public string? GenderProfile { get; set; }
    public string? PriceRange { get; set; }

    public bool Debug { get; set; } = false;
}
