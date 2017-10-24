using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;

namespace Zelis.Core.HttpServiceClient
{
    public class NtlmAuthenticatingHttpServiceClient : AuthenticatingHttpServiceClient
    {
        public NtlmAuthenticatingHttpServiceClient(
            AuthenticatingHttpServiceClientConfiguration configuration
        )
            : base(configuration)
        {
        }

        protected override HttpClient InitializeHttpClient()
        {
            HttpClientHandler handler = InitializeHttpClientHandler();
            var httpClient = new HttpClient(handler)
            {
                BaseAddress = _configuration.BaseAddress
            };
            httpClient.DefaultRequestHeaders.Clear();
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            httpClient.Timeout = new TimeSpan(0, 0, 0, _configuration.Timeout);

            return httpClient;
        }

        protected override HttpClientHandler InitializeHttpClientHandler()
        {
            var networkCredential = new NetworkCredential(
                _authenticatingHttpServiceClientConfiguration.Username,
                _authenticatingHttpServiceClientConfiguration.Password
            );

            var credentialCache = new CredentialCache
            {
                { _configuration.BaseAddress, "NTLM", networkCredential }
            };

            HttpClientHandler handler = base.InitializeHttpClientHandler();
            handler.Credentials = credentialCache;
            handler.UseDefaultCredentials = true;

            return handler;
        }
    }
}