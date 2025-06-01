// Areas/Admin/Controllers/BookingsController.cs
using CaterManagementSystem.Data;
using CaterManagementSystem.Models;
using CaterManagementSystem.ViewModels;      // BookingSummaryViewModel üçün
using CaterManagementSystem.Models.Enums;    // BookingStatus enum üçün
using CaterManagementSystem.Helpers;         // EnumExtensions üçün
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using System;
using CaterManagementSystem.Services;
using CaterManagementSystem.Areas.Admin.ViewModels;
// using CaterManagementSystem.Areas.Admin.ViewModels; // AdminBookingEditViewModel üçün (əgər ayrıca fayldadırsa)

namespace CaterManagementSystem.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class BookingsController : Controller
    {
        private readonly AppDbContext _context;
        private readonly ILogger<BookingsController> _logger;
        private readonly IEmailSender _emailSender; // E-poçt göndərmək üçün (əgər lazımdırsa)

        public BookingsController(AppDbContext context, ILogger<BookingsController> logger, IEmailSender emailSender)
        {
            _context = context;
            _logger = logger;
            _emailSender = emailSender; // Təyin et
        }

        // GET: Admin/Bookings
        public async Task<IActionResult> Index(string searchTerm, string statusFilter, string sortOrder, int pageNumber = 1, int pageSize = 10)
        {
            ViewData["CurrentSort"] = sortOrder;
            ViewData["DateSortParm"] = string.IsNullOrEmpty(sortOrder) ? "date_desc" : "";
            ViewData["StatusSortParm"] = sortOrder == "Status" ? "status_desc" : "Status";
            ViewData["UserSortParm"] = sortOrder == "User" ? "user_desc" : "User";
            ViewData["IdSortParm"] = sortOrder == "Id" ? "id_desc" : "Id";
            ViewData["CurrentSearch"] = searchTerm;
            ViewData["CurrentStatus"] = statusFilter;

            var bookingsQuery = _context.Bookings
                                      .Include(b => b.User)
                                      .AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                bookingsQuery = bookingsQuery.Where(b => (b.User != null && (b.User.FullName.Contains(searchTerm) || b.User.Email.Contains(searchTerm))) ||
                                                         (b.BookingIdentifier != null && b.BookingIdentifier.Contains(searchTerm)) ||
                                                         b.ServiceType.Contains(searchTerm) ||
                                                         b.Place.Contains(searchTerm) ||
                                                         b.City.Contains(searchTerm));
            }

            if (!string.IsNullOrEmpty(statusFilter))
            {
                if (Enum.TryParse<BookingStatus>(statusFilter, true, out BookingStatus parsedStatus))
                {
                    bookingsQuery = bookingsQuery.Where(b => b.Status == parsedStatus);
                }
                else
                {
                    _logger.LogWarning("Invalid status filter value received: {StatusFilter}", statusFilter);
                }
            }

            switch (sortOrder)
            {
                case "date_desc":
                    bookingsQuery = bookingsQuery.OrderByDescending(b => b.BookingDate);
                    break;
                case "Status":
                    bookingsQuery = bookingsQuery.OrderBy(b => b.Status);
                    break;
                case "status_desc":
                    bookingsQuery = bookingsQuery.OrderByDescending(b => b.Status);
                    break;
                case "User":
                    bookingsQuery = bookingsQuery.OrderBy(b => b.User.FullName);
                    break;
                case "user_desc":
                    bookingsQuery = bookingsQuery.OrderByDescending(b => b.User.FullName);
                    break;
                case "Id":
                    bookingsQuery = bookingsQuery.OrderBy(b => b.Id);
                    break;
                case "id_desc":
                    bookingsQuery = bookingsQuery.OrderByDescending(b => b.Id);
                    break;
                default:
                    bookingsQuery = bookingsQuery.OrderByDescending(b => b.CreatedAt);
                    break;
            }

            ViewBag.StatusList = new SelectList(
                Enum.GetValues(typeof(BookingStatus)).Cast<BookingStatus>().Select(v => new SelectListItem
                {
                    Text = v.GetDisplayName(), // Extension metod istifadə edirik
                    Value = v.ToString()
                }).ToList(), "Value", "Text", statusFilter);

            var count = await bookingsQuery.CountAsync();
            var items = await bookingsQuery
                                .Skip((pageNumber - 1) * pageSize)
                                .Take(pageSize)
                                .Select(b => new BookingSummaryViewModel
                                {
                                    Id = b.Id,
                                    BookingIdentifier = b.BookingIdentifier ?? $"REZ-{b.Id:D5}",
                                    UserName = b.User != null ? b.User.FullName : "N/A",
                                    UserEmail = b.User != null ? b.User.Email : "N/A",
                                    ServiceType = b.ServiceType,
                                    BookingDate = b.BookingDate,
                                    Place = b.Place,
                                    City = b.City,
                                    Status = b.Status,
                                    CreatedAt = b.CreatedAt
                                })
                                .ToListAsync();

            var paginatedList = new PaginatedList<BookingSummaryViewModel>(items, count, pageNumber, pageSize);
            return View(paginatedList);
        }

        // GET: Admin/Bookings/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var booking = await _context.Bookings
                .Include(b => b.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (booking == null) return NotFound();

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
            ViewBag.BookingStatusEnum = booking.Status; // Enum olaraq göndər
            ViewBag.UserName = booking.User?.FullName;
            ViewBag.UserEmail = booking.User?.Email;
            ViewBag.CreatedAt = booking.CreatedAt;
            ViewBag.UpdatedAt = booking.UpdatedAt;
            return View(viewModel);
        }

        // GET: Admin/Bookings/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var booking = await _context.Bookings.Include(b => b.User).FirstOrDefaultAsync(b => b.Id == id);
            if (booking == null) return NotFound();

            var viewModel = new AdminBookingEditViewModel
            {
                Id = booking.Id,
                BookingIdentifier = booking.BookingIdentifier ?? $"REZ-{booking.Id:D5}",
                UserName = booking.User?.FullName,
                UserEmail = booking.User?.Email,
                ServiceType = booking.ServiceType,
                BookingDate = booking.BookingDate,
                CurrentStatus = booking.Status,
                NewStatus = booking.Status, // İlkin olaraq eyni olsun
                ContactNumber = booking.ContactNumber,
                Notes = booking.Notes
            };

            ViewBag.StatusOptions = new SelectList(
                Enum.GetValues(typeof(BookingStatus)).Cast<BookingStatus>().Select(v => new SelectListItem
                {
                    Text = v.GetDisplayName(), // Extension metod
                    Value = v.ToString() // Enum-un string dəyəri
                }).ToList(), "Value", "Text", viewModel.CurrentStatus.ToString()); // Seçili dəyər string olmalıdır

            return View(viewModel);
        }

        // POST: Admin/Bookings/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AdminBookingEditViewModel model)
        {
            if (id != model.Id) return NotFound();

            var bookingToUpdate = await _context.Bookings.Include(b => b.User).FirstOrDefaultAsync(b => b.Id == id);
            if (bookingToUpdate == null) return NotFound();

            if (ModelState.IsValid)
            {
                bool statusChanged = bookingToUpdate.Status != model.NewStatus;
                string oldStatusDisplayName = bookingToUpdate.Status.GetDisplayName(); // Köhnə statusun adı

                bookingToUpdate.Status = model.NewStatus; // Enum birbaşa təyin edilir
                bookingToUpdate.ContactNumber = model.ContactNumber;
                bookingToUpdate.Notes = model.Notes;
                bookingToUpdate.UpdatedAt = DateTime.UtcNow;

                try
                {
                    _context.Update(bookingToUpdate);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Admin updated booking {BookingId}. Status changed from {OldStatus} to {NewStatus}.", id, oldStatusDisplayName, model.NewStatus.GetDisplayName());

                    if (statusChanged && bookingToUpdate.User != null && !string.IsNullOrEmpty(bookingToUpdate.User.Email))
                    {
                        try
                        {
                            await _emailSender.SendEmailAsync(bookingToUpdate.User.Email,
                                 $"Rezerv Statusu Yeniləndi - #{bookingToUpdate.BookingIdentifier}",
                                 $"Hörmətli {bookingToUpdate.User.FullName}, <br/><br/> #{bookingToUpdate.BookingIdentifier} nömrəli rezervinizin statusu <strong>'{model.NewStatus.GetDisplayName()}'</strong> olaraq dəyişdirildi.<br/><br/>Hörmətlə,<br/>Cater Management System Komandası");
                            _logger.LogInformation("Status update notification email sent to {UserEmail} for booking {BookingId}.", bookingToUpdate.User.Email, id);
                        }
                        catch (Exception emailEx)
                        {
                            _logger.LogError(emailEx, "Failed to send status update notification email for booking {BookingId} to {UserEmail}.", id, bookingToUpdate.User.Email);
                        }
                    }
                    TempData["SuccessMessage"] = "Rezerv məlumatları uğurla yeniləndi.";
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    _logger.LogError(ex, "Concurrency error while admin updating booking {BookingId}", id);
                    if (!BookingExists(bookingToUpdate.Id)) return NotFound();
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Məlumatlar eyni anda başqası tərəfindən dəyişdirilib. Səhifəni yeniləyib təkrar cəhd edin.");
                        // Populate ViewBag.StatusOptions again for the view
                        ViewBag.StatusOptions = new SelectList(
                            Enum.GetValues(typeof(BookingStatus)).Cast<BookingStatus>().Select(v => new SelectListItem
                            {
                                Text = v.GetDisplayName(),
                                Value = v.ToString()
                            }).ToList(), "Value", "Text", model.CurrentStatus.ToString());
                        // Populate other model properties if needed
                        model.UserName = bookingToUpdate.User?.FullName;
                        model.UserEmail = bookingToUpdate.User?.Email;
                        model.ServiceType = bookingToUpdate.ServiceType;
                        model.BookingDate = bookingToUpdate.BookingDate;
                        model.CurrentStatus = bookingToUpdate.Status; // Qayıdanda köhnə statusu göstər
                        return View(model);
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            // ModelState invalid isə
            model.UserName = bookingToUpdate.User?.FullName;
            model.UserEmail = bookingToUpdate.User?.Email;
            model.ServiceType = bookingToUpdate.ServiceType;
            model.BookingDate = bookingToUpdate.BookingDate;
            model.CurrentStatus = bookingToUpdate.Status; // Köhnə statusu saxla
            ViewBag.StatusOptions = new SelectList(
                Enum.GetValues(typeof(BookingStatus)).Cast<BookingStatus>().Select(v => new SelectListItem
                {
                    Text = v.GetDisplayName(),
                    Value = v.ToString()
                }).ToList(), "Value", "Text", model.CurrentStatus.ToString());
            return View(model);
        }

        // GET: Admin/Bookings/Delete/5 (Təsdiqləmə səhifəsi)
        [HttpGet]
        public async Task<IActionResult> Delete(int? id) // Adı Delete olaraq dəyişdirdim, DeleteConfirmation yerinə
        {
            if (id == null) return NotFound();
            var booking = await _context.Bookings.Include(b => b.User).FirstOrDefaultAsync(b => b.Id == id.Value);
            if (booking == null) return NotFound();
            return View(booking); // Views/Admin/Bookings/Delete.cshtml istifadə edəcək
        }

        // POST: Admin/Bookings/Delete/5
        [HttpPost, ActionName("Delete")] // GET ilə eyni adı daşıması üçün
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var booking = await _context.Bookings.FindAsync(id);
            if (booking == null)
            {
                TempData["ErrorMessage"] = "Silinəcək rezerv tapılmadı.";
                return RedirectToAction(nameof(Index));
            }
            try
            {
                _context.Bookings.Remove(booking);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Admin deleted booking {BookingId}.", id);
                TempData["SuccessMessage"] = "Rezerv uğurla silindi.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting booking {BookingId} by admin. Details: {ExceptionDetails}", id, ex.ToString());
                TempData["ErrorMessage"] = "Rezerv silinərkən xəta baş verdi.";
            }
            return RedirectToAction(nameof(Index));
        }

        private bool BookingExists(int id)
        {
            return _context.Bookings.Any(e => e.Id == id);
        }
    }

    public class PaginatedList<T> : List<T>
    {
        public int PageIndex { get; private set; }
        public int TotalPages { get; private set; }

        public PaginatedList(List<T> items, int count, int pageIndex, int pageSize)
        {
            PageIndex = pageIndex;
            TotalPages = (int)Math.Ceiling(count / (double)pageSize);

            this.AddRange(items);
        }

        public bool HasPreviousPage => PageIndex > 1;
        public bool HasNextPage => PageIndex < TotalPages;

        public static async Task<PaginatedList<T>> CreateAsync(IQueryable<T> source, int pageIndex, int pageSize)
        {
            var count = await source.CountAsync();
            var items = await source.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync();
            return new PaginatedList<T>(items, count, pageIndex, pageSize);
        }
    }
}