using Microsoft.AspNetCore.Mvc;

namespace CaterManagementSystem.Controllers
{
    public class TeamController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
