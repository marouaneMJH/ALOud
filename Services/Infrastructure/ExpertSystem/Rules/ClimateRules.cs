using NRules.Fluent.Dsl;

public class HotClimateRule : Rule
{
    public override void Define()
    {
        UserProfile user = null!;
        Recommendation rec = null!;

        When()
            .Match(() => user, u => u.Climate == EClimate.Hot)
            .Match(() => rec);

        Then()
            .Do(ctx => Apply(rec));
    }

    private static void Apply(Recommendation rec)
    {
        rec.Avoid.UnionWith(new[]
        {
            "oriental_heavy",
            "gourmand_heavy",
            "sweet_high"
        });

        rec.Prefer.UnionWith(new[]
        {
            "citrus",
            "aquatic",
            "green",
            "light_musk"
        });

        rec.Sillage = "moderate_or_intimate";
        rec.Longevity = ">= medium";

        rec.Reasons.Add("Hot climate → fresh and light perfumes preferred");
    }
}
