using System.Net;
using Newtonsoft.Json;
using NStack.Exceptions;
using NStack.WebClient;


namespace NStack.Auth
{
    public class ClientConnection : IClientConnection
    {
        public KeystoneResponse LastKeystoneResponse { get; private set; }
        private AuthData AuthData { get; set; }
        private string m_tokenUri ;


        public KeystoneResponse Connect(string tokenUri, AuthData authData, bool cacheAuthData = true)
        {
            m_tokenUri = tokenUri;
            AuthData = cacheAuthData ? authData : null;

            var kr = JsonPostRequestWithoutAuthHeader<AuthData, KeystoneResponse> (authData, tokenUri);

            if (kr.Result == null)
                throw new AuthenticationException (kr, "Authentication failed");
            LastKeystoneResponse = kr.Result;
            return LastKeystoneResponse;
        }


        private Response<TOut> JsonPostRequestWithoutAuthHeader<TIn, TOut>(TIn requestObject, string uri) where TOut : class, new()
        {
            var request = RequestHelper.PrepareJsonWebRequestWithoutAuthHeader(uri, HttpMethod.POST);
            string postData = JsonConvert.SerializeObject(requestObject);
            return RequestHelper.MakeStringRequest<TOut>(request, postData);
        }


        private HttpWebRequest PrepareJsonWebRequest(string uri, string method)
        {
            var request = RequestHelper.PrepareJsonWebRequestWithoutAuthHeader(uri, method);
            InjectAuthHeader (request);
            return request;
        }


        private HttpWebRequest PrepareWebRequest(string uri, string method)
        {
            var request = RequestHelper.PrepareWebRequestWithoutAuthHeader(uri, method);
            InjectAuthHeader(request);
            return request;
        }


        private void InjectAuthHeader(HttpWebRequest request)
        {
            // FIXME: check if token is still valid, if not (or will shortly become invalid), send a request for a new token
            if (LastKeystoneResponse == null)
                throw new AuthDataNotAvailableException();
            request.Headers.Add("X-Auth-Token", LastKeystoneResponse.Access.Token.Id);
        }


        private Response<TOut> JsonRequest<TOut>(string uri, string method) where TOut : class, new()
        {
            var request = PrepareJsonWebRequest(uri, method);
            return RequestHelper.MakeStringRequest<TOut>(request);
        }


        public Response<TOut> JsonGetRequest<TOut>(string uri) where TOut : class, new()
        {
            return JsonRequest<TOut>(uri, HttpMethod.GET);
        }


        public Response<TOut> JsonPutRequest<TOut>(string uri) where TOut : class, new()
        {
            return JsonRequest<TOut>(uri, HttpMethod.PUT);
        }


        public Response<TOut> JsonDeleteRequest<TOut> (string uri) where TOut : class, new ()
        {
            return JsonRequest<TOut>(uri, HttpMethod.DELETE);
        }



        /// <summary>
        /// User is rsponsible for closing HttpWebResponse after use!
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="method"></param>
        /// <param name="requestBody"></param>
        /// <param name="requestType"></param>
        /// <returns></returns>
        public HttpWebResponse Request(string uri, string method, byte[] requestBody = null, string requestType = "")
        {
            var request = PrepareWebRequest(uri, method);
            if (!string.IsNullOrEmpty(requestType))
                request.ContentType = requestType;
            return RequestHelper.MakeRawRequest (request, requestBody);
        }


        public Response<object> HttpRequest(string uri, string method, byte[] requestBody = null, string requestType = "")
        {
            var request = PrepareWebRequest(uri, method);
            if (!string.IsNullOrEmpty (requestType))
                request.ContentType = requestType;
            return RequestHelper.MakeHttpRequest (request, requestBody);
        }
    }
}

