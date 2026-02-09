using NRules.Fluent.Dsl;

namespace ALOud.Services.Infrastructure.ExpertSystem.Rules.ComplimentRules;

public class ComplimentNeutralRule : Rule
{
    public override void Define()
    {
        UserProfile user = null!;
        Recommendation rec = null!;

        When()
            .Match(() => user, u => u.Compliment == EComplimentDesire.Neutral)
            .Match(() => rec);

        Then()
            .Do(_ => Apply(rec));
    }

    private static void Apply(Recommendation rec)
    {
        rec.Prefer.Add("full_range_allowed");
        rec.Reasons.Add("Neutral compliment desire → full range of fragrances allowed");
    }
}
