using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CaterManagementSystem.Models;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using CaterManagementSystem.Data; // List<Service> üçün

namespace CaterManagementSystem.Controllers
{
    public class ServiceController : Controller
    {
        private readonly AppDbContext _context;

        public ServiceController(AppDbContext context)
        {
            _context = context;
        }

        // GET: /Services
        public async Task<IActionResult> Index()
        {
            List<Service> services = await _context.Services.ToListAsync();
            return View(services);
        }

        // GET: /Services/Details/5
        // Buradakı 'id' ServiceId-dir.
        public async Task<IActionResult> Details(int? id)
        {
            

            var serviceDescription = await _context.ServiceDescriptions
                                                 .Include(sd => sd.Service) 
                                                 .FirstOrDefaultAsync(sd => sd.ServiceId == id);

            if (serviceDescription == null)  // bu hissə əlavə ayarlardır ( cooming soon and not found üçün ) 
            {
               
                var serviceExists = await _context.Services.AnyAsync(s => s.Id == id);
                if (serviceExists)
                {
                    ViewData["ServiceTitle"] = (await _context.Services.FindAsync(id))?.Title;
                    return View("DetailsComingSoon"); // Xüsusi bir "Tezliklə" səhifəsi
                }
                return NotFound(); // Nə service, nə də description tapılmadı
            }

            return View(serviceDescription);
        }
    }
}