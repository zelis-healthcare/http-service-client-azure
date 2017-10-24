using System;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Zelis.Core.HttpServiceClient
{
    public class HMACAuthenticatingHttpServiceClient : HttpServiceClient
    {
        private readonly HMACAuthenticatingHttpServiceClientConfiguration _hmacAuthenticatingHttpServiceClientConfiguration;

        public HMACAuthenticatingHttpServiceClient(HMACAuthenticatingHttpServiceClientConfiguration hmacAuthenticatingHttpServiceClientConfiguration)
            : base(hmacAuthenticatingHttpServiceClientConfiguration)
        {
            _hmacAuthenticatingHttpServiceClientConfiguration = hmacAuthenticatingHttpServiceClientConfiguration
                ?? throw new ArgumentNullException(nameof(hmacAuthenticatingHttpServiceClientConfiguration));
        }

        protected override async Task<HttpResponseMessage> SendWithBody(Uri uri, HttpMethod httpMethod, HttpContent content, CancellationToken cancellationToken, WebHeaderCollection requestHeaders = null)
        {
            if (uri == null)
                throw new ArgumentNullException(nameof(uri));
            if (!uri.IsWellFormedOriginalString())
                throw new ArgumentException("URI is not well-formed.", nameof(uri));
            if (uri.IsAbsoluteUri)
                throw new ArgumentException("URI is absolute, instead of the required relative UriKind.", nameof(uri));
            if (httpMethod == null)
                throw new ArgumentNullException(nameof(httpMethod));
            if (!(httpMethod == HttpMethod.Post || httpMethod == HttpMethod.Put))
                throw new ArgumentException("HttpMethod not supported by this method.", nameof(httpMethod));
            if (content == null)
                throw new ArgumentNullException(nameof(content));
            if (cancellationToken == null)
                throw new ArgumentNullException(nameof(cancellationToken));

            if (requestHeaders == null)
                requestHeaders = new WebHeaderCollection();
            requestHeaders[HttpRequestHeader.Authorization] = $"HMAC {await CreateHMACAuthenticationParameter(httpMethod, uri, content)}";

            return await base.SendWithBody(uri, httpMethod, content, cancellationToken, requestHeaders);
        }


        protected override async Task<HttpResponseMessage> SendWithoutBody(Uri uri, HttpMethod httpMethod, CancellationToken cancellationToken, WebHeaderCollection requestHeaders = null)
        {
            if (uri == null)
                throw new ArgumentNullException(nameof(uri));
            if (!uri.IsWellFormedOriginalString())
                throw new ArgumentException("URI is not well-formed.", nameof(uri));
            if (uri.IsAbsoluteUri)
                throw new ArgumentException("URI is absolute, instead of the required relative UriKind.", nameof(uri));
            if (httpMethod == null)
                throw new ArgumentNullException(nameof(httpMethod));
            if (!(httpMethod == HttpMethod.Delete || httpMethod == HttpMethod.Get))
                throw new ArgumentException("HttpMethod not supported by this method.", nameof(httpMethod));
            if (cancellationToken == null)
                throw new ArgumentNullException(nameof(cancellationToken));

            if (requestHeaders == null)
                requestHeaders = new WebHeaderCollection();
            requestHeaders[HttpRequestHeader.Authorization] = $"HMAC {await CreateHMACAuthenticationParameter(httpMethod, uri)}";

            return await base.SendWithoutBody(uri, httpMethod, cancellationToken, requestHeaders);
        }

        private async Task<string> CreateHMACAuthenticationParameter(HttpMethod httpMethod, Uri requestUri, HttpContent httpContent = null)
        {
            if (httpMethod == null)
                throw new ArgumentNullException(nameof(httpMethod));
            if (requestUri == null)
                throw new ArgumentNullException(nameof(requestUri));
            // httpContent can be null

            string requestContentMD5Base64String = string.Empty;
            string requestContentType = string.Empty;
            if (httpContent != null)
            {
                using (var md5 = MD5.Create())
                {
                    requestContentMD5Base64String = Convert.ToBase64String(
                        md5.ComputeHash(
                            await httpContent.ReadAsByteArrayAsync()
                        )
                    );
                }
                requestContentType = httpContent.Headers?.ContentType?.MediaType ?? throw new InvalidOperationException("Cannot ascertain contentType.");
            }

            string timestamp = ((long)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds).ToString();

            string canonicalizedResource = $"/{requestUri.ToString()}";

            string nonce = Guid.NewGuid().ToString("N");

            string rawSignatureData = string.Join(
                "\n",
                httpMethod.Method,
                requestContentMD5Base64String,
                requestContentType,
                timestamp,
                string.Empty,
                canonicalizedResource,
                nonce
            );
            string signature = string.Empty;
            using (var hmac = new HMACSHA1(_hmacAuthenticatingHttpServiceClientConfiguration.KeyBytes))
            {
                signature = Convert.ToBase64String(
                    hmac.ComputeHash(
                        Encoding.UTF8.GetBytes(rawSignatureData)
                    )
                );
            }

            return string.Join(
                ":",
                _hmacAuthenticatingHttpServiceClientConfiguration.KeyId,
                signature,
                nonce,
                timestamp
            );
        }
    }
}                       