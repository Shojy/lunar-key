using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Lunar.Auth.Controllers
{
    public class DeveloperController : Controller
    {
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> New()
        {
            return View();
        }
    }
}