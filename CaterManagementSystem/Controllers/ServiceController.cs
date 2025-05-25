using Microsoft.AspNetCore.Mvc;

namespace CaterManagementSystem.Controllers
{
    public class ServiceController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
