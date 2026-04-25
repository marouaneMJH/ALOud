namespace ALOud.Services.Infrastructure.ExpertSystem.Domain;

public class Recommendation
{
    public HashSet<string> Prefer { get; } = new();
    public HashSet<string> Avoid { get; } = new();

    public string? Sillage { get; set; }
    public string? Longevity { get; set; }

    public List<string> Reasons { get; } = new();
}
