namespace NStack.Auth
{
    public class AuthData
    {
        public class Auth
        {
            public class PasswordCredentials
            {
                public string username { get; set; }
                public string password { get; set; }
            }

            public string tenantName { get; set; }
            public PasswordCredentials passwordCredentials { get; set; }
        }

        public Auth auth;


        public AuthData (string tenant, string user, string password)
        {
            auth = new Auth
                   {
                       tenantName = tenant,
                       passwordCredentials = new Auth.PasswordCredentials
                                             {
                                                 password = password,
                                                 username = user
                                             }
                   };
        }
    }
}