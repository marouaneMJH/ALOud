using NRules.Fluent.Dsl;

namespace ALOud.Services.Infrastructure.ExpertSystem.Rules.ClimateRules;

public class HumidClimatePreferRule : Rule
{
    public override void Define()
    {
        UserProfile user = null!;
        Recommendation rec = null!;

        When()
            .Match(() => user, u => u.Climate == EClimate.Humid)
            .Match(() => rec);

        Then()
            .Do(_ => Apply(rec));
    }

    private static void Apply(Recommendation rec)
    {
        rec.Prefer.UnionWith(new[]
        {
            "aromatic",
            "woody",
            "iso_e_super"
        });

        rec.Reasons.Add("Humid climate → aromatic, woody and iso e super notes perform better");
    }
}
