
using CaterManagementSystem.Data;
using CaterManagementSystem.Models;
using CaterManagementSystem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System;
using CaterManagementSystem.Models.Enums;
using CaterManagementSystem.Services;
using CaterManagementSystem.Helpers; // BookingStatus enum üçün ( qəbul olunub rədd edilib və s üçün ) 
// using CaterManagementSystem.Helpers; // GetDisplayName() _ViewImports-dan gələcək

namespace CaterManagementSystem.Controllers
{
    [Authorize]
    public class BookingController : Controller
    {
        private readonly AppDbContext _context;
        private readonly ILogger<BookingController> _logger;
        private readonly IEmailSender _emailSender; // İstifadəçiyə bildiriş üçün ( hesaba daxil olundu ) 

        public BookingController(AppDbContext context, ILogger<BookingController> logger, IEmailSender emailSender)
        {
            _context = context;
            _logger = logger;
            _emailSender = emailSender;
        }

        // GET: /Booking/MyBookings  
        public async Task<IActionResult> MyBookings()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
            {
                _logger.LogWarning("MyBookings: User ID claim not found or invalid.");
                return Challenge();
            }

            var bookings = await _context.Bookings
                .Where(b => b.UserId == userId)
                .OrderByDescending(b => b.CreatedAt)
                .Select(b => new BookingSummaryViewModel
                {
                    Id = b.Id,
                    BookingIdentifier = b.BookingIdentifier ?? $"REZ-{b.Id:D5}",
                    ServiceType = b.ServiceType,
                    BookingDate = b.BookingDate,
                    Place = b.Place,
                    City = b.City,
                    Status = b.Status, // Enum dəyəri
                    // BookingSummaryViewModel-də UserName və UserEmail yoxdur, çünki bu istifadəçinin öz rezervləridir
                    // Əgər lazımdırsa, əlavə edilə bilər, amma adətən MyBookings üçün gərək olmur.
                    CreatedAt = b.CreatedAt
                })
                .ToListAsync();

            var viewModel = new MyBookingsViewModel { Bookings = bookings };
            return View(viewModel);
        }

        // GET: /Booking/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int currentUserId))
            {
                return Challenge();
            }

            var booking = await _context.Bookings
                                        .Include(b => b.User) 
                                        .FirstOrDefaultAsync(b => b.Id == id && b.UserId == currentUserId);

            if (booking == null)
            {
                TempData["ErrorMessage"] = "Rezerv tapılmadı və ya bu rezervə baxmaq üçün icazəniz yoxdur.";
                return RedirectToAction(nameof(MyBookings));
            }

            var viewModel = new BookingViewModel
            {
                Id = booking.Id,
                Country = booking.Country,
                City = booking.City,
                Place = booking.Place,
                BookingIdentifier = booking.BookingIdentifier,
                NumberOfPalacesRange = booking.NumberOfPalacesRange,
                ServiceType = booking.ServiceType,
                ContactNumber = booking.ContactNumber,
                BookingDate = booking.BookingDate,
                Email = booking.Email,
                Notes = booking.Notes
            };

            ViewBag.BookingEntityStatus = booking.Status; // Enum dəyərini ötürmə
            ViewBag.CreatedAt = booking.CreatedAt;
            ViewBag.UpdatedAt = booking.UpdatedAt;

            return View(viewModel);
        }

        // GET: /Booking/Create
        public IActionResult Create()
        {
            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            var viewModel = new BookingViewModel
            {
                Email = userEmail ?? string.Empty,
                BookingDate = DateTime.Today.AddDays(1) // Default olaraq sabah götürülür 
            };
          
            return View(viewModel);
        }

        // POST: /Booking/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BookingViewModel model)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
            {
                ModelState.AddModelError("", "İstifadəçi sessiyası etibarsızdır.");
                // PopulateSelectListsIfNecessary(model);
                return View(model);
            }

            if (model.BookingDate < DateTime.Today)
            {
                ModelState.AddModelError(nameof(model.BookingDate), "Rezerv tarixi keçmiş bir tarix ola bilməz.");
            }

            if (ModelState.IsValid)
            {
                var booking = new Booking
                {
                    UserId = userId,
                    Country = model.Country,
                    City = model.City,
                    Place = model.Place,
                    NumberOfPalacesRange = model.NumberOfPalacesRange,
                    ServiceType = model.ServiceType,
                    ContactNumber = model.ContactNumber,
                    BookingDate = model.BookingDate,
                    Email = model.Email, 
                    Notes = model.Notes,
                    Status = BookingStatus.Pending, 
                    CreatedAt = DateTime.UtcNow
                    // BookingIdentifier avtomatik creeate olunur 
                };

                _context.Bookings.Add(booking);
                await _context.SaveChangesAsync();

                // Id alındıqdan sonra BookingIdentifier yaradılır
                booking.BookingIdentifier = model.BookingIdentifier ?? $"REZ-{booking.Id:D5}";
                _context.Update(booking);
                await _context.SaveChangesAsync();

                _logger.LogInformation("User {UserId} created a new booking {BookingId} with Identifier {BookingIdentifier}", userId, booking.Id, booking.BookingIdentifier);
                TempData["SuccessMessage"] = $"Rezerviniz (Kod: {booking.BookingIdentifier}) uğurla yaradıldı və nəzərdən keçirilməsi üçün göndərildi.";

          
                return RedirectToAction(nameof(MyBookings));
            }
            // PopulateSelectListsIfNecessary(model);
            return View(model);
        }

        // GET: /Booking/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int currentUserId))
            {
                return Challenge();
            }

            var booking = await _context.Bookings.FirstOrDefaultAsync(b => b.Id == id && b.UserId == currentUserId);
            if (booking == null)
            {
                TempData["ErrorMessage"] = "Rezerv tapılmadı və ya bu rezervi redaktə etmək üçün icazəniz yoxdur.";
                return RedirectToAction(nameof(MyBookings));
            }

            if (booking.Status != BookingStatus.Pending)
            {
                TempData["ErrorMessage"] = $"Bu rezervin statusu '{booking.Status.GetDisplayName()}' olduğu üçün redaktə edilə bilməz.";
                return RedirectToAction(nameof(MyBookings));
            }

            var viewModel = new BookingViewModel
            {
                Id = booking.Id,
                Country = booking.Country,
                City = booking.City,
                Place = booking.Place,
                BookingIdentifier = booking.BookingIdentifier,
                NumberOfPalacesRange = booking.NumberOfPalacesRange,
                ServiceType = booking.ServiceType,
                ContactNumber = booking.ContactNumber,
                BookingDate = booking.BookingDate,
                Email = booking.Email,
                Notes = booking.Notes
            };
            // PopulateSelectListsIfNecessary(viewModel);
            return View(viewModel);
        }

        // POST: /Booking/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, BookingViewModel model)
        {
            if (id != model.Id) return NotFound();

            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int currentUserId))
            {
                ModelState.AddModelError("", "İstifadəçi sessiyası etibarsızdır.");
                // PopulateSelectListsIfNecessary(model);
                return View(model);
            }

            var bookingToUpdateFromDb = await _context.Bookings
                                            .AsNoTracking()
                                            .FirstOrDefaultAsync(b => b.Id == id && b.UserId == currentUserId);
            if (bookingToUpdateFromDb == null)
            {
                TempData["ErrorMessage"] = "Yenilənəcək rezerv tapılmadı.";
                return RedirectToAction(nameof(MyBookings));
            }

           
            if (model.BookingIdentifier != bookingToUpdateFromDb.BookingIdentifier)
            {
                ModelState.AddModelError(nameof(model.BookingIdentifier), "Rezerv İdentifikatoru dəyişdirilə bilməz.");
                model.BookingIdentifier = bookingToUpdateFromDb.BookingIdentifier; 
            }
            if (model.Email != bookingToUpdateFromDb.Email)
            {
                ModelState.AddModelError(nameof(model.Email), "Rezerv üçün əlaqə e-poçtu dəyişdirilə bilməz.");
                model.Email = bookingToUpdateFromDb.Email; 
            }
            if (model.BookingDate < DateTime.Today)
            {
                ModelState.AddModelError(nameof(model.BookingDate), "Rezerv tarixi keçmiş bir tarix ola bilməz.");
            }

            if (ModelState.IsValid)
            {
                var bookingEntityToUpdate = await _context.Bookings.FirstOrDefaultAsync(b => b.Id == id && b.UserId == currentUserId);
                if (bookingEntityToUpdate == null) return NotFound();

                if (bookingEntityToUpdate.Status != BookingStatus.Pending)
                {
                    TempData["ErrorMessage"] = $"Bu rezervin statusu '{bookingEntityToUpdate.Status.GetDisplayName()}' olduğu üçün yenilənə bilməz.";
                    return RedirectToAction(nameof(MyBookings));
                }

                try
                {
                    bookingEntityToUpdate.Country = model.Country;
                    bookingEntityToUpdate.City = model.City;
                    bookingEntityToUpdate.Place = model.Place;
                    bookingEntityToUpdate.NumberOfPalacesRange = model.NumberOfPalacesRange;
                    bookingEntityToUpdate.ServiceType = model.ServiceType;
                    bookingEntityToUpdate.ContactNumber = model.ContactNumber;
                    bookingEntityToUpdate.BookingDate = model.BookingDate;
                    bookingEntityToUpdate.Notes = model.Notes;
                    bookingEntityToUpdate.UpdatedAt = DateTime.UtcNow;
                    

                    await _context.SaveChangesAsync();
                    _logger.LogInformation("User {UserId} updated their booking {BookingId}", currentUserId, id);
                    TempData["SuccessMessage"] = "Rezerviniz uğurla yeniləndi.";
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    _logger.LogError(ex, "Concurrency error while user updating booking {BookingId}", id);
                    if (!BookingExists(id)) return NotFound();
                    else ModelState.AddModelError(string.Empty, "Məlumatlar eyni anda başqası tərəfindən dəyişdirilib. Səhifəni yeniləyib təkrar cəhd edin.");
                }
                return RedirectToAction(nameof(MyBookings));
            }
            // PopulateSelectListsIfNecessary(model);
            return View(model);
        }

        // POST: /Booking/Delete/5 (İstifadəçi öz rezervini "Ləğv Edilmiş" statusuna keçirir)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int currentUserId))
            {
                TempData["ErrorMessage"] = "İstifadəçi tapılmadı.";
                return RedirectToAction(nameof(MyBookings));
            }

            var booking = await _context.Bookings.Include(b => b.User).FirstOrDefaultAsync(b => b.Id == id && b.UserId == currentUserId);
            if (booking == null)
            {
                TempData["ErrorMessage"] = "Rezerv tapılmadı və ya bu rezervi ləğv etmək üçün icazəniz yoxdur.";
                return RedirectToAction(nameof(MyBookings));
            }

            if (booking.Status != BookingStatus.Pending)
            {
                TempData["ErrorMessage"] = $"Bu rezervin statusu '{booking.Status.GetDisplayName()}' olduğu üçün ləğv edilə bilməz. Zəhmət olmasa dəstək ilə əlaqə saxlayın.";
                return RedirectToAction(nameof(MyBookings));
            }

            try
            {
                booking.Status = BookingStatus.Cancelled; // Statusu Ləğv Edilmişə dəyiş
                booking.UpdatedAt = DateTime.UtcNow;
                // _context.Bookings.Remove(booking); // Tamamilə silmirik
                _context.Update(booking);
                await _context.SaveChangesAsync();
                _logger.LogInformation("User {UserId} cancelled their booking {BookingId}", currentUserId, id);
                TempData["SuccessMessage"] = "Rezerviniz uğurla ləğv edildi.";

                // Adminə bildiriş göndər (opsional)
                // try {
                //    await _emailSender.SendEmailAsync("admin@example.com", $"Rezerv Ləğv Edildi: {booking.BookingIdentifier}", $"İstifadəçi {booking.User?.FullName} #{booking.BookingIdentifier} nömrəli rezervini ləğv etdi.");
                // } catch (Exception emailEx) { _logger.LogError(emailEx, "Failed to send booking cancellation notification to admin."); }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling booking {BookingId} by user {UserId}", id, currentUserId);
                TempData["ErrorMessage"] = "Rezerv ləğv edilərkən xəta baş verdi.";
            }
            return RedirectToAction(nameof(MyBookings));
        }

        private bool BookingExists(int id)
        {
            return _context.Bookings.Any(e => e.Id == id);
        }
    }
}