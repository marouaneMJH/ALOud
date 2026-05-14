using NRules.Fluent.Dsl;
using ALOud.Services.Infrastructure.ExpertSystem.Domain;

namespace ALOud.Services.Infrastructure.ExpertSystem.Rules.ClimateRules;



public class HumidClimateRule : Rule
{
    private static readonly string[] HumidClimatePreferences = { "aromatic", "woody", "iso_e_super" };

    public override void Define()
    {
        UserProfile user = null!;
        Recommendation rec = null!;

        When()
            .Match(() => user, u => u.Climate == EClimate.Humid)
            .Match(() => rec);

        Then()
            .Do(_ => Apply(rec));
    }

    private static void Apply(Recommendation rec)
    {
        rec.Prefer.UnionWith(HumidClimatePreferences);

        rec.Reasons.Add("Humid climate → aromatic and woody perform better");
    }
}
