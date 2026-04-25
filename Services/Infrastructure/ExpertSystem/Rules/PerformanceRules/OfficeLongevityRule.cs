using NRules.Fluent.Dsl;
using ALOud.Services.Infrastructure.ExpertSystem.Domain;


namespace ALOud.Services.Infrastructure.ExpertSystem.Rules.PerformanceRules;

public class OfficeLongevityRule : Rule
{
    public override void Define()
    {
        UserProfile user = null!;
        Recommendation rec = null!;

        When()
            .Match(() => user, u => u.Occasion == EOccasion.Office)
            .Match(() => rec);

        Then()
            .Do(_ => Apply(rec));
    }

    private static void Apply(Recommendation rec)
    {
        rec.Longevity = "<= medium";
        rec.Reasons.Add("Office occasion → longevity should be medium or less");
    }
}
