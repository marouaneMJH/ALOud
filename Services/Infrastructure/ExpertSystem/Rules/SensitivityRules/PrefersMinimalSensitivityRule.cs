using NRules.Fluent.Dsl;
using ALOud.Services.Infrastructure.ExpertSystem.Domain;

using ALOud.Services.Infrastructure.ExpertSystem.Domain;

namespace ALOud.Services.Infrastructure.ExpertSystem.Rules.SensitivityRules;

public class PrefersMinimalSensitivityRule : Rule
{
    public override void Define()
    {
        UserProfile user = null!;
        Recommendation rec = null!;

        When()
            .Match(() => user, u => u.Sensitivity == ESensitivity.PrefersMinimal)
            .Match(() => rec);

        Then()
            .Do(_ => Apply(rec));
    }

    private static void Apply(Recommendation rec)
    {
        rec.Prefer.UnionWith(new[]
        {
            "iso_e_super",
            "musk"
        });

        rec.Reasons.Add("Prefers minimal → iso e super and musk suggested");
    }
}
