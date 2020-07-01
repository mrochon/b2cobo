using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using B2COBOWeb.Models;
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
			//var custom = $"uid={uid},scope={scope}";
			var custom = new CustomAttributes()
			{
				personalAttributes = String.Concat(personalAttrs.Where(c => !Char.IsWhiteSpace(c))),
				roles = "role1,role2",
				scopes = String.Concat(scopes.Where(c => !Char.IsWhiteSpace(c))),
			};
			return new JsonResult(custom);
		}

		static string personalAttrs = @"{
			""confidentiality"": [
				""3"",
				""4"",
				""5""
			]
		}";

		static string scopes = @"";
	}
}
