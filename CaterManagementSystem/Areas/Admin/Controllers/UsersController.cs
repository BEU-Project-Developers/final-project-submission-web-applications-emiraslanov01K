// CaterManagementSystem.Areas.Admin.Controllers.UsersController.cs
using Microsoft.AspNetCore.Mvc;
using CaterManagementSystem.Models;
using System.Threading.Tasks;
using CaterManagementSystem.Data;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using CaterManagementSystem.Areas.Admin.ViewModels; // ViewModellər üçün
using System.Collections.Generic; // List üçün
using Microsoft.AspNetCore.Mvc.Rendering; // SelectList üçün
using System.Security.Claims; // Hazırkı admini yoxlamaq üçün

namespace CaterManagementSystem.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class UsersController : Controller
    {
        private readonly AppDbContext _context;
        private readonly ILogger<UsersController> _logger;
        // private readonly UserManager<User> _userManager; // Əgər ASP.NET Core Identity istifadə etsəydiniz
        // private readonly RoleManager<Role> _roleManager;   // Hazırda öz User/Role sisteminiz var

        public UsersController(AppDbContext context, ILogger<UsersController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Admin/Users
        public async Task<IActionResult> Index()
        {
            var users = await _context.Users
                .Include(u => u.UserDetails)
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .OrderBy(u => u.UserName)
                .Select(u => new UserViewModel
                {
                    Id = u.Id,
                    UserName = u.UserName,
                    Email = u.Email,
                    FullName = u.FullName,
                    EmailConfirmed = u.EmailConfirmed,
                    RegistrationDate = u.RegistrationDate,
                    ProfilePicturePath = u.UserDetails != null ? u.UserDetails.ImagePath : "default-avatar.png",
                    Roles = u.UserRoles.Select(ur => ur.Role.Name).ToList()
                }).ToListAsync();

            return View(users);
        }

        // GET: Admin/Users/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var user = await _context.Users
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null) return NotFound();

            var allRoles = await _context.Role.OrderBy(r => r.Name).ToListAsync();

            var viewModel = new UserEditViewModel
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                FullName = user.FullName,
                EmailConfirmed = user.EmailConfirmed,
                UserRoles = user.UserRoles.Select(ur => ur.Role.Name).ToList(),
                AllRoles = new SelectList(allRoles, "Name", "Name"), // Dəyər və mətn üçün Role.Name istifadə edirik
                SelectedRoles = user.UserRoles.Select(ur => ur.Role.Name).ToList() // Hazırkı rolları seçili göstərmək üçün
            };

            return View(viewModel);
        }

        // POST: Admin/Users/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UserEditViewModel viewModel)
        {
            if (id != viewModel.Id) return NotFound();

            var userToUpdate = await _context.Users
                .Include(u => u.UserRoles) // Rollları da yükləyirik ki, yeniləyə bilək
                .FirstOrDefaultAsync(u => u.Id == id);

            if (userToUpdate == null) return NotFound();

            // Başqa istifadəçidə eyni UserName və ya Email olmaması üçün yoxlama
            if (await _context.Users.AnyAsync(u => u.UserName == viewModel.UserName && u.Id != id))
            {
                ModelState.AddModelError("UserName", "Bu istifadəçi adı artıq başqası tərəfindən istifadə olunur.");
            }
            if (await _context.Users.AnyAsync(u => u.Email == viewModel.Email && u.Id != id))
            {
                ModelState.AddModelError("Email", "Bu e-poçt ünvanı artıq başqası tərəfindən istifadə olunur.");
            }

            if (ModelState.IsValid)
            {
                userToUpdate.UserName = viewModel.UserName;
                userToUpdate.Email = viewModel.Email;
                userToUpdate.FullName = viewModel.FullName;
                userToUpdate.EmailConfirmed = viewModel.EmailConfirmed;

                // Rollların yenilənməsi
                // Əvvəlcə mövcud bütün rolları silirik (sadə yanaşma)
                userToUpdate.UserRoles.Clear(); // Və ya _context.UserRoles.RemoveRange(userToUpdate.UserRoles);

                if (viewModel.SelectedRoles != null && viewModel.SelectedRoles.Any())
                {
                    var rolesToAdd = await _context.Role
                                                 .Where(r => viewModel.SelectedRoles.Contains(r.Name))
                                                 .ToListAsync();
                    foreach (var role in rolesToAdd)
                    {
                        userToUpdate.UserRoles.Add(new UserRole { RoleId = role.Id });
                    }
                }

                try
                {
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("User ID {UserId} ('{UserName}') updated by admin.", id, userToUpdate.UserName);
                    TempData["SuccessMessage"] = "İstifadəçi məlumatları uğurla yeniləndi.";
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    _logger.LogError(ex, "Concurrency error while updating User ID {UserId}", id);
                    if (!UserExists(userToUpdate.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }

            // Əgər ModelState invalid isə, rolları yenidən yükləyib View-a göndər
            var allRoles = await _context.Role.OrderBy(r => r.Name).ToListAsync();
            viewModel.AllRoles = new SelectList(allRoles, "Name", "Name");
            viewModel.UserRoles = userToUpdate.UserRoles.Select(ur => ur.Role.Name).ToList(); // Orijinal rolları saxla
            return View(viewModel);
        }


        // GET: Admin/Users/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var user = await _context.Users
                .Include(u => u.UserDetails)
                .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (user == null) return NotFound();

            // Hazırda aktiv olan adminin özünü silməsinə icazə vermə
            var currentAdminId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (user.Id.ToString() == currentAdminId)
            {
                TempData["ErrorMessage"] = "Siz öz hesabınızı silə bilməzsiniz.";
                return RedirectToAction(nameof(Index));
            }

            var viewModel = new UserViewModel // Delete View-u üçün UserViewModel istifadə edə bilərik
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                FullName = user.FullName,
                EmailConfirmed = user.EmailConfirmed,
                RegistrationDate = user.RegistrationDate,
                ProfilePicturePath = user.UserDetails?.ImagePath,
                Roles = user.UserRoles.Select(ur => ur.Role.Name).ToList()
            };
            return View(viewModel);
        }

        // POST: Admin/Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userToDelete = await _context.Users
                                        .Include(u => u.UserDetails) // Əlaqəli UserDetails-i də silmək üçün
                                        .Include(u => u.UserRoles)     // Əlaqəli UserRoles-ları da silmək üçün
                                        .FirstOrDefaultAsync(u => u.Id == id);

            if (userToDelete == null) return NotFound();

            // Hazırda aktiv olan adminin özünü silməsinə icazə vermə
            var currentAdminId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userToDelete.Id.ToString() == currentAdminId)
            {
                TempData["ErrorMessage"] = "Siz öz hesabınızı silə bilməzsiniz.";
                _logger.LogWarning("Admin user {AdminId} attempted to delete their own account.", currentAdminId);
                return RedirectToAction(nameof(Index));
            }

            try
            {
                // Əgər UserDetails varsa, onu da sil (əgər Cascade Delete yoxdursa)
                if (userToDelete.UserDetails != null)
                {
                    _context.UserDetails.Remove(userToDelete.UserDetails);
                }
                // İstifadəçinin rollarını sil (UserRoles cədvəlindən)
                // _context.UserRoles.RemoveRange(userToDelete.UserRoles); // Bu, artıq User-in özündədir

                _context.Users.Remove(userToDelete); // Bu, UserRoles-ı da silməlidir (əgər əlaqə düzgündürsə)
                await _context.SaveChangesAsync();
                _logger.LogInformation("User ID {UserId} ('{UserName}') deleted by admin.", id, userToDelete.UserName);
                TempData["SuccessMessage"] = "İstifadəçi uğurla silindi.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting User ID {UserId}", id);
                TempData["ErrorMessage"] = "İstifadəçi silinərkən xəta baş verdi.";
            }
            return RedirectToAction(nameof(Index));
        }

        private bool UserExists(int id)
        {
            return _context.Users.Any(e => e.Id == id);
        }
    }
}