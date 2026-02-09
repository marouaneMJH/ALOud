using NRules.Fluent.Dsl; using ALOud.Services.Infrastructure.ExpertSystem.Domain;

using ALOud.Services.Infrastructure.ExpertSystem.Domain;

namespace ALOud.Services.Infrastructure.ExpertSystem.Rules.ComplimentRules;

public class ComplimentNightlifeRule : Rule
{
    public override void Define()
    {
        UserProfile user = null!;
        Recommendation rec = null!;

        When()
            .Match(() => user, u => u.Compliment == EComplimentDesire.Yes && u.Occasion == EOccasion.Nightlife)
            .Match(() => rec);

        Then()
            .Do(_ => Apply(rec));
    }

    private static void Apply(Recommendation rec)
    {
        rec.Prefer.UnionWith(new[]
        {
            "sweet",
            "strong",
            "long_lasting"
        });

        rec.Sillage = ">= moderate";
        rec.Longevity = ">= long";

        rec.Reasons.Add("Compliment desire + Nightlife → sweet, strong and long lasting output");
    }
}
