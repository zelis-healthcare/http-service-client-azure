using Newtonsoft.Json;
using System;
using System.Collections.Specialized;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Zelis.Core.HttpServiceClient
{
    public class HttpServiceClient : IHttpServiceClient
    {
        protected readonly HttpServiceClientConfiguration _configuration;

        protected readonly HttpClient _httpClient;

        public HttpServiceClient(HttpServiceClientConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

            _httpClient = InitializeHttpClient();
        }

        protected virtual HttpClient InitializeHttpClient()
        {
            HttpClientHandler handler = InitializeHttpClientHandler();
            var httpClient = new HttpClient(handler)
            {
                BaseAddress = _configuration.BaseAddress
            };

            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            httpClient.Timeout = new TimeSpan(0, 0, 0, _configuration.Timeout);

            return httpClient;
        }

        protected virtual HttpClientHandler InitializeHttpClientHandler()
        {
            var handler = new HttpClientHandler
            {
                AllowAutoRedirect = true,
            };

            if (
                _configuration.EnableResponseCompression
                && handler.SupportsAutomaticDecompression
            )
                handler.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;

            return handler;
        }

        public virtual async Task<HttpResponseMessage> DeleteRequest(Uri uri, CancellationToken cancellationToken, WebHeaderCollection requestHeaders = null)
        {
            return await SendWithoutBody(uri, HttpMethod.Delete, cancellationToken, requestHeaders);
        }

        public virtual async Task<HttpResponseMessage> GetRequest(Uri uri, CancellationToken cancellationToken, WebHeaderCollection requestHeaders = null)
        {
            return await SendWithoutBody(uri, HttpMethod.Get, cancellationToken, requestHeaders);
        }

        public virtual async Task<HttpResponseMessage> PostRequest(Uri uri, HttpContent body, CancellationToken cancellationToken, WebHeaderCollection requestHeaders = null)
        {
            return await SendWithBody(uri, HttpMethod.Post, body, cancellationToken, requestHeaders);
        }

        public virtual async Task<HttpResponseMessage> PutRequest(Uri uri, HttpContent body, CancellationToken cancellationToken, WebHeaderCollection requestHeaders = null)
        {
            return await SendWithBody(uri, HttpMethod.Put, body, cancellationToken, requestHeaders);
        }

        [Obsolete("Deprecated 12/21/2016 along with IHttpServiceClient.InvokeAsync", false)]
        public async Task<ServiceClientResponse> InvokeAsync(HttpMethod httpMethod, string uri, object request = null, NameValueCollection queryParameters = null, CancellationToken? cancellationToken = null, WebHeaderCollection requestHeaders = null)
        {
            if (cancellationToken == null)
                cancellationToken = new CancellationTokenSource().Token;

            if (string.IsNullOrWhiteSpace(uri))
                uri = string.Empty;

            if (queryParameters != null)
                uri += "?" + queryParameters.ToString();

            HttpResponseMessage responseMessage = null;

            if(httpMethod == HttpMethod.Get || httpMethod == HttpMethod.Delete)
                responseMessage = await SendWithoutBody(new Uri(uri, UriKind.Relative), httpMethod, cancellationToken.Value, requestHeaders);

            if (httpMethod == HttpMethod.Post || httpMethod == HttpMethod.Put) {
                string requestJson = request == null ? "" : JsonConvert.SerializeObject(request);
                HttpContent content = new StringContent(requestJson, Encoding.ASCII, "application/json");
                responseMessage = await SendWithBody(new Uri(uri, UriKind.Relative), httpMethod, content, cancellationToken.Value, requestHeaders);
            }

            return new ServiceClientResponse(responseMessage);
        }


        protected virtual async Task<HttpResponseMessage> SendWithBody(Uri uri, HttpMethod httpMethod, HttpContent content, CancellationToken cancellationToken, WebHeaderCollection requestHeaders = null)
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

            HttpContent requestContent = content;
            if (_configuration.EnableRequestCompression)
            {
                MediaTypeHeaderValue mediaTypeHeaderValue = requestContent.Headers.ContentType;
                byte[] requestContentBytes = await requestContent.ReadAsByteArrayAsync();
                using (var stream = new MemoryStream())
                {
                    using (var gzip = new GZipStream(stream, CompressionMode.Compress, true))
                    {
                        await gzip.WriteAsync(requestContentBytes, 0, requestContentBytes.Length);
                    }
                    requestContent = new ByteArrayContent(stream.ToArray());
                }
                requestContent.Headers.ContentEncoding.Add("gzip");
                requestContent.Headers.ContentType = mediaTypeHeaderValue;
            }

            var request = new HttpRequestMessage(httpMethod, uri);
            request.Content = requestContent;
            if (requestHeaders != null)
            {
                //request.Headers.Clear();
                foreach (string headerName in requestHeaders.AllKeys)
                {
                    string headerValue = requestHeaders[headerName];
                    request.Headers.Add(headerName, headerValue);
                }
            }

            return await _httpClient.SendAsync(request, cancellationToken);
        }

        protected virtual async Task<HttpResponseMessage> SendWithoutBody(Uri uri, HttpMethod httpMethod, CancellationToken cancellationToken, WebHeaderCollection requestHeaders = null)
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

            var request = new HttpRequestMessage(httpMethod, uri);
            if (requestHeaders != null)
            {
                //request.Headers.Clear();
                foreach (string headerName in requestHeaders.AllKeys)
                {
                    string headerValue = requestHeaders[headerName];
                    request.Headers.Add(headerName, headerValue);
                }
            }

            return await _httpClient.SendAsync(request, cancellationToken);
        }
    }
}                       