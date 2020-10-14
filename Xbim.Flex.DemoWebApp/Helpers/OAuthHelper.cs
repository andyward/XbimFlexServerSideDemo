using IdentityModel.Client;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Xbim.Flex.Client;

namespace Xbim.Flex.DemoWebApp.Helpers
{
    public static class OAuthHelper
    {
        /// <summary>
        /// Gets an access token from the Flex ID Server
        /// </summary>
        /// <returns></returns>
        public static async Task<TokenResponse> GetClientAccessToken()
        {
            var client = new HttpClient();

            // discover the OAuth2 endpoints.
            var disco = await client.GetDiscoveryDocumentAsync(Config.FlexIdServerAddress);
            if (disco.IsError) throw new Exception(disco.Error);

            // Use Client Credentials flow. Note this means it's *this app* that is authenticated, not an *end user*.
            var response = await client.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
            {
                Address = disco.TokenEndpoint,
                Scope = "api.write",
                ClientId = Config.ClientId,
                ClientSecret = Config.ClientSecret, 
            });

            if (response.IsError) throw new Exception(response.Error);
            return response;
        }

        /// <summary>
        /// Builds a Flex API Client using the provided access token
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public static FlexAPI BuildApiClient(TokenResponse token)
        {
            var httpClient = new HttpClient();

            httpClient.SetBearerToken(token.AccessToken);

            var flexClient = new FlexAPI(httpClient)
            {
                BaseUrl = Config.FlexApiBase
            };
            return flexClient;
        }
    }
}