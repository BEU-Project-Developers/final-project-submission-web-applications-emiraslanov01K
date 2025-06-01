using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CaterManagementSystem.Models;
using CaterManagementSystem.Data;    //  DbContext namespace-iniz
using CaterManagementSystem.ViewModels; // ViewModel namespace-i
using System.Threading.Tasks;
using System.Linq;

namespace CaterManagementSystem.Controllers
{
    public class AboutController : Controller
    {
        private readonly AppDbContext _context;

        public AboutController(AppDbContext context)
        {
            _context = context;
        }

      
        public async Task<IActionResult> Index()
        {
            var aboutSectionData = await _context.About.FirstOrDefaultAsync();

            
            // Statistik məlumatların hesablanması
            // Fərz edək ki, "Users" adlı bir DbSet-iniz var istifadəçilər üçün.
            // Əgər yoxdursa, bu hissəni dəyişdirin və ya statik bir dəyər verin.
            int happyCustomers = 0;
            if (_context.Users != null) // Users DbSet-inin mövcudluğunu yoxlayın
            {
                happyCustomers = await _context.Users.CountAsync();
            }
            else // Əgər Users cədvəli yoxdursa, statik dəyər və ya başqa məntiq
            {
               
            }


            int expertChefs = await _context.TeamMembers.CountAsync(); // kamanda uzuvlerini sayriq 

            // Və ya yalnız müəyyən peşədə olanları saymaq istəsəniz:
            // int expertChefs = await _context.TeamMembers.CountAsync(tm => tm.Profession.Name.Contains("Chef"));

            int eventsComplete = await _context.Events.CountAsync();

            var viewModel = new AboutPageViewModel  // view model yaradırıq və məlumatları ötürrük ( customer , chefs və s ) 
            {
                AboutSectionData = aboutSectionData,
                HappyCustomersCount = happyCustomers,
                ExpertChefsCount = expertChefs,
                EventsCompleteCount = eventsComplete
            };

            return View(viewModel);  // yaradtdığımız viewnu about page ə return edirik 
        }
    }
}