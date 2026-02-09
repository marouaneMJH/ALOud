using NRules.Fluent.Dsl;

namespace ALOud.Services.Infrastructure.ExpertSystem.Rules.ClimateRules;

public class HotClimateAvoidRule : Rule
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
        rec.Avoid.UnionWith(new[]
        {
            "oriental_heavy",
            "gourmand_heavy",
            "sweet_high"
        });

        rec.Reasons.Add("Hot climate → avoid heavy oriental, gourmand and sweet fragrances");
    }
}
