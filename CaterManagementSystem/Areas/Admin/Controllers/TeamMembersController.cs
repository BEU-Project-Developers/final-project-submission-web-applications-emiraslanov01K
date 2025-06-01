// CaterManagementSystem.Areas.Admin.Controllers.TeamMembersController.cs
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
using Microsoft.AspNetCore.Mvc.Rendering; // SelectList üçün

namespace CaterManagementSystem.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class TeamMembersController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<TeamMembersController> _logger;
        private const string ImageUploadPath = "uploads/team_images"; // Şəkillərin yüklənəcəyi qovluq

        public TeamMembersController(AppDbContext context, IWebHostEnvironment env, ILogger<TeamMembersController> logger)
        {
            _context = context;
            _env = env;
            _logger = logger;
        }

        // GET: Admin/TeamMembers
        public async Task<IActionResult> Index()
        {
            var teamMembers = await _context.TeamMembers
                                            .Include(t => t.Profession) // Peşə məlumatını da yüklə
                                            .OrderBy(t => t.Name)
                                            .ToListAsync();
            return View(teamMembers);
        }

        // GET: Admin/TeamMembers/Create
        public async Task<IActionResult> Create()
        {
            await PopulateProfessionsDropDownList();
            return View(new TeamMember()); // Boş model göndəririk
        }

        // POST: Admin/TeamMembers/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,ProfessionId")] TeamMember teamMember, IFormFile? Photo)
        {
            if (teamMember.ProfessionId == 0) // Və ya başqa bir yoxlama ki, peşə seçilib
            {
                ModelState.AddModelError("ProfessionId", "Peşə seçilməlidir.");
            }
            // Eyni adda komanda üzvü olub olmadığını yoxlaya bilərik (opsional)
            if (await _context.TeamMembers.AnyAsync(t => t.Name.ToLower() == teamMember.Name.ToLower()))
            {
                ModelState.AddModelError("Name", "Bu adda komanda üzvü artıq mövcuddur.");
            }

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
                        teamMember.ImagePath = Path.Combine(ImageUploadPath, fileName).Replace("\\", "/");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error uploading image for new Team Member.");
                        ModelState.AddModelError("Photo", "Şəkil yüklənərkən xəta baş verdi.");
                        await PopulateProfessionsDropDownList(teamMember.ProfessionId);
                        return View(teamMember);
                    }
                }
                else
                {
                    // Şəkil tələb olunursa və ya default şəkil təyin etmək istəyirsinizsə
                    teamMember.ImagePath = Path.Combine(ImageUploadPath, "default-member-avatar.png").Replace("\\", "/"); // Varsayılan şəkil
                    // Və ya şəkil məcburidirsə:
                    // ModelState.AddModelError("Photo", "Komanda üzvü üçün şəkil tələb olunur.");
                }

                _context.Add(teamMember);
                await _context.SaveChangesAsync();
                _logger.LogInformation("New Team Member '{MemberName}' created with ID {MemberId}.", teamMember.Name, teamMember.Id);
                TempData["SuccessMessage"] = "Yeni komanda üzvü uğurla yaradıldı.";
                return RedirectToAction(nameof(Index));
            }
            await PopulateProfessionsDropDownList(teamMember.ProfessionId);
            return View(teamMember);
        }

        // GET: Admin/TeamMembers/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var teamMember = await _context.TeamMembers.FindAsync(id);
            if (teamMember == null) return NotFound();

            await PopulateProfessionsDropDownList(teamMember.ProfessionId);
            return View(teamMember);
        }

        // POST: Admin/TeamMembers/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,ProfessionId,ImagePath")] TeamMember teamMember, IFormFile? Photo)
        {
            if (id != teamMember.Id) return NotFound();

            if (teamMember.ProfessionId == 0)
            {
                ModelState.AddModelError("ProfessionId", "Peşə seçilməlidir.");
            }
            // Eyni adda başqa komanda üzvü olub olmadığını yoxla (hazırkı ID xaric)
            if (await _context.TeamMembers.AnyAsync(t => t.Name.ToLower() == teamMember.Name.ToLower() && t.Id != teamMember.Id))
            {
                ModelState.AddModelError("Name", "Bu adda başqa bir komanda üzvü artıq mövcuddur.");
            }


            if (ModelState.IsValid)
            {
                var memberToUpdate = await _context.TeamMembers.FindAsync(id);
                if (memberToUpdate == null) return NotFound();

                string? oldImagePath = memberToUpdate.ImagePath;

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
                        memberToUpdate.ImagePath = Path.Combine(ImageUploadPath, fileName).Replace("\\", "/");

                        // Köhnə şəkli sil (əgər default deyilsə)
                        if (!string.IsNullOrEmpty(oldImagePath) && oldImagePath != memberToUpdate.ImagePath && !oldImagePath.EndsWith("default-member-avatar.png"))
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
                        _logger.LogError(ex, "Error uploading image for Team Member ID {MemberId}.", id);
                        ModelState.AddModelError("Photo", "Şəkil yüklənərkən xəta baş verdi.");
                        await PopulateProfessionsDropDownList(teamMember.ProfessionId);
                        // Formdan gələn dəyərləri qorumaq üçün teamMember-i geri qaytarırıq, amma ImagePath-i dəyişmirik
                        teamMember.ImagePath = memberToUpdate.ImagePath; // Köhnə şəkli saxla
                        return View(teamMember);
                    }
                }
                else
                {
                    // Yeni şəkil yüklənməyibsə, formdan gələn köhnə ImagePath-i qoru
                    memberToUpdate.ImagePath = teamMember.ImagePath;
                }

                memberToUpdate.Name = teamMember.Name;
                memberToUpdate.ProfessionId = teamMember.ProfessionId;

                try
                {
                    //_context.Update(memberToUpdate); // Artıq context tərəfindən izlənilir
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Team Member ID {MemberId} ('{MemberName}') updated.", id, memberToUpdate.Name);
                    TempData["SuccessMessage"] = "Komanda üzvü uğurla yeniləndi.";
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    _logger.LogError(ex, "Concurrency error while updating Team Member ID {MemberId}", id);
                    if (!TeamMemberExists(memberToUpdate.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            await PopulateProfessionsDropDownList(teamMember.ProfessionId);
            return View(teamMember);
        }

        // GET: Admin/TeamMembers/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var teamMember = await _context.TeamMembers
                .Include(t => t.Profession) // Peşə adını göstərmək üçün
                .FirstOrDefaultAsync(m => m.Id == id);
            if (teamMember == null) return NotFound();

            return View(teamMember);
        }

        // POST: Admin/TeamMembers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var teamMember = await _context.TeamMembers.FindAsync(id);
            if (teamMember == null) return NotFound();

            try
            {
                // Şəkli sil (əgər default deyilsə)
                if (!string.IsNullOrEmpty(teamMember.ImagePath) && !teamMember.ImagePath.EndsWith("default-member-avatar.png"))
                {
                    string fullPath = Path.Combine(_env.WebRootPath, teamMember.ImagePath.TrimStart('/'));
                    if (System.IO.File.Exists(fullPath))
                    {
                        System.IO.File.Delete(fullPath);
                        _logger.LogInformation("Image deleted: {ImagePath}", fullPath);
                    }
                }

                _context.TeamMembers.Remove(teamMember);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Team Member ID {MemberId} ('{MemberName}') deleted.", id, teamMember.Name);
                TempData["SuccessMessage"] = "Komanda üzvü uğurla silindi.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting Team Member ID {MemberId}", id);
                TempData["ErrorMessage"] = "Komanda üzvü silinərkən xəta baş verdi.";
            }
            return RedirectToAction(nameof(Index));
        }

        private bool TeamMemberExists(int id)
        {
            return _context.TeamMembers.Any(e => e.Id == id);
        }

        // Peşələri dropdown üçün yükləmək üçün köməkçi metod
        private async Task PopulateProfessionsDropDownList(object? selectedProfession = null)
        {
            var professionsQuery = from p in _context.Professions
                                   orderby p.Name
                                   select p;
            ViewBag.ProfessionId = new SelectList(await professionsQuery.AsNoTracking().ToListAsync(), "Id", "Name", selectedProfession);
        }
    }
}