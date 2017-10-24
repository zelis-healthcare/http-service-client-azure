using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace Zelis.Core.HttpServiceClient.Azure
{
    public class AzureActiveDirectoryAuthenticatingHttpServiceClient : HttpServiceClient
    {
        #region Internal Classes

        private class AzureActiveDirectoryAuthenticatingHttpClient : HttpClient
        {
            public delegate Task<string> GetAzureActiveDirectoryBearerToken(CancellationToken cancellationToken);

            private readonly GetAzureActiveDirectoryBearerToken _getAzureActiveDirectoryBearerToken;

            public AzureActiveDirectoryAuthenticatingHttpClient(HttpMessageHandler handler, GetAzureActiveDirectoryBearerToken getAzureActiveDirectoryBearerToken)
                : base(handler)
            {
                _getAzureActiveDirectoryBearerToken = getAzureActiveDirectoryBearerToken ?? throw new ArgumentNullException(nameof(getAzureActiveDirectoryBearerToken));
            }

            public override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                if (request == null)
                    throw new ArgumentNullException(nameof(request));
                if (cancellationToken == null)
                    throw new ArgumentNullException(nameof(cancellationToken));

                string azureActiveDirectoryBearerToken = await _getAzureActiveDirectoryBearerToken(cancellationToken);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", azureActiveDirectoryBearerToken);

                return await base.SendAsync(request, cancellationToken);
            }
        }

        #endregion

        private readonly AzureActiveDirectoryAuthenticatingHttpServiceClientConfiguration _azureActiveDirectoryAuthenticatingHttpServiceClientConfiguration;

        private AuthenticationContext _authenticationContext;
        private readonly ClientCredential _clientCredential;

        public AzureActiveDirectoryAuthenticatingHttpServiceClient(
            AzureActiveDirectoryAuthenticatingHttpServiceClientConfiguration azureActiveDirectoryAuthenticatingHttpServiceClientConfiguration
        )
            : base(azureActiveDirectoryAuthenticatingHttpServiceClientConfiguration)
        {
            _azureActiveDirectoryAuthenticatingHttpServiceClientConfiguration = azureActiveDirectoryAuthenticatingHttpServiceClientConfiguration
                ?? throw new ArgumentNullException(nameof(azureActiveDirectoryAuthenticatingHttpServiceClientConfiguration));

            _authenticationContext = new AuthenticationContext(_azureActiveDirectoryAuthenticatingHttpServiceClientConfiguration.Authority, false);
            _clientCredential = new ClientCredential(
                _azureActiveDirectoryAuthenticatingHttpServiceClientConfiguration.ClientId,
                _azureActiveDirectoryAuthenticatingHttpServiceClientConfiguration.ClientKey
            );
        }

        protected override HttpClient InitializeHttpClient()
        {
            HttpClientHandler handler = InitializeHttpClientHandler();
            var httpClient = new AzureActiveDirectoryAuthenticatingHttpClient(handler, GetAzureActiveDirectoryBearerToken)
            {
                BaseAddress = _configuration.BaseAddress,
            };

            //httpClient.DefaultRequestHeaders.Clear();
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            httpClient.Timeout = new TimeSpan(0, 0, 0, _configuration.Timeout);

            return httpClient;
        }

        private async Task<string> GetAzureActiveDirectoryBearerToken(CancellationToken cancellationToken)
        {
            if (cancellationToken == null)
                throw new ArgumentNullException(nameof(cancellationToken));

            AuthenticationResult result = await _authenticationContext.AcquireTokenAsync(
                _azureActiveDirectoryAuthenticatingHttpServiceClientConfiguration.ResourceId,
                _clientCredential
            );
            return result.AccessToken;
        }
    }
}