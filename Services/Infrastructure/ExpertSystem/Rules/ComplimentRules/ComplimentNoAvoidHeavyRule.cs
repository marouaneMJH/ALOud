using NRules.Fluent.Dsl;

namespace ALOud.Services.Infrastructure.ExpertSystem.Rules.ComplimentRules;

public class ComplimentNoAvoidHeavyRule : Rule
{
    public override void Define()
    {
        UserProfile user = null!;
        Recommendation rec = null!;

        When()
            .Match(() => user, u => u.Compliment == EComplimentDesire.No)
            .Match(() => rec);

        Then()
            .Do(_ => Apply(rec));
    }

    private static void Apply(Recommendation rec)
    {
        rec.Avoid.Add("heavy_sillage");
        rec.Reasons.Add("No compliment desire → avoid heavy sillage");
    }
}
