using NRules.Fluent.Dsl;

namespace ALOud.Services.Infrastructure.ExpertSystem.Rules.PersonaRules;

public class ElegantPersonaRule : Rule
{
    public override void Define()
    {
        UserProfile user = null!;
        Recommendation rec = null!;

        When()
            .Match(() => user, u => u.Persona == EPersona.Elegant)
            .Match(() => rec);

        Then()
            .Do(_ => Apply(rec));
    }

    private static void Apply(Recommendation rec)
    {
        rec.Prefer.UnionWith(new[]
        {
            "chypre",
            "floral",
            "musk",
            "amber"
        });

        rec.Reasons.Add("Elegant persona → chypre, floral, musk and amber olfactive family preferred");
    }
}
