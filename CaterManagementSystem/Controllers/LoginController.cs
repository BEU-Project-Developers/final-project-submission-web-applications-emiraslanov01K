using Microsoft.AspNetCore.Mvc;

namespace CaterManagementSystem.Controllers
{
    public class LoginController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
