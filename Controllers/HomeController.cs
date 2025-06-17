using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using WebAppTest.Models;

namespace WebAppTest.Controllers
{
    public class HomeController : Controller
    {
        /// <summary>
        /// Контроллер для роутинга приложения
        /// </summary>
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Cars()
        {
            return Redirect("~/Cars/");
        }

        public IActionResult Owners()
        {
            return Redirect("~/Owners/");
        }

        public IActionResult Mileages()
        {
            return Redirect("~/Mileages/");
        }

        public IActionResult Brands()
        {
            return Redirect("~/Brands/");
        }

        public IActionResult Accidents()
        {
            return Redirect("~/Accidents/");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
