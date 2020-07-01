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
				roles = "General Foreman,Quality Engineer",
				scopes = String.Concat(scopes.Where(c => !Char.IsWhiteSpace(c))),
			};
			return new JsonResult(custom);
		}

		static string personalAttrs = @"{
			""confidentiality"": [
				""3"",
				""4"",
				""5""
			],
			""clearanceCommercial"": [
				""Bechtel Proprietary""
			],
			""clearanceGovernment"": [
				""Secret""
			],
			""physicalLocation"": ""USA"",
			""citizenship"": [
				""USA"",
				""GBR""
			],
			""allowableUse"": [
				""""
			],
			""infrastructureRestriction"": """",
			""certification"": [
				""Control Systems"",
				""Power Systems""
			],
			""educationCertification"": [
				""BS Electrical Engineering"",
				""MS Electrical Engineering""
			]
		}";

		static string scopes = @"[
			{
				""name"": ""Pipe Design"",
				""information"": ""Pipe Design"",
				""permission"": [
					""read"",
					""update status""
				],
				""company"": [
					""Bechtel Corp""
				],
				""GBU"": [
					""NS&E""
				],
				""project"": [
					""Vogtle Unit 3""
				],
				""initiative"": [
					""""
				],
				""operation"": [
					""""
				],
				""WBS"": [
					""""
				],
				""roles"": [
					""General Foreman""
				]
	},
			{
				""name"": ""Pipe Design"",
				""information"": ""Pipe Design"",
				""permission"": [
					""read"",
					""report"",
					""archive""
				],
				""company"": [
					""Bechtel Corp""
				],
				""GBU"": [
					""NS&E""
				],
				""project"": [
					""Vogtle Unit 4""
				],
				""initiative"": [
					""""
				],
				""operation"": [
					""""
				],
				""WBS"": [
					""""
				],
				""roles"": [
					""Quality Engineer""
				]
		}";
	}
}
