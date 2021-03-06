﻿using System;
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
            var url = $"{baseUrl}/authorize";

            var req = new HttpRequestMessage(HttpMethod.Post, url) { Content = new StringContent(query.ToString(), Encoding.UTF8, "application/x-www-form-urlencoded") };
            var resp = await http.SendAsync(req);
            if (!resp.IsSuccessStatusCode)
            {
                return new JsonResult(new ErrorMsg() { userMessage = "B2C did not accept the client assertion" });
            }
            var form = await resp.Content.ReadAsStringAsync();
            var authz = String.Empty;
            try
            {
                authz = GetCode(form);
            } catch
            {
                return BadRequest(form);
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

            return Content(tokenResp, "application/json");
        }

        static XNamespace dflt = "http://www.w3.org/1999/xhtml";
        private string GetCode(string form)
        {
            // Seems like B2C returns an invalid html when reporting errors. Parse fails.
            var page = XDocument.Parse(form);
            var input = page.Descendants(dflt + "input").First();
            //if (input != null)
                return input.Attribute("value").Value;
            //var scriptWithError = page.Descendants(dflt + "script").First(el => el.Attribute("detail") != null);
            //var error = scriptWithError != null ? scriptWithError.Attribute("detail").Value : "Unexpected response from B2C";
            //throw new Exception(error);
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

        /*
         * <!DOCTYPE html>
<!-- Build: 1.0.1552.0 -->
<!-- StateVersion: 2.1.1 -->
<!-- DeploymentMode: Development -->
<!-- CorrelationId: a64a396c-aa51-4b0e-affe-c7f7efa5f1da -->
<!-- DataCenter: BY1 -->
<!-- Slice: 001-000 -->
<html lang="en-US"><head><link rel="icon" href="data:;base64,iVBORw0KGgo="><script data-script="jQuery" src="https://mrochonb2cprod.b2clogin.com/static/bundles/jquery-bundle-1.10.2.min.js?slice=001-000&dc=BY1" nonce="ziD3PoVICzz/cxP1VBQFQA=="></script><script data-container="true" nonce="ziD3PoVICzz/cxP1VBQFQA==">var GLOBALEX = {
  "CorrelationId": "a64a396c-aa51-4b0e-affe-c7f7efa5f1da",
  "Timestamp": "2020-07-01 02:22:15Z",
  "Detail": "AADB2C90017: The client assertion provided in the request is invalid: 'client_secret' was used as the verification key"
};
         * */
    }
}
