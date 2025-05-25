using Microsoft.AspNetCore.Mvc;

namespace CaterManagementSystem.Controllers
{
    public class ReadMoreController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
