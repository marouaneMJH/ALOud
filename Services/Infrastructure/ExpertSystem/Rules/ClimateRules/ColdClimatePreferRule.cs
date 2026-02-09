using NRules.Fluent.Dsl;

namespace ALOud.Services.Infrastructure.ExpertSystem.Rules.ClimateRules;

public class ColdClimatePreferRule : Rule
{
    public override void Define()
    {
        UserProfile user = null!;
        Recommendation rec = null!;

        When()
            .Match(() => user, u => u.Climate == EClimate.Cold)
            .Match(() => rec);

        Then()
            .Do(_ => Apply(rec));
    }

    private static void Apply(Recommendation rec)
    {
        rec.Prefer.UnionWith(new[]
        {
            "oriental",
            "amber",
            "gourmand",
            "smoky",
            "woody"
        });

        rec.Reasons.Add("Cold climate → oriental, amber, gourmand, smoky and woody notes preferred");
    }
}
