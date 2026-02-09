using NRules.Fluent.Dsl;

namespace ALOud.Services.Infrastructure.ExpertSystem.Rules.PerformanceRules;

public class MuskAmberVanillaBaseRule : Rule
{
    public override void Define()
    {
        UserProfile user = null!;
        Recommendation rec = null!;

        When()
            .Match(() => user)
            .Match(() => rec);

        Then()
            .Do(_ => Apply(rec));
    }

    private static void Apply(Recommendation rec)
    {
        rec.Prefer.Add("musk_amber_vanilla_base_boost");
        rec.Reasons.Add("Base notes of musk, amber and vanilla → longevity index boosted");
    }
}
