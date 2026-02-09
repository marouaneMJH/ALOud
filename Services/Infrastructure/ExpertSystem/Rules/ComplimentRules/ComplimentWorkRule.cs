using NRules.Fluent.Dsl;

namespace ALOud.Services.Infrastructure.ExpertSystem.Rules.ComplimentRules;

public class ComplimentWorkRule : Rule
{
    public override void Define()
    {
        UserProfile user = null!;
        Recommendation rec = null!;

        When()
            .Match(() => user, u => u.Compliment == EComplimentDesire.Yes && u.Occasion == EOccasion.Office)
            .Match(() => rec);

        Then()
            .Do(_ => Apply(rec));
    }

    private static void Apply(Recommendation rec)
    {
        rec.Prefer.Add("clean");
        rec.Sillage = "moderate";

        rec.Reasons.Add("Compliment desire + Work → clean fragrance with moderate sillage");
    }
}
