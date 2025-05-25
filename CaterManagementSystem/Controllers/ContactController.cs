using Microsoft.AspNetCore.Mvc;

namespace CaterManagementSystem.Controllers
{
    public class ContactController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
