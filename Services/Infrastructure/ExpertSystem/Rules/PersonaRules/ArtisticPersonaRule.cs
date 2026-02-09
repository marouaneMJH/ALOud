using NRules.Fluent.Dsl;
using ALOud.Services.Infrastructure.ExpertSystem.Domain;


namespace ALOud.Services.Infrastructure.ExpertSystem.Rules.PersonaRules;

public class ArtisticPersonaRule : Rule
{
    public override void Define()
    {
        UserProfile user = null!;
        Recommendation rec = null!;

        When()
            .Match(() => user, u => u.Persona == EPersona.Artistic)
            .Match(() => rec);

        Then()
            .Do(_ => Apply(rec));
    }

    private static void Apply(Recommendation rec)
    {
        rec.Prefer.UnionWith(new[]
        {
            "chypre",
            "leather",
            "incense",
            "niche"
        });

        rec.Reasons.Add("Artistic persona → chypre, leather, incense and niche olfactive family preferred");
    }
}
