// Areas/Admin/Controllers/DashboardController.cs
using Microsoft.AspNetCore.Mvc;
using CaterManagementSystem.Data;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using CaterManagementSystem.Areas.Admin.ViewModels; // DashboardStatsViewModel üçün
using Microsoft.AspNetCore.Authorization;

namespace CaterManagementSystem.Areas.Admin.Controllers
{
    [Area("Admin")]
    //[Authorize(Roles = "Admin")] // Yalnız adminlər daxil ola bilsin
    public class DashboardController : Controller
    {
        private readonly AppDbContext _context;
        private readonly ILogger<DashboardController> _logger;

        public DashboardController(AppDbContext context, ILogger<DashboardController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var viewModel = new DashboardStatsViewModel();

            try
            {
                // Fərz edək ki, User modelinizdə bütün istifadəçilər saxlanılır
                viewModel.TotalUsers = await _context.Users.CountAsync();

                // Fərz edək ki, Service adlı bir modeliniz var
                // viewModel.ActiveServicesCount = await _context.Services.CountAsync(s => s.IsActive); // Məsələn
                // Əgər Service modeliniz yoxdursa, bu hissəni çıxarın və ya uyğunlaşdırın
                viewModel.ActiveServicesCount = await _context.Services.CountAsync(); // Nümunə olaraq bütün servislərin sayı


                // Fərz edək ki, TeamMember modeliniz var və aşpazları Role və ya bir bool ilə ayırırsınız
                // viewModel.TotalChefs = await _context.TeamMembers.CountAsync(t => t.IsChef); // Məsələn
                // Əgər TeamMember modeliniz yoxdursa və ya fərqli strukturunuz varsa, uyğunlaşdırın
                viewModel.TotalChefs = await _context.TeamMembers.CountAsync(); // Nümunə olaraq bütün komanda üzvlərinin sayı


                // Fərz edək ki, Event adlı bir modeliniz var
                // viewModel.TotalEvents = await _context.Events.CountAsync(e => e.EventDate >= DateTime.Today); // Məsələn, gələcək tədbirlər
                // Əgər Event modeliniz yoxdursa və ya fərqli strukturunuz varsa, uyğunlaşdırın
                viewModel.TotalEvents = await _context.Events.CountAsync(); // Nümunə olaraq bütün tədbirlərin sayı

                // AboutPageViewModel-dən gələn məlumatları da istifadə edə bilərsiniz (əgər AppDbContext-də Abouts varsa)
                // var aboutData = await _context.Abouts.FirstOrDefaultAsync();
                // if (aboutData != null)
                // {
                //     // viewModel.TotalChefs = aboutData.ExpertChefsCount; // Əgər About modelində belə bir sahə varsa
                // }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading dashboard statistics.");
                // Xəta halında default dəyərlər göstərilə bilər və ya istifadəçiyə mesaj verilə bilər
                viewModel.TotalUsers = 0;
                viewModel.ActiveServicesCount = 0;
                viewModel.TotalChefs = 0;
                viewModel.TotalEvents = 0;
                TempData["ErrorMessage"] = "Statistik məlumatlar yüklənərkən xəta baş verdi.";
            }

            return View(viewModel); // View-a DashboardStatsViewModel göndərilir
        }
    }
}