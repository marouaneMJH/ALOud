using NRules.Fluent.Dsl;
using ALOud.Services.Infrastructure.ExpertSystem.Domain;


namespace ALOud.Services.Infrastructure.ExpertSystem.Rules.ClimateRules;

public class AllYearSeasonRule : Rule
{
    public override void Define()
    {
        UserProfile user = null!;
        Recommendation rec = null!;

        When()
            .Match(() => user, u => u.SeasonPreference == ESeasonPreference.AllYear)
            .Match(() => rec);

        Then()
            .Do(_ => Apply(rec));
    }

    private static void Apply(Recommendation rec)
    {
        rec.Prefer.UnionWith(new[]
        {
            "woody",
            "aromatic",
            "musk"
        });

        rec.Reasons.Add("All-year preference → woody, aromatic or musk notes are versatile");
    }
}
