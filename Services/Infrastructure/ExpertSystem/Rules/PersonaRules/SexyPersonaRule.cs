using NRules.Fluent.Dsl;
using ALOud.Services.Infrastructure.ExpertSystem.Domain;


namespace ALOud.Services.Infrastructure.ExpertSystem.Rules.PersonaRules;

public class SexyPersonaRule : Rule
{
    public override void Define()
    {
        UserProfile user = null!;
        Recommendation rec = null!;

        When()
            .Match(() => user, u => u.Persona == EPersona.Sexy)
            .Match(() => rec);

        Then()
            .Do(_ => Apply(rec));
    }

    private static void Apply(Recommendation rec)
    {
        rec.Prefer.UnionWith(new[]
        {
            "oriental",
            "gourmand",
            "amber"
        });

        rec.Reasons.Add("Sexy persona → oriental, gourmand and amber olfactive family preferred");
    }
}
