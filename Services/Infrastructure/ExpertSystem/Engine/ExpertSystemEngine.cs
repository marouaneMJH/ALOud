using NRules;
using NRules.Fluent;

public class ExpertSystemEngine
{
    private readonly ISessionFactory _factory;

    public ExpertSystemEngine()
    {
        var repository = new RuleRepository();
        repository.Load(x => x.From(typeof(ExpertSystemEngine).Assembly));

        _factory = repository.Compile();
    }

    public Recommendation Run(UserProfile profile)
    {
        var session = _factory.CreateSession();
        var rec = new Recommendation();

        session.Insert(profile);
        session.Insert(rec);
        session.Fire();

        return rec;
    }
}
