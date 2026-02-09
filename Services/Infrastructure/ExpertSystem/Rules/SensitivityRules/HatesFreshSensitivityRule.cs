using NRules.Fluent.Dsl;

namespace ALOud.Services.Infrastructure.ExpertSystem.Rules.SensitivityRules;

public class HatesFreshSensitivityRule : Rule
{
    public override void Define()
    {
        UserProfile user = null!;
        Recommendation rec = null!;

        When()
            .Match(() => user, u => u.Sensitivity == ESensitivity.HatesFresh)
            .Match(() => rec);

        Then()
            .Do(_ => Apply(rec));
    }

    private static void Apply(Recommendation rec)
    {
        rec.Avoid.UnionWith(new[]
        {
            "citrus",
            "aquatic"
        });

        rec.Reasons.Add("Hates fresh → avoid citrus and aquatic notes");
    }
}
