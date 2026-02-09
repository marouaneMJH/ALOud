public class ExpertSystemService : IExpertSystemService
{
    private readonly ExpertSystemEngine _engine = new();

    public Recommendation Evaluate(UserProfile profile)
        => _engine.Run(profile);
}
