using NRules.Fluent.Dsl;

namespace ALOud.Services.Infrastructure.ExpertSystem.Rules.PerformanceRules;

public class HotClimateTopNotesRule : Rule
{
    public override void Define()
    {
        UserProfile user = null!;
        Recommendation rec = null!;

        When()
            .Match(() => user, u => u.Climate == EClimate.Hot)
            .Match(() => rec);

        Then()
            .Do(_ => Apply(rec));
    }

    private static void Apply(Recommendation rec)
    {
        rec.Avoid.Add("volatile_top_notes_only");
        rec.Reasons.Add("Hot climate → volatile top notes evaporate faster, avoid relying on them");
    }
}
