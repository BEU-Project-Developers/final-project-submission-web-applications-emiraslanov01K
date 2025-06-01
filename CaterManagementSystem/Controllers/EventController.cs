using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CaterManagementSystem.Models;
using CaterManagementSystem.Data; // Sizin DbContext namespace-iniz
using System.Threading.Tasks;
using System.Collections.Generic; // List<Event> üçün
using System.Linq;

namespace CaterManagementSystem.Controllers
{
    public class EventController : Controller
    {
        private readonly AppDbContext _context;

        public EventController(AppDbContext context)
        {
            _context = context;
        }

        // GET: /Events
        public async Task<IActionResult> Index()
        {
            List<Event> events = await _context.Events.ToListAsync();
            return View(events);
        }
    }
}