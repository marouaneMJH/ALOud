using NRules.Fluent.Dsl;

namespace ALOud.Services.Infrastructure.ExpertSystem.Rules.PersonaRules;

public class MinimalistPersonaRule : Rule
{
    public override void Define()
    {
        UserProfile user = null!;
        Recommendation rec = null!;

        When()
            .Match(() => user, u => u.Persona == EPersona.Minimalist)
            .Match(() => rec);

        Then()
            .Do(_ => Apply(rec));
    }

    private static void Apply(Recommendation rec)
    {
        rec.Prefer.UnionWith(new[]
        {
            "iso_e_super",
            "musk",
            "vetiver",
            "cedar"
        });

        rec.Reasons.Add("Minimalist persona → iso e super, musk, vetiver and cedar olfactive family preferred");
    }
}
