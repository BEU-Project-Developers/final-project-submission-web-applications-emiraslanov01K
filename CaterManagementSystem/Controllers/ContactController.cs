using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CaterManagementSystem.Models;
using CaterManagementSystem.Data; // Sizin DbContext namespace-iniz
using System.Threading.Tasks;
using System.Linq;

namespace CaterManagementSystem.Controllers
{
    public class ContactController : Controller
    {
        private readonly AppDbContext _context;

        public ContactController(AppDbContext context)
        {
            _context = context;
        }

        // GET: /Contact
        public async Task<IActionResult> Index()
        {
            Contact contactInfo = await _context.Contacts.FirstOrDefaultAsync();

            return View(contactInfo);
        }

       
    }
}