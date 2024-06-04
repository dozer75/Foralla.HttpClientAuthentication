// Copyright © 2024 Rune Gulbrandsen.
// All rights reserved. Licensed under the MIT License; see LICENSE.txt.

using KISS.HttpClientAuthentication.Constants;

namespace KISS.HttpClientAuthentication.Configuration
{
    /// <summary>
    ///     Configuration settings for the HTTP client authentication.
    /// </summary>
    public sealed class HttpClientAuthenticationConfiguration
    {
        /// <summary>
        ///     Gets or sets the authentication provider type.
        /// </summary>
        public AuthenticationProvider AuthenticationProvider { get; set; }

        /// <summary>
        ///     Configuration settings for API key authentication.
        /// </summary>
        public ApiKeyConfiguration? ApiKey { get; set; }

        /// <summary>
        ///     Configuration settings for Basic authentication.
        /// </summary>
        public BasicConfiguration? Basic { get; set; }

        /// <summary>
        ///     Configuration settings for OAuth2/OpenId authentication.
        /// </summary>
        public OAuth2Configuration? OAuth2 { get; set; }
    }
}
