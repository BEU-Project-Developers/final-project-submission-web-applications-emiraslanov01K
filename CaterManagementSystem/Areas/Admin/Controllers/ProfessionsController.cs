using Microsoft.AspNetCore.Mvc;
using CaterManagementSystem.Models;
using System.Threading.Tasks;
using CaterManagementSystem.Data;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace CaterManagementSystem.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ProfessionsController : Controller
    {
        private readonly AppDbContext _context;
        private readonly ILogger<ProfessionsController> _logger;

        public ProfessionsController(AppDbContext context, ILogger<ProfessionsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Admin/Professions
        public async Task<IActionResult> Index()
        {
            var professions = await _context.Professions.Include(p => p.TeamMembers).ToListAsync();
            return View(professions);
        }

        // GET: Admin/Professions/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Admin/Professions/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name")] Profession profession)
        {
            if (ModelState.IsValid)
            {
                // Eyni adda peşə olub olmadığını yoxlaya bilərik
                if (await _context.Professions.AnyAsync(p => p.Name.ToLower() == profession.Name.ToLower()))
                {
                    ModelState.AddModelError("Name", "Bu adda peşə artıq mövcuddur.");
                    return View(profession);
                }

                _context.Add(profession);
                await _context.SaveChangesAsync();
                _logger.LogInformation("New Profession '{ProfessionName}' created with ID {ProfessionId}.", profession.Name, profession.Id);
                TempData["SuccessMessage"] = "Yeni peşə uğurla yaradıldı.";
                return RedirectToAction(nameof(Index));
            }
            return View(profession);
        }

        // GET: Admin/Professions/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var profession = await _context.Professions.FindAsync(id);
            if (profession == null) return NotFound();
            return View(profession);
        }

        // POST: Admin/Professions/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name")] Profession profession)
        {
            if (id != profession.Id) return NotFound();

            if (ModelState.IsValid)
            {
                // Eyni adda başqa peşə olub olmadığını yoxlaya bilərik (hazırkı ID xaric)
                if (await _context.Professions.AnyAsync(p => p.Name.ToLower() == profession.Name.ToLower() && p.Id != profession.Id))
                {
                    ModelState.AddModelError("Name", "Bu adda başqa bir peşə artıq mövcuddur.");
                    return View(profession);
                }
                try
                {
                    _context.Update(profession);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Profession ID {ProfessionId} ('{ProfessionName}') updated.", profession.Id, profession.Name);
                    TempData["SuccessMessage"] = "Peşə uğurla yeniləndi.";
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    _logger.LogError(ex, "Concurrency error while updating Profession ID {ProfessionId}", profession.Id);
                    if (!ProfessionExists(profession.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(profession);
        }

        // GET: Admin/Professions/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var profession = await _context.Professions
                .Include(p => p.TeamMembers) // Əlaqəli komanda üzvlərini yükləyirik
                .FirstOrDefaultAsync(m => m.Id == id);
            if (profession == null) return NotFound();

            return View(profession);
        }

        // POST: Admin/Professions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var profession = await _context.Professions.Include(p => p.TeamMembers).FirstOrDefaultAsync(p => p.Id == id);
            if (profession == null) return NotFound();

            // Əgər bu peşəyə bağlı komanda üzvləri varsa, silməyə icazə verməyək
            // Və ya həmin komanda üzvlərinin ProfessionId-sini null etmək və ya başqa bir default peşəyə təyin etmək olar.
            // Sadəlik üçün, bağlı üzv varsa silməyə icazə vermirik.
            if (profession.TeamMembers != null && profession.TeamMembers.Any())
            {
                _logger.LogWarning("Attempted to delete Profession ID {ProfessionId} ('{ProfessionName}') which has associated team members.", profession.Id, profession.Name);
                TempData["ErrorMessage"] = $"'{profession.Name}' peşəsi silinə bilməz, çünki bu peşəyə bağlı komanda üzvləri var. Əvvəlcə həmin üzvləri silin və ya peşələrini dəyişin.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                _context.Professions.Remove(profession);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Profession ID {ProfessionId} ('{ProfessionName}') deleted.", profession.Id, profession.Name);
                TempData["SuccessMessage"] = "Peşə uğurla silindi.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting Profession ID {ProfessionId}", profession.Id);
                TempData["ErrorMessage"] = "Peşə silinərkən xəta baş verdi.";
            }
            return RedirectToAction(nameof(Index));
        }

        private bool ProfessionExists(int id)
        {
            return _context.Professions.Any(e => e.Id == id);
        }
    }
}