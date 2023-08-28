// Copyright © 2023 Rune Gulbrandsen.
// All rights reserved. Licensed under the MIT License; see LICENSE.txt.

namespace KISS.HttpClientAuthentication.Constants
{
    /// <summary>
    ///     Various constants used by OAuth2 authentication 
    /// </summary>
    internal static class OAuth2Keyword
    {
        public const string GrantType = "grant_type";

        public const string ClientCredentials = "client_credentials";

        public const string ClientId = "client_id";
        public const string ClientSecret = "client_secret";

        public const string Scope = "scope";
    }
}
