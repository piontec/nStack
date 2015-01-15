using System;
using NStack.Auth;
using NStack.WebClient;


namespace NStack.Exceptions
{
    class AuthenticationException : Exception
    {
        private readonly Response<KeystoneResponse> m_response;

        public Response<KeystoneResponse> Response
        {
            get { return m_response; }
        }

        public AuthenticationException (Response<KeystoneResponse> response, string messagae) : base (messagae)
        {
            m_response = response;
        }
    }
}
