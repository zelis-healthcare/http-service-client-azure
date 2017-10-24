using System;

namespace Zelis.Core.HttpServiceClient
{
    public class HMACAuthenticatingHttpServiceClientConfiguration : HttpServiceClientConfiguration
    {
        public HMACAuthenticatingHttpServiceClientConfiguration(
            string baseAddress,
            string key,
            string keyId,
            int timeout
        )
            : base(baseAddress, timeout)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));
            if (string.IsNullOrWhiteSpace(keyId))
                throw new ArgumentNullException(nameof(keyId));

            Key = key;
            KeyId = keyId;
        }

        public string Key { get; set; }

        public byte[] KeyBytes { get { return Convert.FromBase64String(Key); } }

        public string KeyId { get; set; }
    }
}