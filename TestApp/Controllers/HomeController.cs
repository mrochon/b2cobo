using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Web;
using TestApp.Models;

namespace TestApp.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ITokenAcquisition _oauth2;

        public HomeController(ILogger<HomeController> logger, ITokenAcquisition oauth2)
        {
            _logger = logger;
            _oauth2 = oauth2;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> GetAPI1Token()
        {
            ViewBag.Token = await _oauth2.GetAccessTokenForUserAsync(Constants.API1Scopes);
            return View();
        }

        public async Task<IActionResult> GetAPI2Token()
        {
            // Get API1 token again
            ViewBag.Token = await _oauth2.GetAccessTokenForUserAsync(Constants.API1Scopes);

            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
