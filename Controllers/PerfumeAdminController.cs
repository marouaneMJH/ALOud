using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ALOud.Services.Brand;
using ALOud.Services.Perfume;
using ALOud.Services.Family;
using ALOud.Services.Note;
using ALOud.Services.Accord;
using ALOud.Services.Tag;
using ALOud.Services.Season;
using ALOud.Services.Occasion;
using ALOud.DTOs.Brands;
using ALOud.DTOs.Perfumes;
using ALOud.DTOs.Families;
using ALOud.DTOs.Notes;
using ALOud.DTOs.Accords;
using ALOud.DTOs.Tags;
using ALOud.DTOs.Seasons;
using ALOud.DTOs.Occasions;

namespace ALOud.Controllers
{
    [Authorize]
    [Route("Admin/[action]")]
    public class PerfumeAdminController : Controller
    {
        private readonly IBrandService _brandService;
        private readonly IPerfumeService _perfumeService;
        private readonly IFamilyService _familyService;
        private readonly INoteService _noteService;
        private readonly IAccordService _accordService;
        private readonly ITagService _tagService;
        private readonly ISeasonService _seasonService;
        private readonly IOccasionService _occasionService;
        private readonly ILogger<PerfumeAdminController> _logger;

        public PerfumeAdminController(
            IBrandService brandService,
            IPerfumeService perfumeService,
            IFamilyService familyService,
            INoteService noteService,
            IAccordService accordService,
            ITagService tagService,
            ISeasonService seasonService,
            IOccasionService occasionService,
            ILogger<PerfumeAdminController> logger)
        {
            _brandService = brandService;
            _perfumeService = perfumeService;
            _familyService = familyService;
            _noteService = noteService;
            _accordService = accordService;
            _tagService = tagService;
            _seasonService = seasonService;
            _occasionService = occasionService;
            _logger = logger;
        }

        // =====================================================
        // BRANDS MANAGEMENT
        // =====================================================

        public async Task<IActionResult> Brands(int pageIndex = 1, int pageSize = 10, string? searchTerm = null)
        {
            var brands = await _brandService.GetAllBrandsAsync(pageIndex, pageSize, searchTerm);
            ViewBag.SearchTerm = searchTerm;
            return View("~/Views/Admin/Brands/Index.cshtml", brands);
        }

        public IActionResult CreateBrand()
        {
            return View("~/Views/Admin/Brands/Create.cshtml", new CreateBrandDto());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateBrand(CreateBrandDto dto)
        {
            if (!ModelState.IsValid)
                return View("~/Views/Admin/Brands/Create.cshtml", dto);

            if (await _brandService.BrandExistsAsync(dto.Name))
            {
                ModelState.AddModelError("Name", "A brand with this name already exists");
                return View("~/Views/Admin/Brands/Create.cshtml", dto);
            }

            await _brandService.CreateBrandAsync(dto);
            TempData["Success"] = "Brand created successfully";
            return RedirectToAction(nameof(Brands));
        }

        public async Task<IActionResult> EditBrand(Guid id)
        {
            var brand = await _brandService.GetBrandForEditAsync(id);
            if (brand == null) return NotFound();
            return View("~/Views/Admin/Brands/Edit.cshtml", brand);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditBrand(Guid id, UpdateBrandDto dto)
        {
            if (id != dto.Id) return BadRequest();

            if (!ModelState.IsValid)
                return View("~/Views/Admin/Brands/Edit.cshtml", dto);

            if (await _brandService.BrandExistsAsync(dto.Name, dto.Id))
            {
                ModelState.AddModelError("Name", "A brand with this name already exists");
                return View("~/Views/Admin/Brands/Edit.cshtml", dto);
            }

            var success = await _brandService.UpdateBrandAsync(dto);
            if (!success) return NotFound();

            TempData["Success"] = "Brand updated successfully";
            return RedirectToAction(nameof(Brands));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteBrand(Guid id)
        {
            var success = await _brandService.DeleteBrandAsync(id);
            if (!success)
            {
                TempData["Error"] = "Failed to delete brand";
                return RedirectToAction(nameof(Brands));
            }

            TempData["Success"] = "Brand deleted successfully";
            return RedirectToAction(nameof(Brands));
        }

        // =====================================================
        // PERFUMES MANAGEMENT
        // =====================================================

        public async Task<IActionResult> Perfumes(
            int pageIndex = 1,
            int pageSize = 10,
            string? searchTerm = null,
            Guid? brandId = null,
            Guid? familyId = null,
            string? genderProfile = null)
        {
            var perfumes = await _perfumeService.GetAllPerfumesAsync(pageIndex, pageSize, searchTerm, brandId, familyId, genderProfile);

            ViewBag.SearchTerm = searchTerm;
            ViewBag.BrandId = brandId;
            ViewBag.FamilyId = familyId;
            ViewBag.GenderProfile = genderProfile;
            ViewBag.Brands = await _brandService.GetAllBrandsForSelectAsync();
            ViewBag.Families = await _familyService.GetAllFamiliesForSelectAsync();

            return View("~/Views/Admin/Perfumes/Index.cshtml", perfumes);
        }

        public async Task<IActionResult> CreatePerfume()
        {
            await PopulatePerfumeViewBag();
            return View("~/Views/Admin/Perfumes/Create.cshtml", new CreatePerfumeDto());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePerfume(CreatePerfumeDto dto)
        {
            if (!ModelState.IsValid)
            {
                await PopulatePerfumeViewBag();
                return View("~/Views/Admin/Perfumes/Create.cshtml", dto);
            }

            await _perfumeService.CreatePerfumeAsync(dto);
            TempData["Success"] = "Perfume created successfully";
            return RedirectToAction(nameof(Perfumes));
        }

        public async Task<IActionResult> EditPerfume(Guid id)
        {
            var perfume = await _perfumeService.GetPerfumeForEditAsync(id);
            if (perfume == null) return NotFound();

            await PopulatePerfumeViewBag();
            return View("~/Views/Admin/Perfumes/Edit.cshtml", perfume);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPerfume(Guid id, UpdatePerfumeDto dto)
        {
            if (id != dto.Id) return BadRequest();

            if (!ModelState.IsValid)
            {
                await PopulatePerfumeViewBag();
                return View("~/Views/Admin/Perfumes/Edit.cshtml", dto);
            }

            var success = await _perfumeService.UpdatePerfumeAsync(dto);
            if (!success) return NotFound();

            TempData["Success"] = "Perfume updated successfully";
            return RedirectToAction(nameof(Perfumes));
        }

        public async Task<IActionResult> PerfumeDetails(Guid id)
        {
            var perfume = await _perfumeService.GetPerfumeDetailsAsync(id);
            if (perfume == null) return NotFound();
            return View("~/Views/Admin/Perfumes/Details.cshtml", perfume);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePerfume(Guid id)
        {
            var success = await _perfumeService.DeletePerfumeAsync(id);
            if (!success)
            {
                TempData["Error"] = "Failed to delete perfume";
                return RedirectToAction(nameof(Perfumes));
            }

            TempData["Success"] = "Perfume deleted successfully";
            return RedirectToAction(nameof(Perfumes));
        }

        private async Task PopulatePerfumeViewBag()
        {
            ViewBag.Brands = await _brandService.GetAllBrandsForSelectAsync();
            ViewBag.Families = await _familyService.GetAllFamiliesForSelectAsync();
            ViewBag.Notes = await _noteService.GetAllNotesForSelectAsync();
            ViewBag.Accords = await _accordService.GetAllAccordsForSelectAsync();
            ViewBag.Tags = await _tagService.GetAllTagsForSelectAsync();
            ViewBag.Seasons = await _seasonService.GetAllSeasonsForSelectAsync();
            ViewBag.Occasions = await _occasionService.GetAllOccasionsForSelectAsync();
        }

        // =====================================================
        // FAMILIES MANAGEMENT
        // =====================================================

        public async Task<IActionResult> Families(int pageIndex = 1, int pageSize = 10, string? searchTerm = null)
        {
            var families = await _familyService.GetAllFamiliesAsync(pageIndex, pageSize, searchTerm);
            ViewBag.SearchTerm = searchTerm;
            return View("~/Views/Admin/Families/Index.cshtml", families);
        }

        public IActionResult CreateFamily()
        {
            return View("~/Views/Admin/Families/Create.cshtml", new CreateFamilyDto());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateFamily(CreateFamilyDto dto)
        {
            if (!ModelState.IsValid)
                return View("~/Views/Admin/Families/Create.cshtml", dto);

            if (await _familyService.FamilyExistsAsync(dto.Name))
            {
                ModelState.AddModelError("Name", "A family with this name already exists");
                return View("~/Views/Admin/Families/Create.cshtml", dto);
            }

            await _familyService.CreateFamilyAsync(dto);
            TempData["Success"] = "Family created successfully";
            return RedirectToAction(nameof(Families));
        }

        public async Task<IActionResult> EditFamily(Guid id)
        {
            var family = await _familyService.GetFamilyForEditAsync(id);
            if (family == null) return NotFound();
            return View("~/Views/Admin/Families/Edit.cshtml", family);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditFamily(Guid id, UpdateFamilyDto dto)
        {
            if (id != dto.Id) return BadRequest();

            if (!ModelState.IsValid)
                return View("~/Views/Admin/Families/Edit.cshtml", dto);

            if (await _familyService.FamilyExistsAsync(dto.Name, dto.Id))
            {
                ModelState.AddModelError("Name", "A family with this name already exists");
                return View("~/Views/Admin/Families/Edit.cshtml", dto);
            }

            var success = await _familyService.UpdateFamilyAsync(dto);
            if (!success) return NotFound();

            TempData["Success"] = "Family updated successfully";
            return RedirectToAction(nameof(Families));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteFamily(Guid id)
        {
            var success = await _familyService.DeleteFamilyAsync(id);
            if (!success)
            {
                TempData["Error"] = "Failed to delete family";
                return RedirectToAction(nameof(Families));
            }

            TempData["Success"] = "Family deleted successfully";
            return RedirectToAction(nameof(Families));
        }

        // =====================================================
        // NOTES MANAGEMENT
        // =====================================================

        public async Task<IActionResult> Notes(int pageIndex = 1, int pageSize = 10, string? searchTerm = null, string? category = null)
        {
            var notes = await _noteService.GetAllNotesAsync(pageIndex, pageSize, searchTerm, category);
            ViewBag.SearchTerm = searchTerm;
            ViewBag.Category = category;
            ViewBag.Categories = await _noteService.GetNoteCategoriesAsync();
            return View("~/Views/Admin/Notes/Index.cshtml", notes);
        }

        public async Task<IActionResult> CreateNote()
        {
            ViewBag.Categories = await _noteService.GetNoteCategoriesAsync();
            return View("~/Views/Admin/Notes/Create.cshtml", new CreateNoteDto());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateNote(CreateNoteDto dto)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Categories = await _noteService.GetNoteCategoriesAsync();
                return View("~/Views/Admin/Notes/Create.cshtml", dto);
            }

            if (await _noteService.NoteExistsAsync(dto.Name))
            {
                ModelState.AddModelError("Name", "A note with this name already exists");
                ViewBag.Categories = await _noteService.GetNoteCategoriesAsync();
                return View("~/Views/Admin/Notes/Create.cshtml", dto);
            }

            await _noteService.CreateNoteAsync(dto);
            TempData["Success"] = "Note created successfully";
            return RedirectToAction(nameof(Notes));
        }

        public async Task<IActionResult> EditNote(Guid id)
        {
            var note = await _noteService.GetNoteForEditAsync(id);
            if (note == null) return NotFound();
            ViewBag.Categories = await _noteService.GetNoteCategoriesAsync();
            return View("~/Views/Admin/Notes/Edit.cshtml", note);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditNote(Guid id, UpdateNoteDto dto)
        {
            if (id != dto.Id) return BadRequest();

            if (!ModelState.IsValid)
            {
                ViewBag.Categories = await _noteService.GetNoteCategoriesAsync();
                return View("~/Views/Admin/Notes/Edit.cshtml", dto);
            }

            if (await _noteService.NoteExistsAsync(dto.Name, dto.Id))
            {
                ModelState.AddModelError("Name", "A note with this name already exists");
                ViewBag.Categories = await _noteService.GetNoteCategoriesAsync();
                return View("~/Views/Admin/Notes/Edit.cshtml", dto);
            }

            var success = await _noteService.UpdateNoteAsync(dto);
            if (!success) return NotFound();

            TempData["Success"] = "Note updated successfully";
            return RedirectToAction(nameof(Notes));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteNote(Guid id)
        {
            var success = await _noteService.DeleteNoteAsync(id);
            if (!success)
            {
                TempData["Error"] = "Failed to delete note";
                return RedirectToAction(nameof(Notes));
            }

            TempData["Success"] = "Note deleted successfully";
            return RedirectToAction(nameof(Notes));
        }

        // =====================================================
        // ACCORDS MANAGEMENT
        // =====================================================

        public async Task<IActionResult> Accords(int pageIndex = 1, int pageSize = 10, string? searchTerm = null)
        {
            var accords = await _accordService.GetAllAccordsAsync(pageIndex, pageSize, searchTerm);
            ViewBag.SearchTerm = searchTerm;
            return View("~/Views/Admin/Accords/Index.cshtml", accords);
        }

        public IActionResult CreateAccord()
        {
            return View("~/Views/Admin/Accords/Create.cshtml", new CreateAccordDto());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateAccord(CreateAccordDto dto)
        {
            if (!ModelState.IsValid)
                return View("~/Views/Admin/Accords/Create.cshtml", dto);

            if (await _accordService.AccordExistsAsync(dto.Name))
            {
                ModelState.AddModelError("Name", "An accord with this name already exists");
                return View("~/Views/Admin/Accords/Create.cshtml", dto);
            }

            await _accordService.CreateAccordAsync(dto);
            TempData["Success"] = "Accord created successfully";
            return RedirectToAction(nameof(Accords));
        }

        public async Task<IActionResult> EditAccord(Guid id)
        {
            var accord = await _accordService.GetAccordForEditAsync(id);
            if (accord == null) return NotFound();
            return View("~/Views/Admin/Accords/Edit.cshtml", accord);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditAccord(Guid id, UpdateAccordDto dto)
        {
            if (id != dto.Id) return BadRequest();

            if (!ModelState.IsValid)
                return View("~/Views/Admin/Accords/Edit.cshtml", dto);

            if (await _accordService.AccordExistsAsync(dto.Name, dto.Id))
            {
                ModelState.AddModelError("Name", "An accord with this name already exists");
                return View("~/Views/Admin/Accords/Edit.cshtml", dto);
            }

            var success = await _accordService.UpdateAccordAsync(dto);
            if (!success) return NotFound();

            TempData["Success"] = "Accord updated successfully";
            return RedirectToAction(nameof(Accords));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAccord(Guid id)
        {
            var success = await _accordService.DeleteAccordAsync(id);
            if (!success)
            {
                TempData["Error"] = "Failed to delete accord";
                return RedirectToAction(nameof(Accords));
            }

            TempData["Success"] = "Accord deleted successfully";
            return RedirectToAction(nameof(Accords));
        }

        // =====================================================
        // TAGS MANAGEMENT
        // =====================================================

        public async Task<IActionResult> Tags(int pageIndex = 1, int pageSize = 10, string? searchTerm = null)
        {
            var tags = await _tagService.GetAllTagsAsync(pageIndex, pageSize, searchTerm);
            ViewBag.SearchTerm = searchTerm;
            return View("~/Views/Admin/Tags/Index.cshtml", tags);
        }

        public IActionResult CreateTag()
        {
            return View("~/Views/Admin/Tags/Create.cshtml", new CreateTagDto());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateTag(CreateTagDto dto)
        {
            if (!ModelState.IsValid)
                return View("~/Views/Admin/Tags/Create.cshtml", dto);

            if (await _tagService.TagExistsAsync(dto.Name))
            {
                ModelState.AddModelError("Name", "A tag with this name already exists");
                return View("~/Views/Admin/Tags/Create.cshtml", dto);
            }

            await _tagService.CreateTagAsync(dto);
            TempData["Success"] = "Tag created successfully";
            return RedirectToAction(nameof(Tags));
        }

        public async Task<IActionResult> EditTag(Guid id)
        {
            var tag = await _tagService.GetTagForEditAsync(id);
            if (tag == null) return NotFound();
            return View("~/Views/Admin/Tags/Edit.cshtml", tag);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditTag(Guid id, UpdateTagDto dto)
        {
            if (id != dto.Id) return BadRequest();

            if (!ModelState.IsValid)
                return View("~/Views/Admin/Tags/Edit.cshtml", dto);

            if (await _tagService.TagExistsAsync(dto.Name, dto.Id))
            {
                ModelState.AddModelError("Name", "A tag with this name already exists");
                return View("~/Views/Admin/Tags/Edit.cshtml", dto);
            }

            var success = await _tagService.UpdateTagAsync(dto);
            if (!success) return NotFound();

            TempData["Success"] = "Tag updated successfully";
            return RedirectToAction(nameof(Tags));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteTag(Guid id)
        {
            var success = await _tagService.DeleteTagAsync(id);
            if (!success)
            {
                TempData["Error"] = "Failed to delete tag";
                return RedirectToAction(nameof(Tags));
            }

            TempData["Success"] = "Tag deleted successfully";
            return RedirectToAction(nameof(Tags));
        }

        // =====================================================
        // SEASONS MANAGEMENT
        // =====================================================

        public async Task<IActionResult> Seasons(int pageIndex = 1, int pageSize = 10, string? searchTerm = null)
        {
            var seasons = await _seasonService.GetAllSeasonsAsync(pageIndex, pageSize, searchTerm);
            ViewBag.SearchTerm = searchTerm;
            return View("~/Views/Admin/Seasons/Index.cshtml", seasons);
        }

        public IActionResult CreateSeason()
        {
            return View("~/Views/Admin/Seasons/Create.cshtml", new CreateSeasonDto());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateSeason(CreateSeasonDto dto)
        {
            if (!ModelState.IsValid)
                return View("~/Views/Admin/Seasons/Create.cshtml", dto);

            if (await _seasonService.SeasonExistsAsync(dto.Name))
            {
                ModelState.AddModelError("Name", "A season with this name already exists");
                return View("~/Views/Admin/Seasons/Create.cshtml", dto);
            }

            await _seasonService.CreateSeasonAsync(dto);
            TempData["Success"] = "Season created successfully";
            return RedirectToAction(nameof(Seasons));
        }

        public async Task<IActionResult> EditSeason(Guid id)
        {
            var season = await _seasonService.GetSeasonForEditAsync(id);
            if (season == null) return NotFound();
            return View("~/Views/Admin/Seasons/Edit.cshtml", season);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditSeason(Guid id, UpdateSeasonDto dto)
        {
            if (id != dto.Id) return BadRequest();

            if (!ModelState.IsValid)
                return View("~/Views/Admin/Seasons/Edit.cshtml", dto);

            if (await _seasonService.SeasonExistsAsync(dto.Name, dto.Id))
            {
                ModelState.AddModelError("Name", "A season with this name already exists");
                return View("~/Views/Admin/Seasons/Edit.cshtml", dto);
            }

            var success = await _seasonService.UpdateSeasonAsync(dto);
            if (!success) return NotFound();

            TempData["Success"] = "Season updated successfully";
            return RedirectToAction(nameof(Seasons));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteSeason(Guid id)
        {
            var success = await _seasonService.DeleteSeasonAsync(id);
            if (!success)
            {
                TempData["Error"] = "Failed to delete season";
                return RedirectToAction(nameof(Seasons));
            }

            TempData["Success"] = "Season deleted successfully";
            return RedirectToAction(nameof(Seasons));
        }

        // =====================================================
        // OCCASIONS MANAGEMENT
        // =====================================================

        public async Task<IActionResult> Occasions(int pageIndex = 1, int pageSize = 10, string? searchTerm = null)
        {
            var occasions = await _occasionService.GetAllOccasionsAsync(pageIndex, pageSize, searchTerm);
            ViewBag.SearchTerm = searchTerm;
            return View("~/Views/Admin/Occasions/Index.cshtml", occasions);
        }

        public IActionResult CreateOccasion()
        {
            return View("~/Views/Admin/Occasions/Create.cshtml", new CreateOccasionDto());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateOccasion(CreateOccasionDto dto)
        {
            if (!ModelState.IsValid)
                return View("~/Views/Admin/Occasions/Create.cshtml", dto);

            if (await _occasionService.OccasionExistsAsync(dto.Name))
            {
                ModelState.AddModelError("Name", "An occasion with this name already exists");
                return View("~/Views/Admin/Occasions/Create.cshtml", dto);
            }

            await _occasionService.CreateOccasionAsync(dto);
            TempData["Success"] = "Occasion created successfully";
            return RedirectToAction(nameof(Occasions));
        }

        public async Task<IActionResult> EditOccasion(Guid id)
        {
            var occasion = await _occasionService.GetOccasionForEditAsync(id);
            if (occasion == null) return NotFound();
            return View("~/Views/Admin/Occasions/Edit.cshtml", occasion);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditOccasion(Guid id, UpdateOccasionDto dto)
        {
            if (id != dto.Id) return BadRequest();

            if (!ModelState.IsValid)
                return View("~/Views/Admin/Occasions/Edit.cshtml", dto);

            if (await _occasionService.OccasionExistsAsync(dto.Name, dto.Id))
            {
                ModelState.AddModelError("Name", "An occasion with this name already exists");
                return View("~/Views/Admin/Occasions/Edit.cshtml", dto);
            }

            var success = await _occasionService.UpdateOccasionAsync(dto);
            if (!success) return NotFound();

            TempData["Success"] = "Occasion updated successfully";
            return RedirectToAction(nameof(Occasions));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteOccasion(Guid id)
        {
            var success = await _occasionService.DeleteOccasionAsync(id);
            if (!success)
            {
                TempData["Error"] = "Failed to delete occasion";
                return RedirectToAction(nameof(Occasions));
            }

            TempData["Success"] = "Occasion deleted successfully";
            return RedirectToAction(nameof(Occasions));
        }
    }
}
