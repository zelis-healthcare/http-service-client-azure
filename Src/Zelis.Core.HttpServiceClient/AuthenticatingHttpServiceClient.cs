using System;

namespace Zelis.Core.HttpServiceClient
{
    public abstract class AuthenticatingHttpServiceClient : HttpServiceClient
    {
        protected readonly AuthenticatingHttpServiceClientConfiguration _authenticatingHttpServiceClientConfiguration;

        public AuthenticatingHttpServiceClient(AuthenticatingHttpServiceClientConfiguration authenticatingHttpServiceClientConfiguration)
            : base(authenticatingHttpServiceClientConfiguration)
        {
            _authenticatingHttpServiceClientConfiguration = authenticatingHttpServiceClientConfiguration
                ?? throw new ArgumentNullException(nameof(authenticatingHttpServiceClientConfiguration));
        }
    }
}