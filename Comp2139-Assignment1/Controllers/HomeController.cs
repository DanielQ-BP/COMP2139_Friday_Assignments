using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Comp2139_Assignment1.Models;

namespace Comp2139_Assignment1.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        [Route("Home/Error")]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            var requestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;

            _logger.LogError("Unhandled exception occurred. Request ID: {RequestId}", requestId);

            return View("InternalServerError", new ErrorViewModel
            {
                RequestId = requestId
            });
        }

        [Route("Home/CustomNotFound")]
        public IActionResult CustomNotFound(int? statusCode = null)
        {
            _logger.LogWarning("404 Not Found: StatusCode {StatusCode}, Path: {Path}", statusCode, HttpContext.Request.Path);
            ViewData["StatusCode"] = statusCode ?? 404;
            return View("NotFound");
        }

    }
}