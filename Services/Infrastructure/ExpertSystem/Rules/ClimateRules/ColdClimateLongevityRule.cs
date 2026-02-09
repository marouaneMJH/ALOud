using NRules.Fluent.Dsl;

namespace ALOud.Services.Infrastructure.ExpertSystem.Rules.ClimateRules;

public class ColdClimateLongevityRule : Rule
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
        rec.Longevity = ">= long";
        rec.Reasons.Add("Cold climate → longevity should be long");
    }
}
