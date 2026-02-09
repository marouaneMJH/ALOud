using NRules.Fluent.Dsl;
using ALOud.Services.Infrastructure.ExpertSystem.Domain;

using ALOud.Services.Infrastructure.ExpertSystem.Domain;

namespace ALOud.Services.Infrastructure.ExpertSystem.Rules.ComplimentRules;

public class ComplimentYesAvoidIntimateRule : Rule
{
    public override void Define()
    {
        UserProfile user = null!;
        Recommendation rec = null!;

        When()
            .Match(() => user, u => u.Compliment == EComplimentDesire.Yes)
            .Match(() => rec);

        Then()
            .Do(_ => Apply(rec));
    }

    private static void Apply(Recommendation rec)
    {
        rec.Avoid.Add("intimate_only");
        rec.Reasons.Add("Compliment desire → avoid intimate-only sillage");
    }
}
