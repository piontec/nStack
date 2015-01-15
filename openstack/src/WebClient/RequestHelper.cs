using System;
using System.IO;
using System.Net;
using System.Text;
using Newtonsoft.Json;


namespace NStack.WebClient
{
    internal static class RequestHelper
    {
        private static Response<TOut> MakeRequest<TOut>(HttpWebRequest request, Action sendAction) where TOut : class, new()
        {
            WebResponse response;
            string responseFromServer = null;
            try
            {
                sendAction ();
                responseFromServer = ReadResponseString(request, out response);
            }
            catch (WebException we)
            {
                response = we.Response;
            }

            var httpResp = response as HttpWebResponse;
            if (httpResp == null)
                throw new Exception("Could not cast to http response");
            
            return ParseResponse<TOut>(httpResp, responseFromServer);
        }


        internal static Response<TOut> MakeStringRequest<TOut> (HttpWebRequest request, string requestBody="") where TOut : class, new()
        {
            return MakeRequest<TOut> (request, () =>
                                    {
                                        if (!string.IsNullOrEmpty(requestBody))
                                            SendRequestString (request, requestBody);
                                    });
        }


        /// <summary>
        /// User is responsible for closing HttpWebResponse after use
        /// </summary>
        /// <param name="request"></param>
        /// <param name="requestBody"></param>
        /// <returns></returns>
        internal static HttpWebResponse MakeRawRequest (HttpWebRequest request, byte[] requestBody)
        {
            HttpWebResponse httpResp;
            try
            {
                if (!(requestBody == null || requestBody.Length == 0))
                    SendRequestBytes (request, requestBody);
                httpResp = request.GetResponse () as HttpWebResponse;
            }
            catch (WebException we)
            {
                httpResp = we.Response as HttpWebResponse;
                if (httpResp == null)
                    throw new Exception("Could not cast to http response");
            }
            return httpResp;
        }


        internal static Response<object> MakeHttpRequest(HttpWebRequest request, byte[] requestBody)
        {
            var httpResp = MakeRawRequest (request, requestBody);
            return ParseResponse<object> (httpResp, string.Empty);
        }


        private static Response<TOut> ParseResponse<TOut> (HttpWebResponse httpResp, string responseFromServer) where TOut : class, new ()
        {
            // we got non-200-series http code response
            if (((int)httpResp.StatusCode) >= 300)
                return new Response<TOut> {Status = httpResp.StatusDescription, StatusCode = httpResp.StatusCode, RawResult = responseFromServer};

            // we got no content to parse or user doesn't want to parse it into the output object - ignore response
            if (httpResp.ContentLength == 0 || typeof(TOut) == typeof(object))
                return new Response<TOut> {Status = httpResp.StatusDescription, StatusCode = httpResp.StatusCode};

            try
            {
                var reply = JsonConvert.DeserializeObject<TOut> (responseFromServer);
                var result = new Response<TOut> {Status = httpResp.StatusDescription, StatusCode = httpResp.StatusCode, Result = reply};

                return result;
            }
            catch (Exception e)
            {
                return new Response<TOut>
                       {
                           Status = string.Format ("Error deserializing JSON; exception: {0}, message: {1}", e, e.Message),
                           StatusCode = HttpStatusCode.UnsupportedMediaType,
                           RawResult = responseFromServer
                       };
            }
            finally
            {
                httpResp.Close();
            }
        }


        private static string ReadResponseString (HttpWebRequest request, out WebResponse response)
        {
            response = request.GetResponse ();
            Stream dataStream = response.GetResponseStream ();
            StreamReader reader = new StreamReader (dataStream);
            string responseFromServer = reader.ReadToEnd ();

            reader.Close ();
            dataStream.Close ();
            response.Close ();
            return responseFromServer;
        }


        private static void SendRequestString (HttpWebRequest request, string requestBody)
        {
            SendRequestBytes (request, Encoding.UTF8.GetBytes (requestBody));
        }


        private static void SendRequestBytes(HttpWebRequest request, byte[] requestBytes)
        {
            //TODO: handle large requests
            request.ContentLength = requestBytes.Length;
            Stream dataStream = request.GetRequestStream();
            dataStream.Write(requestBytes, 0, requestBytes.Length);
            dataStream.Close();
        }


        internal static HttpWebRequest PrepareJsonWebRequestWithoutAuthHeader (string uri, string method)
        {
            var request = PrepareWebRequestWithoutAuthHeader (uri, method);
            request.ContentType = "application/json";
            request.Accept = "application/json";

            return request;
        }


        internal static HttpWebRequest PrepareWebRequestWithoutAuthHeader(string uri, string method)
        {
            var request = (HttpWebRequest)WebRequest.Create(uri);
            request.Method = method;

            return request;
        }
    }
}
