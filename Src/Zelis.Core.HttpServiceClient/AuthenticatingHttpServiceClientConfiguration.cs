using System;

namespace Zelis.Core.HttpServiceClient
{
    public class AuthenticatingHttpServiceClientConfiguration : HttpServiceClientConfiguration
    {
        private readonly string _password;
        private readonly string _username;

        public AuthenticatingHttpServiceClientConfiguration(
            string baseAddress,
            string password,
            int timeout,
            string username
        )
            : base(baseAddress, timeout)
        {
            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentNullException(nameof(password));
            if (string.IsNullOrWhiteSpace(username))
                throw new ArgumentNullException(nameof(username));

            _password = password;
            _username = username;
        }

        public string Password { get { return _password; } }
        public string Username { get { return _username; } }
    }
}