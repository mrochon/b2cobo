using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Web;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using B2COBOWeb.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace B2COBOWeb.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class Token : ControllerBase
    {
        public Token(ILogger<Token> logger, IOptions<B2COptions> options)
        {
            _logger = logger;
            _options = options;
        }
        private readonly ILogger<Token> _logger;
        private readonly IOptions<B2COptions> _options;
                [HttpPost]
        //[Consumes("application/x-www-form-urlencoded")]
        public async Task<IActionResult> Obo([FromForm] string grant_type, [FromForm] string client_id, [FromForm] string client_secret, [FromForm] string assertion, [FromForm] string scope, [FromForm] string requested_token_use)
        {
            if (String.IsNullOrEmpty(grant_type) || string.Compare(grant_type, "urn:ietf:params:oauth:grant-type:jwt-bearer") != 0)
                return BadRequest("Invalid grant_type");
            Guid id;
            if (!Guid.TryParse(client_id, out id))
                return BadRequest("Mising or incorrect client id");
            if (string.IsNullOrEmpty(client_secret) | client_secret.Length > 120)
                return BadRequest("Bad or missing client secret");
            if (string.IsNullOrEmpty(assertion))
                return BadRequest("Bad or missing client assertion");
            if (string.IsNullOrEmpty(scope))
                return BadRequest("Bad or missing scope");
            if (!IsTokenForClient(assertion, client_id))
                return BadRequest("Client assertion is for a different client application");

            // Initiate request to B2C
            var baseUrl =$"https://{_options.Value.tenantName}.b2clogin.com/{_options.Value.tenantId}/{_options.Value.oboJourneyName}/oauth2/v2.0";
            var http = new HttpClient();
            var query = HttpUtility.ParseQueryString(string.Empty);
            query["client_id"] = client_id;
            query["client_assertion_type"] = "urn:ietf:params:oauth:client-assertion-type:jwt-bearer";
            query["client_assertion"] = assertion;
            query["response_mode"] = "form_post";
            query["nonce"] = "defaultNonce";
            query["scope"] = scope;
            query["response_type"] = "code";
            query["redirect_uri"] = _options.Value.redirectUri;
            var url = $"{baseUrl}/authorize?{query.ToString()}";

            var req = new HttpRequestMessage(HttpMethod.Get, url);
            var resp = await http.SendAsync(req);
            if (!resp.IsSuccessStatusCode)
            {
                return new JsonResult(new ErrorMsg() { userMessage = "Bad return from B2C" });
            }
            var form = await resp.Content.ReadAsStringAsync();
            var authz = String.Empty;
            try
            {
                authz = GetCode(form);
            } catch
            {
                return BadRequest("Provided token is invalid");
            }

            // Exchange authz code for a token
            url = $"{baseUrl}/token";
            query = HttpUtility.ParseQueryString(string.Empty);
            query["grant_type"] = "authorization_code";
            query["client_id"] = client_id;
            query["scope"] = scope;
            query["code"] = authz;
            query["redirect_uri"] = _options.Value.redirectUri;
            query["client_secret"] = client_secret;
            req = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(query.ToString(), Encoding.UTF8, "application/x-www-form-urlencoded")
            };
            resp = await http.SendAsync(req);
            if (!resp.IsSuccessStatusCode)
            {
                return BadRequest($"Failed to redeem authorization code. {resp.ReasonPhrase}");
            }
            var tokenResp = await resp.Content.ReadAsStringAsync();

            return new OkObjectResult(tokenResp);
        }

        static XNamespace dflt = "http://www.w3.org/1999/xhtml";
        private string GetCode(string form)
        {
            var page = XDocument.Parse(form);
            var input = page.Descendants(dflt + "input").First();
            var code = input.Attribute("value").Value;
            return code;
        }

        private bool IsTokenForClient(string assertion, string client_id)
        {
            var body = assertion.Split('.')[1];
            while ((body.Length % 4) != 0)
            {
                body += "=";
            }
            body = System.Text.Encoding.UTF8.GetString(System.Convert.FromBase64String(body));
            var aud = JsonDocument.Parse(body).RootElement.GetProperty("aud").GetString();
            return String.Compare(client_id, aud) == 0;
        }
    }
}
