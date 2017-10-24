using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Specialized;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Zelis.Core.HttpServiceClient
{
    public interface IHttpServiceClient
    {
        Task<HttpResponseMessage> DeleteRequest(Uri uri, CancellationToken cancellationToken, WebHeaderCollection requestHeaders = null);
        Task<HttpResponseMessage> GetRequest(Uri uri, CancellationToken cancellationToken, WebHeaderCollection requestHeaders = null);
        Task<HttpResponseMessage> PostRequest(Uri uri, HttpContent body, CancellationToken cancellationToken, WebHeaderCollection requestHeaders = null);
        Task<HttpResponseMessage> PutRequest(Uri uri, HttpContent body, CancellationToken cancellationToken, WebHeaderCollection requestHeaders = null);
        [Obsolete("Deprecated 12/21/2016 in favor of GetRequest/PostRequest/PutRequest/etc.", false)]
        Task<ServiceClientResponse> InvokeAsync(HttpMethod httpMethod, string uri, object request = null, NameValueCollection queryParameters = null, CancellationToken? cancellationToken = null, WebHeaderCollection requestHeaders = null);
    }

    [Obsolete("Deprecated 12/21/2016 along iwth IHttpServiceClient.InvokeAsync", false)]
    public class ServiceClientResponse
    {
        public string JsonBody { get; set; }
        public bool IsSuccessful { get { return ResponseMessage.IsSuccessStatusCode; } }
        public HttpStatusCode StatusCode { get { return ResponseMessage.StatusCode; } }
        public string ReasonPhrase { get { return ResponseMessage.ReasonPhrase; } }
        public HttpResponseMessage ResponseMessage { get; set; }

        public ServiceClientResponse(HttpResponseMessage responseMessage)
        {
            JsonBody = Task.Run(() => responseMessage.Content.ReadAsStringAsync()).Result;
            ResponseMessage = responseMessage;
        }

        public TResponse GetObject<TResponse>(TResponse returnObject = default(TResponse))
        {
            return JsonConvert.DeserializeAnonymousType(JsonBody, returnObject);
        }

        public TElementType GetElement<TElementType>(string elementName, TElementType returnObject = default(TElementType))
        {
            dynamic data = JObject.Parse(JsonBody);
            if (data[elementName] == null)
                return default(TElementType);

            string element = data[elementName].ToString();
            return JsonConvert.DeserializeAnonymousType(element, returnObject);
        }
    }

}