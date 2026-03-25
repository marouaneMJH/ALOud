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
using ALOud.Services;

namespace ALOud.Controllers.MVC
{
    /// <summary>
    /// MVC Controller for comprehensive perfume and catalog admin management
    /// </summary>
    [Authorize]
    [Route("Admin")]
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

        /// <summary>
        /// Initializes a new instance of the PerfumeAdminController class
        /// </summary>
        public PerfumeAdminController(
            IBrandService brandService,
            IDashboardService dashboardService,
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

        #region Brands Management

        /// <summary>
        /// Displays list of brands with pagination and search
        /// </summary>
        [HttpGet("Brands", Name = "MvcPerfumeAdminBrands")]
        public async Task<IActionResult> Brands(int pageIndex = 1, int pageSize = 10, string? searchTerm = null)
        {
            try
            {
                var brands = await _brandService.GetAllBrandsAsync(pageIndex, pageSize, searchTerm);
                ViewBag.SearchTerm = searchTerm;
                return View("~/Views/Admin/Brands/Index.cshtml", brands);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading brands");
                SetErrorMessage("Failed to load brands");
                return View("~/Views/Admin/Brands/Index.cshtml");
            }
        }

        /// <summary>
        /// Displays create brand form
        /// </summary>
        [HttpGet("CreateBrand", Name = "MvcPerfumeAdminCreateBrandGet")]
        public IActionResult CreateBrand()
        {
            return View("~/Views/Admin/Brands/Create.cshtml", new CreateBrandDto());
        }

        /// <summary>
        /// Creates a new brand
        /// </summary>
        [HttpPost("CreateBrand", Name = "MvcPerfumeAdminCreateBrandPost")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateBrand(CreateBrandDto dto)
        {
            if (!ModelState.IsValid)
                return View("~/Views/Admin/Brands/Create.cshtml", dto);

            try
            {
                if (await _brandService.BrandExistsAsync(dto.Name))
                {
                    ModelState.AddModelError("Name", "A brand with this name already exists");
                    return View("~/Views/Admin/Brands/Create.cshtml", dto);
                }

                await _brandService.CreateBrandAsync(dto);
                SetSuccessMessage("Brand created successfully");
                return RedirectToRoute("MvcPerfumeAdminBrands");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating brand");
                SetErrorMessage("Failed to create brand");
                return View("~/Views/Admin/Brands/Create.cshtml", dto);
            }
        }

        /// <summary>
        /// Displays edit brand form
        /// </summary>
        [HttpGet("EditBrand/{id}", Name = "MvcPerfumeAdminEditBrandGet")]
        public async Task<IActionResult> EditBrand(Guid id)
        {
            try
            {
                var brand = await _brandService.GetBrandForEditAsync(id);
                if (brand == null) return NotFound();
                return View("~/Views/Admin/Brands/Edit.cshtml", brand);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading brand for edit: {Id}", id);
                SetErrorMessage("Failed to load brand");
                return RedirectToRoute("MvcPerfumeAdminBrands");
            }
        }

        /// <summary>
        /// Updates an existing brand
        /// </summary>
        [HttpPost("EditBrand/{id}", Name = "MvcPerfumeAdminEditBrandPost")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditBrand(Guid id, UpdateBrandDto dto)
        {
            if (id != dto.Id) return BadRequest();

            if (!ModelState.IsValid)
                return View("~/Views/Admin/Brands/Edit.cshtml", dto);

            try
            {
                if (await _brandService.BrandExistsAsync(dto.Name, dto.Id))
                {
                    ModelState.AddModelError("Name", "A brand with this name already exists");
                    return View("~/Views/Admin/Brands/Edit.cshtml", dto);
                }

                var success = await _brandService.UpdateBrandAsync(dto);
                if (!success) return NotFound();

                SetSuccessMessage("Brand updated successfully");
                return RedirectToRoute("MvcPerfumeAdminBrands");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating brand: {Id}", id);
                SetErrorMessage("Failed to update brand");
                return View("~/Views/Admin/Brands/Edit.cshtml", dto);
            }
        }

        /// <summary>
        /// Deletes a brand
        /// </summary>
        [HttpPost("DeleteBrand/{id}", Name = "MvcPerfumeAdminDeleteBrand")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteBrand(Guid id)
        {
            try
            {
                var success = await _brandService.DeleteBrandAsync(id);
                if (!success)
                {
                    SetErrorMessage("Failed to delete brand");
                    return RedirectToRoute("MvcPerfumeAdminBrands");
                }

                SetSuccessMessage("Brand deleted successfully");
                return RedirectToRoute("MvcPerfumeAdminBrands");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting brand: {Id}", id);
                SetErrorMessage("Failed to delete brand");
                return RedirectToRoute("MvcPerfumeAdminBrands");
            }
        }

        #endregion

        #region Perfumes Management

        /// <summary>
        /// Displays list of perfumes with filtering and pagination
        /// </summary>
        [HttpGet("Perfumes", Name = "MvcPerfumeAdminPerfumes")]
        public async Task<IActionResult> Perfumes(
            int pageIndex = 1,
            int pageSize = 10,
            string? searchTerm = null,
            Guid? brandId = null,
            Guid? familyId = null,
            string? genderProfile = null)
        {
            try
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading perfumes");
                SetErrorMessage("Failed to load perfumes");
                return View("~/Views/Admin/Perfumes/Index.cshtml");
            }
        }

        /// <summary>
        /// Displays create perfume form
        /// </summary>
        [HttpGet("CreatePerfume", Name = "MvcPerfumeAdminCreatePerfumeGet")]
        public async Task<IActionResult> CreatePerfume()
        {
            try
            {
                await PopulatePerfumeViewBag();
                return View("~/Views/Admin/Perfumes/Create.cshtml", new CreatePerfumeDto());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading create perfume form");
                SetErrorMessage("Failed to load create form");
                return RedirectToRoute("MvcPerfumeAdminPerfumes");
            }
        }

        /// <summary>
        /// Creates a new perfume
        /// </summary>
        [HttpPost("CreatePerfume", Name = "MvcPerfumeAdminCreatePerfumePost")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePerfume(CreatePerfumeDto dto)
        {
            if (!ModelState.IsValid)
            {
                await PopulatePerfumeViewBag();
                return View("~/Views/Admin/Perfumes/Create.cshtml", dto);
            }

            try
            {
                await _perfumeService.CreatePerfumeAsync(dto);
                SetSuccessMessage("Perfume created successfully");
                return RedirectToRoute("MvcPerfumeAdminPerfumes");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating perfume");
                await PopulatePerfumeViewBag();
                SetErrorMessage("Failed to create perfume");
                return View("~/Views/Admin/Perfumes/Create.cshtml", dto);
            }
        }

        /// <summary>
        /// Displays edit perfume form
        /// </summary>
        [HttpGet("EditPerfume/{id}", Name = "MvcPerfumeAdminEditPerfumeGet")]
        public async Task<IActionResult> EditPerfume(Guid id)
        {
            try
            {
                var perfume = await _perfumeService.GetPerfumeForEditAsync(id);
                if (perfume == null) return NotFound();
                await PopulatePerfumeViewBag();
                return View("~/Views/Admin/Perfumes/Edit.cshtml", perfume);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading perfume for edit: {Id}", id);
                SetErrorMessage("Failed to load perfume");
                return RedirectToRoute("MvcPerfumeAdminPerfumes");
            }
        }

        /// <summary>
        /// Updates an existing perfume
        /// </summary>
        [HttpPost("EditPerfume/{id}", Name = "MvcPerfumeAdminEditPerfumePost")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPerfume(Guid id, UpdatePerfumeDto dto)
        {
            if (id != dto.Id) return BadRequest();

            if (!ModelState.IsValid)
            {
                await PopulatePerfumeViewBag();
                return View("~/Views/Admin/Perfumes/Edit.cshtml", dto);
            }

            try
            {
                var success = await _perfumeService.UpdatePerfumeAsync(dto);
                if (!success) return NotFound();
                SetSuccessMessage("Perfume updated successfully");
                return RedirectToRoute("MvcPerfumeAdminPerfumes");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating perfume: {Id}", id);
                await PopulatePerfumeViewBag();
                SetErrorMessage("Failed to update perfume");
                return View("~/Views/Admin/Perfumes/Edit.cshtml", dto);
            }
        }

        /// <summary>
        /// Displays perfume details
        /// </summary>
        [HttpGet("PerfumeDetails/{id}", Name = "MvcPerfumeAdminPerfumeDetails")]
        public async Task<IActionResult> PerfumeDetails(Guid id)
        {
            try
            {
                var perfume = await _perfumeService.GetPerfumeDetailsAsync(id);
                if (perfume == null) return NotFound();
                return View("~/Views/Admin/Perfumes/Details.cshtml", perfume);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading perfume details: {Id}", id);
                SetErrorMessage("Failed to load perfume");
                return RedirectToRoute("MvcPerfumeAdminPerfumes");
            }
        }

        /// <summary>
        /// Deletes a perfume
        /// </summary>
        [HttpPost("DeletePerfume/{id}", Name = "MvcPerfumeAdminDeletePerfume")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePerfume(Guid id)
        {
            try
            {
                var success = await _perfumeService.DeletePerfumeAsync(id);
                if (!success)
                {
                    SetErrorMessage("Failed to delete perfume");
                    return RedirectToRoute("MvcPerfumeAdminPerfumes");
                }
                SetSuccessMessage("Perfume deleted successfully");
                return RedirectToRoute("MvcPerfumeAdminPerfumes");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting perfume: {Id}", id);
                SetErrorMessage("Failed to delete perfume");
                return RedirectToRoute("MvcPerfumeAdminPerfumes");
            }
        }

        #endregion

        #region Families Management

        /// <summary>
        /// Displays list of families with pagination and search
        /// </summary>
        [HttpGet("Families", Name = "MvcPerfumeAdminFamilies")]
        public async Task<IActionResult> Families(int pageIndex = 1, int pageSize = 10, string? searchTerm = null)
        {
            try
            {
                var families = await _familyService.GetAllFamiliesAsync(pageIndex, pageSize, searchTerm);
                ViewBag.SearchTerm = searchTerm;
                return View("~/Views/Admin/Families/Index.cshtml", families);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading families");
                SetErrorMessage("Failed to load families");
                return View("~/Views/Admin/Families/Index.cshtml");
            }
        }

        /// <summary>
        /// Displays create family form
        /// </summary>
        [HttpGet("CreateFamily", Name = "MvcPerfumeAdminCreateFamilyGet")]
        public IActionResult CreateFamily()
        {
            return View("~/Views/Admin/Families/Create.cshtml", new CreateFamilyDto());
        }

        /// <summary>
        /// Creates a new family
        /// </summary>
        [HttpPost("CreateFamily", Name = "MvcPerfumeAdminCreateFamilyPost")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateFamily(CreateFamilyDto dto)
        {
            if (!ModelState.IsValid)
                return View("~/Views/Admin/Families/Create.cshtml", dto);

            try
            {
                if (await _familyService.FamilyExistsAsync(dto.Name))
                {
                    ModelState.AddModelError("Name", "A family with this name already exists");
                    return View("~/Views/Admin/Families/Create.cshtml", dto);
                }
                await _familyService.CreateFamilyAsync(dto);
                SetSuccessMessage("Family created successfully");
                return RedirectToRoute("MvcPerfumeAdminFamilies");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating family");
                SetErrorMessage("Failed to create family");
                return View("~/Views/Admin/Families/Create.cshtml", dto);
            }
        }

        /// <summary>
        /// Displays edit family form
        /// </summary>
        [HttpGet("EditFamily/{id}", Name = "MvcPerfumeAdminEditFamilyGet")]
        public async Task<IActionResult> EditFamily(Guid id)
        {
            try
            {
                var family = await _familyService.GetFamilyForEditAsync(id);
                if (family == null) return NotFound();
                return View("~/Views/Admin/Families/Edit.cshtml", family);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading family for edit: {Id}", id);
                SetErrorMessage("Failed to load family");
                return RedirectToRoute("MvcPerfumeAdminFamilies");
            }
        }

        /// <summary>
        /// Updates an existing family
        /// </summary>
        [HttpPost("EditFamily/{id}", Name = "MvcPerfumeAdminEditFamilyPost")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditFamily(Guid id, UpdateFamilyDto dto)
        {
            if (id != dto.Id) return BadRequest();

            if (!ModelState.IsValid)
                return View("~/Views/Admin/Families/Edit.cshtml", dto);

            try
            {
                if (await _familyService.FamilyExistsAsync(dto.Name, dto.Id))
                {
                    ModelState.AddModelError("Name", "A family with this name already exists");
                    return View("~/Views/Admin/Families/Edit.cshtml", dto);
                }
                var success = await _familyService.UpdateFamilyAsync(dto);
                if (!success) return NotFound();
                SetSuccessMessage("Family updated successfully");
                return RedirectToRoute("MvcPerfumeAdminFamilies");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating family: {Id}", id);
                SetErrorMessage("Failed to update family");
                return View("~/Views/Admin/Families/Edit.cshtml", dto);
            }
        }

        /// <summary>
        /// Deletes a family
        /// </summary>
        [HttpPost("DeleteFamily/{id}", Name = "MvcPerfumeAdminDeleteFamily")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteFamily(Guid id)
        {
            try
            {
                var success = await _familyService.DeleteFamilyAsync(id);
                if (!success)
                {
                    SetErrorMessage("Failed to delete family");
                    return RedirectToRoute("MvcPerfumeAdminFamilies");
                }
                SetSuccessMessage("Family deleted successfully");
                return RedirectToRoute("MvcPerfumeAdminFamilies");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting family: {Id}", id);
                SetErrorMessage("Failed to delete family");
                return RedirectToRoute("MvcPerfumeAdminFamilies");
            }
        }

        #endregion

        #region Notes Management

        /// <summary>
        /// Displays list of notes with pagination and category filtering
        /// </summary>
        [HttpGet("Notes", Name = "MvcPerfumeAdminNotes")]
        public async Task<IActionResult> Notes(int pageIndex = 1, int pageSize = 10, string? searchTerm = null, string? category = null)
        {
            try
            {
                var notes = await _noteService.GetAllNotesAsync(pageIndex, pageSize, searchTerm, category);
                ViewBag.SearchTerm = searchTerm;
                ViewBag.Category = category;
                ViewBag.Categories = await _noteService.GetNoteCategoriesAsync();
                return View("~/Views/Admin/Notes/Index.cshtml", notes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading notes");
                SetErrorMessage("Failed to load notes");
                return View("~/Views/Admin/Notes/Index.cshtml");
            }
        }

        /// <summary>
        /// Displays create note form
        /// </summary>
        [HttpGet("CreateNote", Name = "MvcPerfumeAdminCreateNoteGet")]
        public async Task<IActionResult> CreateNote()
        {
            try
            {
                ViewBag.Categories = await _noteService.GetNoteCategoriesAsync();
                return View("~/Views/Admin/Notes/Create.cshtml", new CreateNoteDto());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading create note form");
                SetErrorMessage("Failed to load create form");
                return RedirectToRoute("MvcPerfumeAdminNotes");
            }
        }

        /// <summary>
        /// Creates a new note
        /// </summary>
        [HttpPost("CreateNote", Name = "MvcPerfumeAdminCreateNotePost")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateNote(CreateNoteDto dto)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Categories = await _noteService.GetNoteCategoriesAsync();
                return View("~/Views/Admin/Notes/Create.cshtml", dto);
            }

            try
            {
                if (await _noteService.NoteExistsAsync(dto.Name))
                {
                    ModelState.AddModelError("Name", "A note with this name already exists");
                    ViewBag.Categories = await _noteService.GetNoteCategoriesAsync();
                    return View("~/Views/Admin/Notes/Create.cshtml", dto);
                }
                await _noteService.CreateNoteAsync(dto);
                SetSuccessMessage("Note created successfully");
                return RedirectToRoute("MvcPerfumeAdminNotes");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating note");
                ViewBag.Categories = await _noteService.GetNoteCategoriesAsync();
                SetErrorMessage("Failed to create note");
                return View("~/Views/Admin/Notes/Create.cshtml", dto);
            }
        }

        /// <summary>
        /// Displays edit note form
        /// </summary>
        [HttpGet("EditNote/{id}", Name = "MvcPerfumeAdminEditNoteGet")]
        public async Task<IActionResult> EditNote(Guid id)
        {
            try
            {
                var note = await _noteService.GetNoteForEditAsync(id);
                if (note == null) return NotFound();
                ViewBag.Categories = await _noteService.GetNoteCategoriesAsync();
                return View("~/Views/Admin/Notes/Edit.cshtml", note);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading note for edit: {Id}", id);
                SetErrorMessage("Failed to load note");
                return RedirectToRoute("MvcPerfumeAdminNotes");
            }
        }

        /// <summary>
        /// Updates an existing note
        /// </summary>
        [HttpPost("EditNote/{id}", Name = "MvcPerfumeAdminEditNotePost")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditNote(Guid id, UpdateNoteDto dto)
        {
            if (id != dto.Id) return BadRequest();

            if (!ModelState.IsValid)
            {
                ViewBag.Categories = await _noteService.GetNoteCategoriesAsync();
                return View("~/Views/Admin/Notes/Edit.cshtml", dto);
            }

            try
            {
                if (await _noteService.NoteExistsAsync(dto.Name, dto.Id))
                {
                    ModelState.AddModelError("Name", "A note with this name already exists");
                    ViewBag.Categories = await _noteService.GetNoteCategoriesAsync();
                    return View("~/Views/Admin/Notes/Edit.cshtml", dto);
                }
                var success = await _noteService.UpdateNoteAsync(dto);
                if (!success) return NotFound();
                SetSuccessMessage("Note updated successfully");
                return RedirectToRoute("MvcPerfumeAdminNotes");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating note: {Id}", id);
                ViewBag.Categories = await _noteService.GetNoteCategoriesAsync();
                SetErrorMessage("Failed to update note");
                return View("~/Views/Admin/Notes/Edit.cshtml", dto);
            }
        }

        /// <summary>
        /// Deletes a note
        /// </summary>
        [HttpPost("DeleteNote/{id}", Name = "MvcPerfumeAdminDeleteNote")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteNote(Guid id)
        {
            try
            {
                var success = await _noteService.DeleteNoteAsync(id);
                if (!success)
                {
                    SetErrorMessage("Failed to delete note");
                    return RedirectToRoute("MvcPerfumeAdminNotes");
                }
                SetSuccessMessage("Note deleted successfully");
                return RedirectToRoute("MvcPerfumeAdminNotes");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting note: {Id}", id);
                SetErrorMessage("Failed to delete note");
                return RedirectToRoute("MvcPerfumeAdminNotes");
            }
        }

        #endregion

        #region Accords Management

        /// <summary>
        /// Displays list of accords with pagination and search
        /// </summary>
        [HttpGet("Accords", Name = "MvcPerfumeAdminAccords")]
        public async Task<IActionResult> Accords(int pageIndex = 1, int pageSize = 10, string? searchTerm = null)
        {
            try
            {
                var accords = await _accordService.GetAllAccordsAsync(pageIndex, pageSize, searchTerm);
                ViewBag.SearchTerm = searchTerm;
                return View("~/Views/Admin/Accords/Index.cshtml", accords);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading accords");
                SetErrorMessage("Failed to load accords");
                return View("~/Views/Admin/Accords/Index.cshtml");
            }
        }

        /// <summary>
        /// Displays create accord form
        /// </summary>
        [HttpGet("CreateAccord", Name = "MvcPerfumeAdminCreateAccordGet")]
        public IActionResult CreateAccord()
        {
            return View("~/Views/Admin/Accords/Create.cshtml", new CreateAccordDto());
        }

        /// <summary>
        /// Creates a new accord
        /// </summary>
        [HttpPost("CreateAccord", Name = "MvcPerfumeAdminCreateAccordPost")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateAccord(CreateAccordDto dto)
        {
            if (!ModelState.IsValid)
                return View("~/Views/Admin/Accords/Create.cshtml", dto);

            try
            {
                if (await _accordService.AccordExistsAsync(dto.Name))
                {
                    ModelState.AddModelError("Name", "An accord with this name already exists");
                    return View("~/Views/Admin/Accords/Create.cshtml", dto);
                }
                await _accordService.CreateAccordAsync(dto);
                SetSuccessMessage("Accord created successfully");
                return RedirectToRoute("MvcPerfumeAdminAccords");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating accord");
                SetErrorMessage("Failed to create accord");
                return View("~/Views/Admin/Accords/Create.cshtml", dto);
            }
        }

        /// <summary>
        /// Displays edit accord form
        /// </summary>
        [HttpGet("EditAccord/{id}", Name = "MvcPerfumeAdminEditAccordGet")]
        public async Task<IActionResult> EditAccord(Guid id)
        {
            try
            {
                var accord = await _accordService.GetAccordForEditAsync(id);
                if (accord == null) return NotFound();
                return View("~/Views/Admin/Accords/Edit.cshtml", accord);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading accord for edit: {Id}", id);
                SetErrorMessage("Failed to load accord");
                return RedirectToRoute("MvcPerfumeAdminAccords");
            }
        }

        /// <summary>
        /// Updates an existing accord
        /// </summary>
        [HttpPost("EditAccord/{id}", Name = "MvcPerfumeAdminEditAccordPost")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditAccord(Guid id, UpdateAccordDto dto)
        {
            if (id != dto.Id) return BadRequest();

            if (!ModelState.IsValid)
                return View("~/Views/Admin/Accords/Edit.cshtml", dto);

            try
            {
                if (await _accordService.AccordExistsAsync(dto.Name, dto.Id))
                {
                    ModelState.AddModelError("Name", "An accord with this name already exists");
                    return View("~/Views/Admin/Accords/Edit.cshtml", dto);
                }
                var success = await _accordService.UpdateAccordAsync(dto);
                if (!success) return NotFound();
                SetSuccessMessage("Accord updated successfully");
                return RedirectToRoute("MvcPerfumeAdminAccords");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating accord: {Id}", id);
                SetErrorMessage("Failed to update accord");
                return View("~/Views/Admin/Accords/Edit.cshtml", dto);
            }
        }

        /// <summary>
        /// Deletes an accord
        /// </summary>
        [HttpPost("DeleteAccord/{id}", Name = "MvcPerfumeAdminDeleteAccord")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAccord(Guid id)
        {
            try
            {
                var success = await _accordService.DeleteAccordAsync(id);
                if (!success)
                {
                    SetErrorMessage("Failed to delete accord");
                    return RedirectToRoute("MvcPerfumeAdminAccords");
                }
                SetSuccessMessage("Accord deleted successfully");
                return RedirectToRoute("MvcPerfumeAdminAccords");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting accord: {Id}", id);
                SetErrorMessage("Failed to delete accord");
                return RedirectToRoute("MvcPerfumeAdminAccords");
            }
        }

        #endregion

        #region Tags Management

        /// <summary>
        /// Displays list of tags with pagination and search
        /// </summary>
        [HttpGet("Tags", Name = "MvcPerfumeAdminTags")]
        public async Task<IActionResult> Tags(int pageIndex = 1, int pageSize = 10, string? searchTerm = null)
        {
            try
            {
                var tags = await _tagService.GetAllTagsAsync(pageIndex, pageSize, searchTerm);
                ViewBag.SearchTerm = searchTerm;
                return View("~/Views/Admin/Tags/Index.cshtml", tags);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading tags");
                SetErrorMessage("Failed to load tags");
                return View("~/Views/Admin/Tags/Index.cshtml");
            }
        }

        /// <summary>
        /// Displays create tag form
        /// </summary>
        [HttpGet("CreateTag", Name = "MvcPerfumeAdminCreateTagGet")]
        public IActionResult CreateTag()
        {
            return View("~/Views/Admin/Tags/Create.cshtml", new CreateTagDto());
        }

        /// <summary>
        /// Creates a new tag
        /// </summary>
        [HttpPost("CreateTag", Name = "MvcPerfumeAdminCreateTagPost")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateTag(CreateTagDto dto)
        {
            if (!ModelState.IsValid)
                return View("~/Views/Admin/Tags/Create.cshtml", dto);

            try
            {
                if (await _tagService.TagExistsAsync(dto.Name))
                {
                    ModelState.AddModelError("Name", "A tag with this name already exists");
                    return View("~/Views/Admin/Tags/Create.cshtml", dto);
                }
                await _tagService.CreateTagAsync(dto);
                SetSuccessMessage("Tag created successfully");
                return RedirectToRoute("MvcPerfumeAdminTags");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating tag");
                SetErrorMessage("Failed to create tag");
                return View("~/Views/Admin/Tags/Create.cshtml", dto);
            }
        }

        /// <summary>
        /// Displays edit tag form
        /// </summary>
        [HttpGet("EditTag/{id}", Name = "MvcPerfumeAdminEditTagGet")]
        public async Task<IActionResult> EditTag(Guid id)
        {
            try
            {
                var tag = await _tagService.GetTagForEditAsync(id);
                if (tag == null) return NotFound();
                return View("~/Views/Admin/Tags/Edit.cshtml", tag);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading tag for edit: {Id}", id);
                SetErrorMessage("Failed to load tag");
                return RedirectToRoute("MvcPerfumeAdminTags");
            }
        }

        /// <summary>
        /// Updates an existing tag
        /// </summary>
        [HttpPost("EditTag/{id}", Name = "MvcPerfumeAdminEditTagPost")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditTag(Guid id, UpdateTagDto dto)
        {
            if (id != dto.Id) return BadRequest();

            if (!ModelState.IsValid)
                return View("~/Views/Admin/Tags/Edit.cshtml", dto);

            try
            {
                if (await _tagService.TagExistsAsync(dto.Name, dto.Id))
                {
                    ModelState.AddModelError("Name", "A tag with this name already exists");
                    return View("~/Views/Admin/Tags/Edit.cshtml", dto);
                }
                var success = await _tagService.UpdateTagAsync(dto);
                if (!success) return NotFound();
                SetSuccessMessage("Tag updated successfully");
                return RedirectToRoute("MvcPerfumeAdminTags");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating tag: {Id}", id);
                SetErrorMessage("Failed to update tag");
                return View("~/Views/Admin/Tags/Edit.cshtml", dto);
            }
        }

        /// <summary>
        /// Deletes a tag
        /// </summary>
        [HttpPost("DeleteTag/{id}", Name = "MvcPerfumeAdminDeleteTag")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteTag(Guid id)
        {
            try
            {
                var success = await _tagService.DeleteTagAsync(id);
                if (!success)
                {
                    SetErrorMessage("Failed to delete tag");
                    return RedirectToRoute("MvcPerfumeAdminTags");
                }
                SetSuccessMessage("Tag deleted successfully");
                return RedirectToRoute("MvcPerfumeAdminTags");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting tag: {Id}", id);
                SetErrorMessage("Failed to delete tag");
                return RedirectToRoute("MvcPerfumeAdminTags");
            }
        }

        #endregion

        #region Seasons Management

        /// <summary>
        /// Displays list of seasons with pagination and search
        /// </summary>
        [HttpGet("Seasons", Name = "MvcPerfumeAdminSeasons")]
        public async Task<IActionResult> Seasons(int pageIndex = 1, int pageSize = 10, string? searchTerm = null)
        {
            try
            {
                var seasons = await _seasonService.GetAllSeasonsAsync(pageIndex, pageSize, searchTerm);
                ViewBag.SearchTerm = searchTerm;
                return View("~/Views/Admin/Seasons/Index.cshtml", seasons);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading seasons");
                SetErrorMessage("Failed to load seasons");
                return View("~/Views/Admin/Seasons/Index.cshtml");
            }
        }

        /// <summary>
        /// Displays create season form
        /// </summary>
        [HttpGet("CreateSeason", Name = "MvcPerfumeAdminCreateSeasonGet")]
        public IActionResult CreateSeason()
        {
            return View("~/Views/Admin/Seasons/Create.cshtml", new CreateSeasonDto());
        }

        /// <summary>
        /// Creates a new season
        /// </summary>
        [HttpPost("CreateSeason", Name = "MvcPerfumeAdminCreateSeasonPost")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateSeason(CreateSeasonDto dto)
        {
            if (!ModelState.IsValid)
                return View("~/Views/Admin/Seasons/Create.cshtml", dto);

            try
            {
                if (await _seasonService.SeasonExistsAsync(dto.Name))
                {
                    ModelState.AddModelError("Name", "A season with this name already exists");
                    return View("~/Views/Admin/Seasons/Create.cshtml", dto);
                }
                await _seasonService.CreateSeasonAsync(dto);
                SetSuccessMessage("Season created successfully");
                return RedirectToRoute("MvcPerfumeAdminSeasons");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating season");
                SetErrorMessage("Failed to create season");
                return View("~/Views/Admin/Seasons/Create.cshtml", dto);
            }
        }

        /// <summary>
        /// Displays edit season form
        /// </summary>
        [HttpGet("EditSeason/{id}", Name = "MvcPerfumeAdminEditSeasonGet")]
        public async Task<IActionResult> EditSeason(Guid id)
        {
            try
            {
                var season = await _seasonService.GetSeasonForEditAsync(id);
                if (season == null) return NotFound();
                return View("~/Views/Admin/Seasons/Edit.cshtml", season);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading season for edit: {Id}", id);
                SetErrorMessage("Failed to load season");
                return RedirectToRoute("MvcPerfumeAdminSeasons");
            }
        }

        /// <summary>
        /// Updates an existing season
        /// </summary>
        [HttpPost("EditSeason/{id}", Name = "MvcPerfumeAdminEditSeasonPost")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditSeason(Guid id, UpdateSeasonDto dto)
        {
            if (id != dto.Id) return BadRequest();

            if (!ModelState.IsValid)
                return View("~/Views/Admin/Seasons/Edit.cshtml", dto);

            try
            {
                if (await _seasonService.SeasonExistsAsync(dto.Name, dto.Id))
                {
                    ModelState.AddModelError("Name", "A season with this name already exists");
                    return View("~/Views/Admin/Seasons/Edit.cshtml", dto);
                }
                var success = await _seasonService.UpdateSeasonAsync(dto);
                if (!success) return NotFound();
                SetSuccessMessage("Season updated successfully");
                return RedirectToRoute("MvcPerfumeAdminSeasons");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating season: {Id}", id);
                SetErrorMessage("Failed to update season");
                return View("~/Views/Admin/Seasons/Edit.cshtml", dto);
            }
        }

        /// <summary>
        /// Deletes a season
        /// </summary>
        [HttpPost("DeleteSeason/{id}", Name = "MvcPerfumeAdminDeleteSeason")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteSeason(Guid id)
        {
            try
            {
                var success = await _seasonService.DeleteSeasonAsync(id);
                if (!success)
                {
                    SetErrorMessage("Failed to delete season");
                    return RedirectToRoute("MvcPerfumeAdminSeasons");
                }
                SetSuccessMessage("Season deleted successfully");
                return RedirectToRoute("MvcPerfumeAdminSeasons");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting season: {Id}", id);
                SetErrorMessage("Failed to delete season");
                return RedirectToRoute("MvcPerfumeAdminSeasons");
            }
        }

        #endregion

        #region Occasions Management

        /// <summary>
        /// Displays list of occasions with pagination and search
        /// </summary>
        [HttpGet("Occasions", Name = "MvcPerfumeAdminOccasions")]
        public async Task<IActionResult> Occasions(int pageIndex = 1, int pageSize = 10, string? searchTerm = null)
        {
            try
            {
                var occasions = await _occasionService.GetAllOccasionsAsync(pageIndex, pageSize, searchTerm);
                ViewBag.SearchTerm = searchTerm;
                return View("~/Views/Admin/Occasions/Index.cshtml", occasions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading occasions");
                SetErrorMessage("Failed to load occasions");
                return View("~/Views/Admin/Occasions/Index.cshtml");
            }
        }

        /// <summary>
        /// Displays create occasion form
        /// </summary>
        [HttpGet("CreateOccasion", Name = "MvcPerfumeAdminCreateOccasionGet")]
        public IActionResult CreateOccasion()
        {
            return View("~/Views/Admin/Occasions/Create.cshtml", new CreateOccasionDto());
        }

        /// <summary>
        /// Creates a new occasion
        /// </summary>
        [HttpPost("CreateOccasion", Name = "MvcPerfumeAdminCreateOccasionPost")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateOccasion(CreateOccasionDto dto)
        {
            if (!ModelState.IsValid)
                return View("~/Views/Admin/Occasions/Create.cshtml", dto);

            try
            {
                if (await _occasionService.OccasionExistsAsync(dto.Name))
                {
                    ModelState.AddModelError("Name", "An occasion with this name already exists");
                    return View("~/Views/Admin/Occasions/Create.cshtml", dto);
                }
                await _occasionService.CreateOccasionAsync(dto);
                SetSuccessMessage("Occasion created successfully");
                return RedirectToRoute("MvcPerfumeAdminOccasions");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating occasion");
                SetErrorMessage("Failed to create occasion");
                return View("~/Views/Admin/Occasions/Create.cshtml", dto);
            }
        }

        /// <summary>
        /// Displays edit occasion form
        /// </summary>
        [HttpGet("EditOccasion/{id}", Name = "MvcPerfumeAdminEditOccasionGet")]
        public async Task<IActionResult> EditOccasion(Guid id)
        {
            try
            {
                var occasion = await _occasionService.GetOccasionForEditAsync(id);
                if (occasion == null) return NotFound();
                return View("~/Views/Admin/Occasions/Edit.cshtml", occasion);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading occasion for edit: {Id}", id);
                SetErrorMessage("Failed to load occasion");
                return RedirectToRoute("MvcPerfumeAdminOccasions");
            }
        }

        /// <summary>
        /// Updates an existing occasion
        /// </summary>
        [HttpPost("EditOccasion/{id}", Name = "MvcPerfumeAdminEditOccasionPost")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditOccasion(Guid id, UpdateOccasionDto dto)
        {
            if (id != dto.Id) return BadRequest();

            if (!ModelState.IsValid)
                return View("~/Views/Admin/Occasions/Edit.cshtml", dto);

            try
            {
                if (await _occasionService.OccasionExistsAsync(dto.Name, dto.Id))
                {
                    ModelState.AddModelError("Name", "An occasion with this name already exists");
                    return View("~/Views/Admin/Occasions/Edit.cshtml", dto);
                }
                var success = await _occasionService.UpdateOccasionAsync(dto);
                if (!success) return NotFound();
                SetSuccessMessage("Occasion updated successfully");
                return RedirectToRoute("MvcPerfumeAdminOccasions");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating occasion: {Id}", id);
                SetErrorMessage("Failed to update occasion");
                return View("~/Views/Admin/Occasions/Edit.cshtml", dto);
            }
        }

        /// <summary>
        /// Deletes an occasion
        /// </summary>
        [HttpPost("DeleteOccasion/{id}", Name = "MvcPerfumeAdminDeleteOccasion")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteOccasion(Guid id)
        {
            try
            {
                var success = await _occasionService.DeleteOccasionAsync(id);
                if (!success)
                {
                    SetErrorMessage("Failed to delete occasion");
                    return RedirectToRoute("MvcPerfumeAdminOccasions");
                }
                SetSuccessMessage("Occasion deleted successfully");
                return RedirectToRoute("MvcPerfumeAdminOccasions");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting occasion: {Id}", id);
                SetErrorMessage("Failed to delete occasion");
                return RedirectToRoute("MvcPerfumeAdminOccasions");
            }
        }

        #endregion

        #region Private Helper Methods

        /// <summary>
        /// Populates ViewBag with perfume-related dropdown data
        /// </summary>
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

        /// <summary>
        /// Sets a success message in TempData
        /// </summary>
        private void SetSuccessMessage(string message)
        {
            TempData["Success"] = message;
        }

        /// <summary>
        /// Sets an error message in TempData
        /// </summary>
        private void SetErrorMessage(string message)
        {
            TempData["Error"] = message;
        }

        #endregion
    }
}
