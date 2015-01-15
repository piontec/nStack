using System.Net;
using NStack.WebClient;


namespace NStack.Auth
{
    public interface IClientConnection
    {
        KeystoneResponse LastKeystoneResponse { get; }


        KeystoneResponse Connect(string tokenUri, AuthData authData, bool cacheAuthData = true);


        Response<TOut> JsonGetRequest<TOut>(string uri) where TOut : class, new();


        Response<TOut> JsonPutRequest<TOut> (string uri) where TOut : class, new ();


        Response<TOut> JsonDeleteRequest<TOut>(string uri) where TOut : class, new();


        HttpWebResponse Request (string uri, string method, byte[] requestBody = null, string requestType = "");


        Response<object> HttpRequest (string uri, string method, byte[] requestBody = null, string requestType = "");
    }
}