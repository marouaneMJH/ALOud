using NRules.Fluent.Dsl;
using ALOud.Services.Infrastructure.ExpertSystem.Domain;

using ALOud.Services.Infrastructure.ExpertSystem.Domain;


namespace ALOud.Services.Infrastructure.ExpertSystem.Rules.OccasionRules;

public class GymPreferRule : Rule
{
    public override void Define()
    {
        UserProfile user = null!;
        Recommendation rec = null!;

        When()
            .Match(() => user, u => u.Occasion == EOccasion.Gym)
            .Match(() => rec);

        Then()
            .Do(_ => Apply(rec));
    }

    private static void Apply(Recommendation rec)
    {
        rec.Prefer.UnionWith(new[]
        {
            "very_light",
            "citrus"
        });

        rec.Avoid.Add("sweet");

        rec.Reasons.Add("Gym occasion → very light citrus with no sweet notes preferred");
    }
}
