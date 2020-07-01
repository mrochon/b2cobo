# AzureAD B2C OBO sample 
Shows an implementation of [OAuth2 Extension grant (OBO)](https://tools.ietf.org/html/rfc6749#section-4.5) using Azure AD B2C.

## Introduction
Currently, Azure B2C does not support the extension grant. This sample uses a custom web service (B2BOBOWeb) to provide a token endpoint, which 
handles the Extension Grant requests and communicates with B2C to respond with a valid response (access token). It uses a specific B2C tenant
configured with custom journeys to handle this communication.

**Note:** this sample code, not intended for production use.

## Design
The B2COBOWeb web service provides one endpoint: */token*. It handles the [OAuth2 Extension Grant request](https://tools.ietf.org/html/rfc6749#section-4.5). 
As per the spec, the request includes the id of the requesting client, its secret key and 
client assertion type and value (currently JWT). Client assertion is the access token the client received when it was called by another client.
It must be a B2C-issued token. The */token* endpoint makes a
standard OAuth2 Authorization Code request to a [B2C custom journey](https://github.com/mrochon/b2cobo/blob/master/SocialAndLocalAccounts/obo.xml),
including the client assertion. B2C validates the token and, provided the application presenting the token
has been configured to requested API, issues an authorization code. The B2COBOWebApp uses this token client secret provided in the
request to exchange the code and an access toke, which is then returned to the caller.

*TestApp* in this repo is a [deployed sample client web app](https://b2cobotestapp.azurewebsites.net/) using the OBO request.



