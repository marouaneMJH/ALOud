using NRules.Fluent.Dsl;
using ALOud.Services.Infrastructure.ExpertSystem.Domain;


namespace ALOud.Services.Infrastructure.ExpertSystem.Rules.PersonaRules;

public class YouthfulPersonaRule : Rule
{
    public override void Define()
    {
        UserProfile user = null!;
        Recommendation rec = null!;

        When()
            .Match(() => user, u => u.Persona == EPersona.Youthful)
            .Match(() => rec);

        Then()
            .Do(_ => Apply(rec));
    }

    private static void Apply(Recommendation rec)
    {
        rec.Prefer.UnionWith(new[]
        {
            "sweet",
            "fruity",
            "gourmand"
        });

        rec.Reasons.Add("Youthful persona → sweet, fruity and gourmand olfactive family preferred");
    }
}
