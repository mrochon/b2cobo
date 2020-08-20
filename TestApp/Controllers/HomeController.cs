using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;
using Microsoft.Identity.Web;
using TestApp.Models;

namespace TestApp.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ITokenAcquisition _oauth2;
        private readonly IConfiguration _conf;

        public HomeController(ILogger<HomeController> logger, ITokenAcquisition oauth2, IConfiguration conf)
        {
            _logger = logger;
            _oauth2 = oauth2;
            _conf = conf;
        }
        [AllowAnonymous]
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
            // Get token to API2 using token to API1
            var opts = new OpenIdConnectOptions();
            _conf.Bind("API1", opts);
            var http = new HttpClient();
            var query = HttpUtility.ParseQueryString(string.Empty);
            query["client_id"] = opts.ClientId;
            query["client_secret"] = opts.ClientSecret;
            query["scope"] = Constants.API2Scopes.First();
            query["grant_type"] = "urn:ietf:params:oauth:grant-type:jwt-bearer";
            query["assertion"] = await _oauth2.GetAccessTokenForUserAsync(Constants.API1Scopes);
            var req = new HttpRequestMessage(HttpMethod.Post, $"{_conf.GetValue<string>("OBOServerUrl")}/token")
            {
                Content = new StringContent(query.ToString(), Encoding.UTF8, "application/x-www-form-urlencoded")
            };
            var resp = await http.SendAsync(req);
            if (resp.IsSuccessStatusCode)
                ViewBag.TokenResponse = await resp.Content.ReadAsStringAsync();
            else
                ViewBag.TokenResponse = resp.ToString();
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
