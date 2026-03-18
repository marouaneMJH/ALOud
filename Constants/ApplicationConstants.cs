namespace ALOud.Constants
{
    /// <summary>
    /// Route constants for consistent routing across controllers
    /// </summary>
    public static class RouteConstants
    {
        public const string Admin = "Admin";
        public const string Expert = "Expert";
        public const string HybridRecommendation = "HybridRecommendation";
        public const string ExpertSystem = "ExpertSystem";
    }

    /// <summary>
    /// Filter keys used in search operations
    /// </summary>
    public static class FilterKeys
    {
        public const string Content = "content";
    }

    /// <summary>
    /// TempData keys for message passing
    /// </summary>
    public static class TempDataKeys
    {
        public const string Success = "Success";
        public const string Error = "Error";
    }
}