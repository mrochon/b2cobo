# B2C OBO sample 
Shows an implementation of [OAuth2 Extension grant (OBO)](https://tools.ietf.org/html/rfc6749#section-4.5) using Azure AD B2C.

## Introduction
Currently, B2C does not support this grant. This sample uses a custom web service (B2BOBOWeb) to provide a token endpoint, which 
handles the Extension Grant requests and communicates with B2C to respond with a valid response (access token). It uses a specific B2C tenant
configured with custom journeys to handle this communication.

**Note:** this sample code, not intended for production use.

## Architecture
The B2COBOWeb web service provides one endpoint, receiving POST requests according to the OAuth2 specified format. 
The request must include a client assertion, which is a previously received access token issued by the B2C tenant.
The endpoint makes a
standard OAuth2 Authorization Code request to a [B2C custom journey](https://github.com/mrochon/b2cobo/blob/master/SocialAndLocalAccounts/obo.xml),
including the client assertion. B2C validates the token and, provided the application presenting the token
has been configured to requested API, issues a new token to that API.

TestApp in this repo is a [deployed sample client web app](https://b2cobotestapp.azurewebsites.net/) using the OBO request.

