// CaterManagementSystem.Areas.Admin.Controllers.ServicesController.cs
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
    [Authorize(Roles = "Admin")] // Yalnız adminlər daxil ola bilsin
    public class ServicesController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<ServicesController> _logger;
        private const string ImageUploadPath = "uploads/service_images";

        public ServicesController(AppDbContext context, IWebHostEnvironment env, ILogger<ServicesController> logger)
        {
            _context = context;
            _env = env;
            _logger = logger;
        }

        // GET: Admin/Services
        public async Task<IActionResult> Index()
        {
            // ServiceDescription-ı da yükləyirik ki, Index səhifəsində göstərə bilək (lazım olarsa)
            var services = await _context.Services.Include(s => s.ServiceDescription).ToListAsync();
            return View(services);
        }

        // GET: Admin/Services/Details/5 (Opsional, Index-də kifayət qədər məlumat varsa)
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var service = await _context.Services
                .Include(s => s.ServiceDescription)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (service == null) return NotFound();

            return View(service);
        }

        // GET: Admin/Services/Create
        public IActionResult Create()
        {
            // Create zamanı həm Service, həm də ServiceDescription üçün məlumat alacağıq
            // Bunun üçün bir ViewModel istifadə etmək daha yaxşıdır.
            // Sadəlik üçün hələlik Service modelini istifadə edirik, amma ServiceDescription sahələri üçün
            // View-da ayrıca inputlar olacaq və onları ayrıca emal edəcəyik.
            // Daha yaxşı yanaşma: ServiceWithDescriptionViewModel yaratmaq.
            return View(new Service());
        }

        // POST: Admin/Services/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,Description,ButtonText")] Service service, // Yalnız Service-in öz propertiləri
                                                IFormFile? Photo, // Şəkil üçün ayrıca parametr
                                                                  // ServiceDescription üçün parametrlər
                                                string sd_Title, int sd_GuestCount, int sd_PerPersonPay, string sd_DateWithMonths)
        {
            // ServiceDescription üçün validasiya (sadə)
            if (string.IsNullOrWhiteSpace(sd_Title) || sd_GuestCount <= 0 || sd_PerPersonPay <= 0 || string.IsNullOrWhiteSpace(sd_DateWithMonths))
            {
                ModelState.AddModelError("ServiceDescription", "Xidmət detalları tam doldurulmalıdır.");
            }

            // Modelin özü ServiceDescription propertisi olmadan gəlir
            // service.ServiceDescription null olacaq

            if (ModelState.IsValid)
            {
                if (Photo != null && Photo.Length > 0)
                {
                    try
                    {
                        string folderPath = Path.Combine(_env.WebRootPath, ImageUploadPath);
                        Directory.CreateDirectory(folderPath); // Əgər yoxdursa yaradır

                        string fileName = Guid.NewGuid().ToString() + Path.GetExtension(Photo.FileName);
                        string filePath = Path.Combine(folderPath, fileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await Photo.CopyToAsync(stream);
                        }
                        service.ImagePath = Path.Combine(ImageUploadPath, fileName).Replace("\\", "/");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error uploading image for new Service.");
                        ModelState.AddModelError("Photo", "Şəkil yüklənərkən xəta baş verdi.");
                        return View(service); // Service-i geri qaytarırıq
                    }
                }

                _context.Services.Add(service);
                await _context.SaveChangesAsync(); // Service-i yadda saxlayırıq ki, ServiceId alınsın

                // İndi ServiceDescription-ı yaradıb ServiceId ilə bağlayırıq
                var serviceDescription = new ServiceDescription
                {
                    Title = sd_Title,
                    GuestCount = sd_GuestCount,
                    PerPersonPay = sd_PerPersonPay,
                    DateWithMonths = sd_DateWithMonths,
                    ServiceId = service.Id // Yeni yaradılmış Service-in ID-si
                };
                _context.ServiceDescriptions.Add(serviceDescription);
                await _context.SaveChangesAsync(); // ServiceDescription-ı yadda saxlayırıq

                _logger.LogInformation("New Service with ID {ServiceId} and its description created.", service.Id);
                TempData["SuccessMessage"] = "Yeni xidmət uğurla yaradıldı.";
                return RedirectToAction(nameof(Index));
            }
            // Əgər ModelState.IsValid false isə, service obyektini View-a geri qaytarırıq
            // ServiceDescription məlumatlarını da ViewBag və ya ViewData ilə geri qaytarmaq lazım gələ bilər
            ViewData["sd_Title"] = sd_Title;
            ViewData["sd_GuestCount"] = sd_GuestCount;
            ViewData["sd_PerPersonPay"] = sd_PerPersonPay;
            ViewData["sd_DateWithMonths"] = sd_DateWithMonths;
            return View(service);
        }

        // GET: Admin/Services/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var service = await _context.Services
                                        .Include(s => s.ServiceDescription) // ServiceDescription-ı yükləyirik
                                        .FirstOrDefaultAsync(s => s.Id == id);
            if (service == null) return NotFound();

            // View-a göndərmək üçün ViewModel yaratmaq daha yaxşıdır,
            // çünki həm Service, həm də ServiceDescription məlumatlarını daşımalıdır.
            // Sadəlik üçün Service modelini göndəririk, View-da ServiceDescription-a çata biləcəyik.
            return View(service);
        }

        // POST: Admin/Services/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id,
                                              [Bind("Id,Title,Description,ButtonText,ImagePath")] Service service, // ImagePath-i də bind edirik
                                              IFormFile? Photo,
                                              // ServiceDescription üçün parametrlər
                                              int sd_Id, string sd_Title, int sd_GuestCount, int sd_PerPersonPay, string sd_DateWithMonths)
        {
            if (id != service.Id) return NotFound();

            // ServiceDescription üçün validasiya
            if (string.IsNullOrWhiteSpace(sd_Title) || sd_GuestCount <= 0 || sd_PerPersonPay <= 0 || string.IsNullOrWhiteSpace(sd_DateWithMonths))
            {
                ModelState.AddModelError("ServiceDescription", "Xidmət detalları tam doldurulmalıdır.");
            }


            if (ModelState.IsValid)
            {
                var serviceToUpdate = await _context.Services.FindAsync(id);
                if (serviceToUpdate == null) return NotFound();

                string? oldImagePath = serviceToUpdate.ImagePath;

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
                        serviceToUpdate.ImagePath = Path.Combine(ImageUploadPath, fileName).Replace("\\", "/");

                        // Köhnə şəkli sil
                        if (!string.IsNullOrEmpty(oldImagePath) && oldImagePath != serviceToUpdate.ImagePath)
                        {
                            string fullOldPath = Path.Combine(_env.WebRootPath, oldImagePath.TrimStart('/'));
                            if (System.IO.File.Exists(fullOldPath))
                            {
                                System.IO.File.Delete(fullOldPath);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error uploading image for Service ID {ServiceId}.", id);
                        ModelState.AddModelError("Photo", "Şəkil yüklənərkən xəta baş verdi.");
                        // ServiceDescription-ı da View-a geri göndərmək üçün modeli yükləmək lazımdır
                        service.ServiceDescription = await _context.ServiceDescriptions.FirstOrDefaultAsync(sd => sd.ServiceId == id);
                        return View(service);
                    }
                }
                else
                {
                    // Əgər yeni şəkil yüklənməyibsə, köhnə ImagePath-i qoru (hidden inputdan gələn)
                    serviceToUpdate.ImagePath = service.ImagePath;
                }

                serviceToUpdate.Title = service.Title;
                serviceToUpdate.Description = service.Description;
                serviceToUpdate.ButtonText = service.ButtonText;

                // ServiceDescription-ı tap və yenilə
                var serviceDescriptionToUpdate = await _context.ServiceDescriptions.FirstOrDefaultAsync(sd => sd.ServiceId == id);
                if (serviceDescriptionToUpdate != null)
                {
                    if (serviceDescriptionToUpdate.Id != sd_Id && sd_Id != 0) // sd_Id 0 gələ bilər əgər View-da düzgün set edilməyibsə
                    {
                        _logger.LogWarning("ServiceDescription ID mismatch during update for Service ID {ServiceId}.", id);
                        // Xəta mesajı və ya başqa bir處理
                    }
                    serviceDescriptionToUpdate.Title = sd_Title;
                    serviceDescriptionToUpdate.GuestCount = sd_GuestCount;
                    serviceDescriptionToUpdate.PerPersonPay = sd_PerPersonPay;
                    serviceDescriptionToUpdate.DateWithMonths = sd_DateWithMonths;
                    // _context.Update(serviceDescriptionToUpdate); // Artıq izlənilir
                }
                else if (!string.IsNullOrWhiteSpace(sd_Title)) // Əgər əvvəl yox idisə, amma indi məlumat varsa, yeni yarat
                {
                    var newServiceDescription = new ServiceDescription
                    {
                        Title = sd_Title,
                        GuestCount = sd_GuestCount,
                        PerPersonPay = sd_PerPersonPay,
                        DateWithMonths = sd_DateWithMonths,
                        ServiceId = serviceToUpdate.Id
                    };
                    _context.ServiceDescriptions.Add(newServiceDescription);
                }


                try
                {
                    // _context.Update(serviceToUpdate); // Artıq izlənilir
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Service ID {ServiceId} and its description updated.", id);
                    TempData["SuccessMessage"] = "Xidmət uğurla yeniləndi.";
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    _logger.LogError(ex, "Concurrency error while updating Service ID {ServiceId}", id);
                    if (!ServiceExists(service.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            // ModelState invalid olarsa
            // ServiceDescription-ı da View-a geri göndərmək üçün modeli yükləmək lazımdır
            if (service.ServiceDescription == null && id > 0)
            {
                service.ServiceDescription = await _context.ServiceDescriptions.FirstOrDefaultAsync(sd => sd.ServiceId == id);
            }
            // View-a göndərmədən əvvəl ServiceDescription inputlarını da ViewData ilə ötürmək daha etibarlı ola bilər
            ViewData["sd_Id"] = sd_Id;
            ViewData["sd_Title"] = sd_Title;
            ViewData["sd_GuestCount"] = sd_GuestCount;
            ViewData["sd_PerPersonPay"] = sd_PerPersonPay;
            ViewData["sd_DateWithMonths"] = sd_DateWithMonths;
            return View(service);
        }

        // GET: Admin/Services/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var service = await _context.Services
                .Include(s => s.ServiceDescription) // Silmədən əvvəl əlaqəli məlumatı göstərmək üçün
                .FirstOrDefaultAsync(m => m.Id == id);
            if (service == null) return NotFound();

            return View(service);
        }

        // POST: Admin/Services/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var service = await _context.Services.FindAsync(id);
            if (service == null) return NotFound();

            try
            {
                // Əvvəlcə əlaqəli ServiceDescription-ı sil (əgər varsa və Cascade Delete konfiqurasiya edilməyibsə)
                var serviceDescription = await _context.ServiceDescriptions.FirstOrDefaultAsync(sd => sd.ServiceId == id);
                if (serviceDescription != null)
                {
                    _context.ServiceDescriptions.Remove(serviceDescription);
                }

                // Şəkli sil
                if (!string.IsNullOrEmpty(service.ImagePath))
                {
                    string fullPath = Path.Combine(_env.WebRootPath, service.ImagePath.TrimStart('/'));
                    if (System.IO.File.Exists(fullPath))
                    {
                        System.IO.File.Delete(fullPath);
                    }
                }

                _context.Services.Remove(service);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Service ID {ServiceId} and its description deleted.", id);
                TempData["SuccessMessage"] = "Xidmət uğurla silindi.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting Service ID {ServiceId}", id);
                TempData["ErrorMessage"] = "Xidmət silinərkən xəta baş verdi.";
            }
            return RedirectToAction(nameof(Index));
        }

        private bool ServiceExists(int id)
        {
            return _context.Services.Any(e => e.Id == id);
        }
    }
}