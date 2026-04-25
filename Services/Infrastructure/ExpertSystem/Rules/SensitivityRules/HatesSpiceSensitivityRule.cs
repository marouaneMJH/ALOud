using NRules.Fluent.Dsl;
using ALOud.Services.Infrastructure.ExpertSystem.Domain;


namespace ALOud.Services.Infrastructure.ExpertSystem.Rules.SensitivityRules;

public class HatesSpiceSensitivityRule : Rule
{
    public override void Define()
    {
        UserProfile user = null!;
        Recommendation rec = null!;

        When()
            .Match(() => user, u => u.Sensitivity == ESensitivity.HatesSpice)
            .Match(() => rec);

        Then()
            .Do(_ => Apply(rec));
    }

    private static void Apply(Recommendation rec)
    {
        rec.Avoid.UnionWith(new[]
        {
            "cardamom",
            "cinnamon",
            "pepper"
        });

        rec.Reasons.Add("Hates spice → avoid cardamom, cinnamon and pepper notes");
    }
}
