// CaterManagementSystem.Areas.Admin.Controllers.AboutController.cs
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
    public class AboutController : Controller
    {
        private readonly IWebHostEnvironment _env;
        private readonly AppDbContext _context;
        private readonly ILogger<AboutController> _logger;

        // Şəkillərin yüklənəcəyi qovluq (wwwroot içində)
        private const string ImageUploadDirectory = "All-Images"; // Qovluq adı dəyişdirildi

        public AboutController(AppDbContext context, IWebHostEnvironment env, ILogger<AboutController> logger)
        {
            _context = context;
            _env = env;
            _logger = logger;
        }

        // INDEX metodu eyni qalır...
        public async Task<IActionResult> Index()
        {
            var abouts = await _context.About.ToListAsync();
            return View(abouts);
        }


        // CREATE - GET metodu eyni qalır...
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            if (await _context.About.AnyAsync())
            {
                var existingAbout = await _context.About.FirstOrDefaultAsync();
                if (existingAbout != null)
                {
                    TempData["InfoMessage"] = "'Haqqımızda' məlumatı artıq mövcuddur. Yalnız redaktə edə bilərsiniz.";
                    return RedirectToAction(nameof(Update), new { id = existingAbout.Id });
                }
            }
            return View(new About());
        }


        // CREATE - POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(About model)
        {
            if (await _context.About.AnyAsync())
            {
                ModelState.AddModelError(string.Empty, "'Haqqımızda' məlumatı artıq yaradılıb. Yalnız mövcud olanı redaktə edə bilərsiniz.");
                // return View(model);
            }

            if (ModelState.IsValid)
            {
                if (model.Photo != null && model.Photo.Length > 0)
                {
                    try
                    {
                        // DƏYİŞİKLİK: Qovluq yolu ImageUploadDirectory istifadə edərək
                        string folderPath = Path.Combine(_env.WebRootPath, ImageUploadDirectory);
                        if (!Directory.Exists(folderPath))
                        {
                            Directory.CreateDirectory(folderPath);
                            _logger.LogInformation("Created directory: {DirectoryPath}", folderPath);
                        }

                        string fileName = Guid.NewGuid().ToString() + Path.GetExtension(model.Photo.FileName);
                        string filePath = Path.Combine(folderPath, fileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await model.Photo.CopyToAsync(stream);
                        }
                        // DƏYİŞİKLİK: Verilənlər bazasına yazılan yol
                        // Path.Combine wwwroot-dan sonrakı hissəni alacaq
                        model.ImagePath = Path.Combine(ImageUploadDirectory, fileName).Replace("\\", "/");
                        // Və ya birbaşa belə yaza bilərsiniz:
                        // model.ImagePath = $"{ImageUploadDirectory}/{fileName}";
                        _logger.LogInformation("Image uploaded for new About section: {ImagePath}", model.ImagePath);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error uploading image for new About section.");
                        ModelState.AddModelError("Photo", "Şəkil yüklənərkən xəta baş verdi.");
                        return View(model);
                    }
                }

                await _context.About.AddAsync(model);
                await _context.SaveChangesAsync();
                _logger.LogInformation("New About section created with ID {Id}", model.Id);
                TempData["SuccessMessage"] = "'Haqqımızda' məlumatı uğurla yaradıldı.";
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        // UPDATE - GET metodu eyni qalır...
        [HttpGet]
        public async Task<IActionResult> Update(int? id)
        {
            if (id == null)
            {
                var firstAbout = await _context.About.FirstOrDefaultAsync();
                if (firstAbout == null)
                {
                    _logger.LogWarning("Update GET: No About section found to update, redirecting to Create.");
                    TempData["InfoMessage"] = "Redaktə ediləcək 'Haqqımızda' məlumatı tapılmadı. Zəhmət olmasa, birini yaradın.";
                    return RedirectToAction(nameof(Create));
                }
                id = firstAbout.Id;
            }

            var modelFromDb = await _context.About.FindAsync(id.Value);
            if (modelFromDb == null)
            {
                _logger.LogWarning("Update GET: About section with ID {Id} not found.", id);
                return NotFound();
            }
            return View(modelFromDb);
        }


        // UPDATE - POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(int id, About model)
        {
            if (id != model.Id)
            {
                return BadRequest();
            }

            if (ModelState.IsValid)
            {
                var existingAbout = await _context.About.FindAsync(model.Id);
                if (existingAbout == null) return NotFound();

                string? oldImagePath = existingAbout.ImagePath;

                if (model.Photo != null && model.Photo.Length > 0)
                {
                    try
                    {
                        // DƏYİŞİKLİK: Qovluq yolu
                        string folderPath = Path.Combine(_env.WebRootPath, ImageUploadDirectory);
                        if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

                        string fileName = Guid.NewGuid().ToString() + Path.GetExtension(model.Photo.FileName);
                        string filePath = Path.Combine(folderPath, fileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await model.Photo.CopyToAsync(stream);
                        }
                        // DƏYİŞİKLİK: Verilənlər bazasına yazılan yol
                        existingAbout.ImagePath = Path.Combine(ImageUploadDirectory, fileName).Replace("\\", "/");
                        // existingAbout.ImagePath = $"{ImageUploadDirectory}/{fileName}";
                        _logger.LogInformation("Image updated for About section ID {Id}: {ImagePath}", existingAbout.Id, existingAbout.ImagePath);

                        if (!string.IsNullOrEmpty(oldImagePath) && oldImagePath != existingAbout.ImagePath && !oldImagePath.Contains("default-about-image.png")) // default şəkilləri silmə
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
                        _logger.LogError(ex, "Error uploading image for About section ID {Id}.", existingAbout.Id);
                        ModelState.AddModelError("Photo", "Şəkil yüklənərkən xəta baş verdi.");
                        return View(existingAbout);
                    }
                }

                existingAbout.TitleTag = model.TitleTag;
                existingAbout.MainTitle = model.MainTitle;
                existingAbout.Description = model.Description;
                existingAbout.Feature1Text = model.Feature1Text;
                existingAbout.Feature2Text = model.Feature2Text;
                existingAbout.Feature3Text = model.Feature3Text;
                existingAbout.Feature4Text = model.Feature4Text;
                existingAbout.ButtonText = model.ButtonText;
                existingAbout.ButtonUrl = model.ButtonUrl;

                try
                {
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("About section ID {Id} updated successfully.", existingAbout.Id);
                    TempData["SuccessMessage"] = "'Haqqımızda' məlumatı uğurla yeniləndi.";
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    // ... (xəta idarəetməsi eyni qalır)
                }
                return RedirectToAction(nameof(Index));
            }
            var aboutFromDbForError = await _context.About.AsNoTracking().FirstOrDefaultAsync(a => a.Id == model.Id);
            if (aboutFromDbForError != null && string.IsNullOrEmpty(model.ImagePath))
            {
                model.ImagePath = aboutFromDbForError.ImagePath;
            }
            return View(model);
        }

        // DELETE - GET metodu eyni qalır...
        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var modelToDelete = await _context.About.FirstOrDefaultAsync(x => x.Id == id);
            if (modelToDelete == null) return NotFound();
            return View(modelToDelete);
        }

        // DELETE - POST
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var modelToDelete = await _context.About.FindAsync(id);
            if (modelToDelete == null) return NotFound();

            try
            {
                if (!string.IsNullOrEmpty(modelToDelete.ImagePath) && !modelToDelete.ImagePath.Contains("default-about-image.png"))
                {
                    // DƏYİŞİKLİK: Köhnə şəkilin tam yolu düzgün alınır
                    string fullPath = Path.Combine(_env.WebRootPath, modelToDelete.ImagePath.TrimStart('/'));
                    if (System.IO.File.Exists(fullPath))
                    {
                        System.IO.File.Delete(fullPath);
                        _logger.LogInformation("Image deleted for About section ID {Id}: {ImagePath}", id, fullPath);
                    }
                }

                _context.About.Remove(modelToDelete);
                await _context.SaveChangesAsync();
                _logger.LogInformation("About section ID {Id} deleted successfully.", id);
                TempData["SuccessMessage"] = "'Haqqımızda' məlumatı uğurla silindi.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting About section ID {Id}", id);
                TempData["ErrorMessage"] = "Silmə zamanı xəta baş verdi.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}