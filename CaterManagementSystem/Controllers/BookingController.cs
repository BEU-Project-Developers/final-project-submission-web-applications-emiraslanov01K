using Microsoft.AspNetCore.Mvc;

namespace CaterManagementSystem.Controllers
{
    public class BookingController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
