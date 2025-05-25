using Microsoft.AspNetCore.Mvc;

namespace CaterManagementSystem.Controllers
{
    public class MenuController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
