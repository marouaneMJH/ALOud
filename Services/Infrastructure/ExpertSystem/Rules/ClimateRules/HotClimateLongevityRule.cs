using NRules.Fluent.Dsl;
using ALOud.Services.Infrastructure.ExpertSystem.Domain;


namespace ALOud.Services.Infrastructure.ExpertSystem.Rules.ClimateRules;

public class HotClimateLongevityRule : Rule
{
    public override void Define()
    {
        UserProfile user = null!;
        Recommendation rec = null!;

        When()
            .Match(() => user, u => u.Climate == EClimate.Hot)
            .Match(() => rec);

        Then()
            .Do(_ => Apply(rec));
    }

    private static void Apply(Recommendation rec)
    {
        rec.Longevity = ">= medium";
        rec.Reasons.Add("Hot climate → longevity should be at least medium");
    }
}
