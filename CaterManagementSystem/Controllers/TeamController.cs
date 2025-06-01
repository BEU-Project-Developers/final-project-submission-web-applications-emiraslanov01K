using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CaterManagementSystem.Models;
using CaterManagementSystem.Data; // Sizin DbContext namespace-iniz
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace CaterManagementSystem.Controllers
{
    public class TeamController : Controller
    {
        private readonly AppDbContext _context;

        public TeamController(AppDbContext context)
        {
            _context = context;
        }

        // GET: /Teams
        public async Task<IActionResult> Index()
        {
         
            List<TeamMember> teamMembers = await _context.TeamMembers
                                                         .Include(tm => tm.Profession)
                                                         .ToListAsync();
            return View(teamMembers);
        }
    }
}