/**
 *  Convert a PerfumeRagSource into
 *  a semantic document string suitablefor embeddings.
 *  Here the document template
        Perfume Name
        Brand
        Category / Gender

        Olfactory Profile
        Usage & Performance

        Notes
        Accords

        Best Seasons
        Best Occasions

        Tags
*/



using System.Text;
using ALOud.Services.Rag.Models;

public class DocumentBuilderService : IDocumentBuilderService
{
    public string BuildDocument(PerfumeRagSource perfume)
    {
        var sb = new StringBuilder();

        sb.AppendLine($"Perfume Name: {perfume.Name}");
        sb.AppendLine($"Brand: {perfume.Brand}");

        if (!string.IsNullOrWhiteSpace(perfume.GenderProfile))
            sb.AppendLine($"Gender Profile: {perfume.GenderProfile}");

        sb.AppendLine($"Price: {perfume.Price} MAD");

        if (!string.IsNullOrWhiteSpace(perfume.PriceRange))
            sb.AppendLine($"Price Range: {perfume.PriceRange}");

        sb.AppendLine();

        if (!string.IsNullOrWhiteSpace(perfume.Description))
        {
            sb.AppendLine("Description:");
            sb.AppendLine(perfume.Description.Trim());
            sb.AppendLine();
        }

        sb.AppendLine("Performance Characteristics:");
        if (!string.IsNullOrWhiteSpace(perfume.Intensity))
            sb.AppendLine($"- Intensity: {perfume.Intensity}");
        if (!string.IsNullOrWhiteSpace(perfume.Longevity))
            sb.AppendLine($"- Longevity: {perfume.Longevity}");
        if (!string.IsNullOrWhiteSpace(perfume.Sillage))
            sb.AppendLine($"- Sillage: {perfume.Sillage}");
        sb.AppendLine();

        if (perfume.Families.Any())
        {
            sb.AppendLine("Olfactory Families:");
            sb.AppendLine(string.Join(", ", perfume.Families));
            sb.AppendLine();
        }

        if (perfume.Notes.Any())
        {
            sb.AppendLine("Notes:");
            foreach (var note in perfume.Notes)
            {
                sb.AppendLine($"- {note.Name} ({note.Category}, level: {note.Level})");
            }
            sb.AppendLine();
        }

        if (perfume.Accords.Any())
        {
            sb.AppendLine("Main Accords:");
            foreach (var accord in perfume.Accords)
            {
                sb.AppendLine($"- {accord.Name} (intensity: {accord.Intensity})");
            }
            sb.AppendLine();
        }

        if (perfume.Seasons.Any())
        {
            sb.AppendLine("Best Seasons:");
            sb.AppendLine(string.Join(", ", perfume.Seasons));
            sb.AppendLine();
        }

        if (perfume.Occasions.Any())
        {
            sb.AppendLine("Best Occasions:");
            sb.AppendLine(string.Join(", ", perfume.Occasions));
            sb.AppendLine();
        }

        if (perfume.Tags.Any())
        {
            sb.AppendLine("Tags:");
            sb.AppendLine(string.Join(", ", perfume.Tags));
        }

        return sb.ToString().Trim();
    }

}
