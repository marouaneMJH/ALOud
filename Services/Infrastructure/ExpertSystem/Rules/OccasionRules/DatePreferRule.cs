using NRules.Fluent.Dsl;

namespace ALOud.Services.Infrastructure.ExpertSystem.Rules.OccasionRules;

public class DatePreferRule : Rule
{
    public override void Define()
    {
        UserProfile user = null!;
        Recommendation rec = null!;

        When()
            .Match(() => user, u => u.Occasion == EOccasion.Date)
            .Match(() => rec);

        Then()
            .Do(_ => Apply(rec));
    }

    private static void Apply(Recommendation rec)
    {
        rec.Prefer.UnionWith(new[]
        {
            "warm",
            "intimate",
            "sexy",
            "sweet_medium",
            "vanilla",
            "amber"
        });

        rec.Reasons.Add("Date occasion → warm, intimate, sexy, vanilla and amber notes preferred");
    }
}
