using NRules.Fluent.Dsl;
using ALOud.Services.Infrastructure.ExpertSystem.Domain;


namespace ALOud.Services.Infrastructure.ExpertSystem.Rules.OccasionRules;

public class NightlifePreferRule : Rule
{
    public override void Define()
    {
        UserProfile user = null!;
        Recommendation rec = null!;

        When()
            .Match(() => user, u => u.Occasion == EOccasion.Nightlife)
            .Match(() => rec);

        Then()
            .Do(_ => Apply(rec));
    }

    private static void Apply(Recommendation rec)
    {
        rec.Prefer.UnionWith(new[]
        {
            "gourmand",
            "oriental",
            "sweet_high"
        });

        rec.Reasons.Add("Nightlife occasion → gourmand, oriental and sweet notes preferred");
    }
}
