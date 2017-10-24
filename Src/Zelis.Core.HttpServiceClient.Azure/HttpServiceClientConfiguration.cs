using System;

namespace Zelis.Core.HttpServiceClient
{
    public class HttpServiceClientConfiguration
    {
        private readonly Uri _baseAddress;
        private readonly int _timeout;

        public HttpServiceClientConfiguration(
            string baseAddress,
            int timeout
        )
        {
            if (string.IsNullOrWhiteSpace(baseAddress))
                throw new ArgumentNullException(nameof(baseAddress));
            if (!Uri.IsWellFormedUriString(baseAddress, UriKind.Absolute))
                throw new ArgumentException("URI is not well formed.", nameof(baseAddress));
            if (timeout <= 0)
                throw new ArgumentOutOfRangeException(nameof(timeout), "Expected positive, non-zero value.");

            _baseAddress = new Uri(baseAddress, UriKind.Absolute);
            _timeout = timeout;
        }

        public Uri BaseAddress { get { return _baseAddress; } }

        public bool EnableRequestCompression { get; set; } = false;

        public bool EnableResponseCompression { get; set; } = false;

        public int Timeout { get { return _timeout; } }
    }
}