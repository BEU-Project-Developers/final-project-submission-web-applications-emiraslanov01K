using Microsoft.AspNetCore.Mvc;

namespace CaterManagementSystem.Controllers
{
    public class CustomController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
