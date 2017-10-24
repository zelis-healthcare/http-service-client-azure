using System;

namespace Zelis.Core.HttpServiceClient
{
    public class AzureActiveDirectoryAuthenticatingHttpServiceClientConfiguration : HttpServiceClientConfiguration
    {
        public AzureActiveDirectoryAuthenticatingHttpServiceClientConfiguration(
            string authority,
            string baseAddress,
            string clientId,
            string clientKey,
            string resourceId,
            int timeout
        )
            : base(baseAddress, timeout)
        {
            if (string.IsNullOrWhiteSpace(authority))
                throw new ArgumentNullException(authority);
            if (string.IsNullOrWhiteSpace(clientId))
                throw new ArgumentNullException(clientId);
            if (string.IsNullOrWhiteSpace(clientKey))
                throw new ArgumentNullException(clientKey);
            if (string.IsNullOrWhiteSpace(resourceId))
                throw new ArgumentNullException(resourceId);

            Authority = authority;
            ClientId = clientId;
            ClientKey = clientKey;
            ResourceId = resourceId;
        }

        public string Authority { get;  }
        public string ClientId { get;  }
        public string ClientKey { get;  }
        public string ResourceId { get;  }
    }
}