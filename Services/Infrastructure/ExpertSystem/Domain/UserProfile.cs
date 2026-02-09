public class UserProfile
{
    public EClimate Climate { get; set; }
    public EOccasion Occasion { get; set; }
    public ESkinType SkinType { get; set; }
    public EComplimentDesire Compliment { get; set; }

    public bool WantsLongPerformance { get; set; }

    public string Persona { get; set; }
    public string Sensitivity { get; set; }
}
