using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;

namespace Zelis.Core.HttpServiceClient
{
    public class BasicAuthenticatingHttpServiceClient : AuthenticatingHttpServiceClient
    {
        public BasicAuthenticatingHttpServiceClient(
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
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (!(_configuration is AuthenticatingHttpServiceClientConfiguration))
                throw new ArgumentException("Expected a AuthenticatingHttpServiceClientConfiguration");
            var authenticatingConfiguration = (_configuration as AuthenticatingHttpServiceClientConfiguration);

            string credentials = Convert.ToBase64String(
                System.Text.Encoding.ASCII.GetBytes(
                    $"{authenticatingConfiguration.Username}:{authenticatingConfiguration.Password}"
                )
            );

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);

            httpClient.Timeout = new TimeSpan(0, 0, 0, _configuration.Timeout);

            return httpClient;
        }

        protected override HttpClientHandler InitializeHttpClientHandler()
        {
            if (!(_configuration is AuthenticatingHttpServiceClientConfiguration))
                throw new ArgumentException("Expected a AuthenticatingHttpServiceClientConfiguration");
            var authenticatingConfiguration = (_configuration as AuthenticatingHttpServiceClientConfiguration);

            var networkCredential = new NetworkCredential(
                authenticatingConfiguration.Username,
                authenticatingConfiguration.Password
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