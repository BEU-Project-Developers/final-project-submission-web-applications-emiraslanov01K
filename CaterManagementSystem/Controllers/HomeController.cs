using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; 
using System.Threading.Tasks;     // Required for async operations
using System.Linq;                // Required for FirstOrDefaultAsync
using System.Diagnostics;
using CaterManagementSystem.Models;
using CaterManagementSystem.Data;         // For ErrorViewModel

namespace CaterManagementSystem.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _context; // Inject the DbContext

        public HomeController(AppDbContext context)
        {
            _context = context; // Assign the injected context
        }

        public async Task<IActionResult> Index()
        {
            Home homeModel = await _context.Homes.FirstOrDefaultAsync(); // Get the first Home record

            return View(homeModel); // Pass the fetched or default model to the view
        }

        // Other actions like About, Contact, etc.

        
    }
}