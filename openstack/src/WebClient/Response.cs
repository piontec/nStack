using System;
using System.Net;


namespace NStack.WebClient
{
    public class Response<T> where T : class, new ()
    {
        public HttpStatusCode StatusCode {get; set;}

        public string Status { get; set;}

        public string RawResult { get; set; }

        public T Result { get; set; }


        public override string ToString ()
        {
            return string.Format ("StatusCode: {0}, Status: {1}", Convert.ToInt32 (StatusCode), Status);
        }
    }
}

