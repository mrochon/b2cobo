using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace B2COBOWeb.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CustomClaims : ControllerBase
    {
        private readonly ILogger<CustomClaims> _logger;

        public CustomClaims(ILogger<CustomClaims> logger)
        {
            _logger = logger;
        }

        //TODO: Can B2C store attrs longer than 256 bytes?
        [HttpGet]
        public IActionResult Claims(string uid, string scope)
        {
            var custom = $"uid={uid},scope={scope}";
            return new JsonResult(new { custom });
        }
    }
}
