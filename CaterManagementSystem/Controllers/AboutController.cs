using Microsoft.AspNetCore.Mvc;

namespace CaterManagementSystem.Controllers
{
    public class AboutController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
