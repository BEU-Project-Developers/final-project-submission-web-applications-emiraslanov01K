using Microsoft.AspNetCore.Mvc;

namespace CaterManagementSystem.Controllers
{
    public class EventController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
