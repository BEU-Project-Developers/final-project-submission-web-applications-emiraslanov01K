using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CaterManagementSystem.Models;
using CaterManagementSystem.Data; // Sizin DbContext namespace-iniz
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace CaterManagementSystem.Controllers
{
    public class MenuController : Controller
    {
        private readonly AppDbContext _context;

        public MenuController(AppDbContext context)
        {
            _context = context;
        }

        // GET: /Menu
        public async Task<IActionResult> Index()
        {
            List<MenuItem> menuItems = await _context.MenuItems.ToListAsync();
            return View(menuItems);
        }
    }
}