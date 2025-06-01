// CaterManagementSystem.Areas.Admin.Controllers.EventsController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using CaterManagementSystem.Models;
using System.IO;
using System.Threading.Tasks;
using CaterManagementSystem.Data;
using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace CaterManagementSystem.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class EventsController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<EventsController> _logger;
        private const string ImageUploadPath = "uploads/event_images"; // Şəkillərin yüklənəcəyi qovluq

        public EventsController(AppDbContext context, IWebHostEnvironment env, ILogger<EventsController> logger)
        {
            _context = context;
            _env = env;
            _logger = logger;
        }

        // GET: Admin/Events
        public async Task<IActionResult> Index()
        {
            var events = await _context.Events.OrderByDescending(e => e.Id).ToListAsync();
            return View(events);
        }

        // GET: Admin/Events/Create
        public IActionResult Create()
        {
            return View(new Event());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Title")] Event eventModel, IFormFile? Photo) // Bind-dan ImagePath-i çıxarırıq
        {
            // 1. Şəkil yoxlaması
            if (Photo == null || Photo.Length == 0)
            {
                ModelState.AddModelError("Photo", "Tədbir üçün şəkil seçilməlidir.");
            }

            // 2. Modelin qalan hissəsinin validasiyası (Title üçün)
            // Əgər Title [Required] isə və boşdursa, ModelState.IsValid onsuz da false olacaq.
            // Əgər başqa validasiyalarınız varsa, onlar da burada yoxlanılacaq.

            if (ModelState.IsValid) // Bütün yoxlamalardan sonra
            {
                // ModelState.IsValid true-dursa, deməli həm Title doludur, həm də Photo seçilib.
                try
                {
                    string folderPath = Path.Combine(_env.WebRootPath, ImageUploadPath);
                    Directory.CreateDirectory(folderPath);

                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(Photo.FileName); // Photo burada null ola bilməz
                    string filePath = Path.Combine(folderPath, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await Photo.CopyToAsync(stream);
                    }
                    eventModel.ImagePath = Path.Combine(ImageUploadPath, fileName).Replace("\\", "/");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error uploading image for new Event.");
                    ModelState.AddModelError("Photo", "Şəkil yüklənərkən xəta baş verdi.");
                    return View(eventModel); // Şəkil yükləmə xətası ilə geri qayıt
                }

                _context.Add(eventModel); // eventModel-də artıq ImagePath var
                await _context.SaveChangesAsync();
                _logger.LogInformation("New Event '{EventTitle}' created with ID {EventId}.", eventModel.Title, eventModel.Id);
                TempData["SuccessMessage"] = "Yeni tədbir uğurla yaradıldı.";
                return RedirectToAction(nameof(Index));
            }

            // ModelState.IsValid false olarsa (ya Title boşdur, ya da Photo seçilməyib/xətası var)
            _logger.LogWarning("Event creation failed due to invalid model state. Errors: {Errors}",
                string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
            return View(eventModel);
        }
        // GET: Admin/Events/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var eventModel = await _context.Events.FindAsync(id);
            if (eventModel == null) return NotFound();
            return View(eventModel);
        }

        // POST: Admin/Events/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,ImagePath")] Event eventModel, IFormFile? Photo)
        {
            if (id != eventModel.Id) return NotFound();

            if (ModelState.IsValid)
            {
                var eventToUpdate = await _context.Events.FindAsync(id);
                if (eventToUpdate == null) return NotFound();

                string? oldImagePath = eventToUpdate.ImagePath;

                if (Photo != null && Photo.Length > 0)
                {
                    try
                    {
                        string folderPath = Path.Combine(_env.WebRootPath, ImageUploadPath);
                        Directory.CreateDirectory(folderPath);

                        string fileName = Guid.NewGuid().ToString() + Path.GetExtension(Photo.FileName);
                        string filePath = Path.Combine(folderPath, fileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await Photo.CopyToAsync(stream);
                        }
                        eventToUpdate.ImagePath = Path.Combine(ImageUploadPath, fileName).Replace("\\", "/");

                        // Köhnə şəkli sil
                        if (!string.IsNullOrEmpty(oldImagePath) && oldImagePath != eventToUpdate.ImagePath)
                        {
                            string fullOldPath = Path.Combine(_env.WebRootPath, oldImagePath.TrimStart('/'));
                            if (System.IO.File.Exists(fullOldPath))
                            {
                                System.IO.File.Delete(fullOldPath);
                                _logger.LogInformation("Old image deleted: {OldImagePath}", fullOldPath);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error uploading image for Event ID {EventId}.", id);
                        ModelState.AddModelError("Photo", "Şəkil yüklənərkən xəta baş verdi.");
                        // eventModel.ImagePath = oldImagePath; // Xəta olarsa köhnə şəkli göstər
                        return View(eventModel); // Formdan gələn modeli qaytarırıq, ImagePath-i əl ilə düzəldə bilərik
                    }
                }
                else
                {
                    // Yeni şəkil yüklənməyibsə, modeldən gələn köhnə ImagePath-i qoru
                    eventToUpdate.ImagePath = eventModel.ImagePath;
                }

                eventToUpdate.Title = eventModel.Title;

                try
                {
                    //_context.Update(eventToUpdate); // Artıq context tərəfindən izlənilir
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Event ID {EventId} ('{EventTitle}') updated.", id, eventToUpdate.Title);
                    TempData["SuccessMessage"] = "Tədbir uğurla yeniləndi.";
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    _logger.LogError(ex, "Concurrency error while updating Event ID {EventId}", id);
                    if (!EventExists(eventModel.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            // ModelState.IsValid false olarsa, köhnə şəkli göstərmək üçün
            if (Photo == null && !string.IsNullOrEmpty(eventModel.ImagePath))
            {
                // Bu lazım deyil, çünki hidden inputda ImagePath saxlanılır
            }
            else if (Photo == null && string.IsNullOrEmpty(eventModel.ImagePath) && id > 0)
            {
                var originalEvent = await _context.Events.AsNoTracking().FirstOrDefaultAsync(e => e.Id == id);
                eventModel.ImagePath = originalEvent?.ImagePath;
            }
            return View(eventModel);
        }

        // GET: Admin/Events/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var eventModel = await _context.Events.FirstOrDefaultAsync(m => m.Id == id);
            if (eventModel == null) return NotFound();

            return View(eventModel);
        }

        // POST: Admin/Events/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var eventModel = await _context.Events.FindAsync(id);
            if (eventModel == null) return NotFound();

            try
            {
                // Şəkli sil
                if (!string.IsNullOrEmpty(eventModel.ImagePath))
                {
                    string fullPath = Path.Combine(_env.WebRootPath, eventModel.ImagePath.TrimStart('/'));
                    if (System.IO.File.Exists(fullPath))
                    {
                        System.IO.File.Delete(fullPath);
                        _logger.LogInformation("Image deleted for Event ID {EventId}: {ImagePath}", id, fullPath);
                    }
                }

                _context.Events.Remove(eventModel);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Event ID {EventId} ('{EventTitle}') deleted.", id, eventModel.Title);
                TempData["SuccessMessage"] = "Tədbir uğurla silindi.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting Event ID {EventId}", id);
                TempData["ErrorMessage"] = "Tədbir silinərkən xəta baş verdi.";
            }
            return RedirectToAction(nameof(Index));
        }

        private bool EventExists(int id)
        {
            return _context.Events.Any(e => e.Id == id);
        }
    }
}