﻿using Microsoft.AspNetCore.Mvc;

namespace CaterManagementSystem.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
