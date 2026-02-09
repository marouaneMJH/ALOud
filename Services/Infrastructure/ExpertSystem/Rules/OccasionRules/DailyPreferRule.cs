using NRules.Fluent.Dsl;
using ALOud.Services.Infrastructure.ExpertSystem.Domain;


namespace ALOud.Services.Infrastructure.ExpertSystem.Rules.OccasionRules;

using ALOud.Services.Infrastructure.ExpertSystem.Domain;

public class DailyPreferRule : Rule
{
    public override void Define()
    {
        UserProfile user = null!;
        Recommendation rec = null!;

        When()
            .Match(() => user, u => u.Occasion == EOccasion.Daily)
            .Match(() => rec);

        Then()
            .Do(_ => Apply(rec));
    }

    private static void Apply(Recommendation rec)
    {
        rec.Prefer.UnionWith(new[]
        {
            "versatile",
            "woody",
            "musk",
            "citrus"
        });

        rec.Reasons.Add("Daily occasion → versatile woody, musk and citrus combination preferred");
    }
}
