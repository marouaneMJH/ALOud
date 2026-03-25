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

    /// <summary>
    /// Success message templates
    /// </summary>
    public static class SuccessMessages
    {
        public const string BrandCreated = "Brand created successfully";
        public const string BrandUpdated = "Brand updated successfully";
        public const string BrandDeleted = "Brand deleted successfully";
        
        public const string FamilyCreated = "Family created successfully";
        public const string FamilyUpdated = "Family updated successfully";
        public const string FamilyDeleted = "Family deleted successfully";
        
        public const string NoteCreated = "Note created successfully";
        public const string NoteUpdated = "Note updated successfully";
        public const string NoteDeleted = "Note deleted successfully";
        
        public const string AccordCreated = "Accord created successfully";
        public const string AccordUpdated = "Accord updated successfully";
        public const string AccordDeleted = "Accord deleted successfully";
        
        public const string TagCreated = "Tag created successfully";
        public const string TagUpdated = "Tag updated successfully";
        public const string TagDeleted = "Tag deleted successfully";
        
        public const string SeasonCreated = "Season created successfully";
        public const string SeasonUpdated = "Season updated successfully";
        public const string SeasonDeleted = "Season deleted successfully";
        
        public const string OccasionCreated = "Occasion created successfully";
        public const string OccasionUpdated = "Occasion updated successfully";
        public const string OccasionDeleted = "Occasion deleted successfully";
        
        public const string PerfumeCreated = "Perfume created successfully";
        public const string PerfumeUpdated = "Perfume updated successfully";
        public const string PerfumeDeleted = "Perfume deleted successfully";
    }

    /// <summary>
    /// Error message templates
    /// </summary>
    public static class ErrorMessages
    {
        public const string BrandCreateFailed = "Failed to create brand";
        public const string BrandUpdateFailed = "Failed to update brand";
        public const string BrandDeleteFailed = "Failed to delete brand";
        public const string BrandNotFound = "Brand not found";
        public const string BrandExists = "A brand with this name already exists";
        
        public const string FamilyCreateFailed = "Failed to create family";
        public const string FamilyUpdateFailed = "Failed to update family";
        public const string FamilyDeleteFailed = "Failed to delete family";
        public const string FamilyNotFound = "Family not found";
        public const string FamilyExists = "A family with this name already exists";
        
        public const string NoteCreateFailed = "Failed to create note";
        public const string NoteUpdateFailed = "Failed to update note";
        public const string NoteDeleteFailed = "Failed to delete note";
        public const string NoteNotFound = "Note not found";
        public const string NoteExists = "A note with this name already exists";
        
        public const string AccordCreateFailed = "Failed to create accord";
        public const string AccordUpdateFailed = "Failed to update accord";
        public const string AccordDeleteFailed = "Failed to delete accord";
        public const string AccordNotFound = "Accord not found";
        public const string AccordExists = "An accord with this name already exists";
        
        public const string TagCreateFailed = "Failed to create tag";
        public const string TagUpdateFailed = "Failed to update tag";
        public const string TagDeleteFailed = "Failed to delete tag";
        public const string TagNotFound = "Tag not found";
        public const string TagExists = "A tag with this name already exists";
        
        public const string SeasonCreateFailed = "Failed to create season";
        public const string SeasonUpdateFailed = "Failed to update season";
        public const string SeasonDeleteFailed = "Failed to delete season";
        public const string SeasonNotFound = "Season not found";
        public const string SeasonExists = "A season with this name already exists";
        
        public const string OccasionCreateFailed = "Failed to create occasion";
        public const string OccasionUpdateFailed = "Failed to update occasion";
        public const string OccasionDeleteFailed = "Failed to delete occasion";
        public const string OccasionNotFound = "Occasion not found";
        public const string OccasionExists = "An occasion with this name already exists";
        
        public const string PerfumeCreateFailed = "Failed to create perfume";
        public const string PerfumeUpdateFailed = "Failed to update perfume";
        public const string PerfumeDeleteFailed = "Failed to delete perfume";
        public const string PerfumeNotFound = "Perfume not found";
        
        public const string InvalidRequest = "Invalid request";
        public const string InvalidId = "Invalid ID";
        public const string ValidationFailed = "Validation failed";
        public const string DatabaseError = "Database error occurred";
        public const string UnauthorizedAccess = "Unauthorized access";
        public const string NotFound = "Resource not found";
    }

    /// <summary>
    /// View paths for consistent view resolution
    /// </summary>
    public static class ViewPaths
    {
        public const string AdminBrandsCreate = "~/Views/Admin/Brands/Create.cshtml";
        public const string AdminBrandsEdit = "~/Views/Admin/Brands/Edit.cshtml";
        public const string AdminBrandsIndex = "~/Views/Admin/Brands/Index.cshtml";
        
        public const string AdminFamiliesCreate = "~/Views/Admin/Families/Create.cshtml";
        public const string AdminFamiliesEdit = "~/Views/Admin/Families/Edit.cshtml";
        public const string AdminFamiliesIndex = "~/Views/Admin/Families/Index.cshtml";
        
        public const string AdminNotesCreate = "~/Views/Admin/Notes/Create.cshtml";
        public const string AdminNotesEdit = "~/Views/Admin/Notes/Edit.cshtml";
        public const string AdminNotesIndex = "~/Views/Admin/Notes/Index.cshtml";
        
        public const string AdminAccordsCreate = "~/Views/Admin/Accords/Create.cshtml";
        public const string AdminAccordsEdit = "~/Views/Admin/Accords/Edit.cshtml";
        public const string AdminAccordsIndex = "~/Views/Admin/Accords/Index.cshtml";
        
        public const string AdminTagsCreate = "~/Views/Admin/Tags/Create.cshtml";
        public const string AdminTagsEdit = "~/Views/Admin/Tags/Edit.cshtml";
        public const string AdminTagsIndex = "~/Views/Admin/Tags/Index.cshtml";
        
        public const string AdminSeasonsCreate = "~/Views/Admin/Seasons/Create.cshtml";
        public const string AdminSeasonsEdit = "~/Views/Admin/Seasons/Edit.cshtml";
        public const string AdminSeasonsIndex = "~/Views/Admin/Seasons/Index.cshtml";
        
        public const string AdminOccasionsCreate = "~/Views/Admin/Occasions/Create.cshtml";
        public const string AdminOccasionsEdit = "~/Views/Admin/Occasions/Edit.cshtml";
        public const string AdminOccasionsIndex = "~/Views/Admin/Occasions/Index.cshtml";
        
        public const string AdminPerfumesCreate = "~/Views/Admin/Perfumes/Create.cshtml";
        public const string AdminPerfumesEdit = "~/Views/Admin/Perfumes/Edit.cshtml";
        public const string AdminPerfumesIndex = "~/Views/Admin/Perfumes/Index.cshtml";
        public const string AdminPerfumesDetails = "~/Views/Admin/Perfumes/Details.cshtml";
    }

    /// <summary>
    /// Route names for consistent redirections
    /// </summary>
    public static class RouteNames
    {
        public const string MvcPerfumeAdminBrands = "MvcPerfumeAdminBrands";
        public const string MvcPerfumeAdminFamilies = "MvcPerfumeAdminFamilies";
        public const string MvcPerfumeAdminNotes = "MvcPerfumeAdminNotes";
        public const string MvcPerfumeAdminAccords = "MvcPerfumeAdminAccords";
        public const string MvcPerfumeAdminTags = "MvcPerfumeAdminTags";
        public const string MvcPerfumeAdminSeasons = "MvcPerfumeAdminSeasons";
        public const string MvcPerfumeAdminOccasions = "MvcPerfumeAdminOccasions";
        public const string MvcPerfumeAdminPerfumes = "MvcPerfumeAdminPerfumes";
        public const string MvcCartIndex = "MvcCartIndex";
    }
}