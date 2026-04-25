using NRules.Fluent.Dsl;
using ALOud.Services.Infrastructure.ExpertSystem.Domain;


namespace ALOud.Services.Infrastructure.ExpertSystem.Rules.PerformanceRules;

public class NightlifeLongevityRule : Rule
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
        rec.Longevity = ">= long";
        rec.Reasons.Add("Nightlife occasion → longevity should be long");
    }
}
