// Copyright © 2025 Rune Gulbrandsen.
// All rights reserved. Licensed under the MIT License; see LICENSE.txt.

namespace KISS.HttpClientAuthentication.Configuration
{
    /// <summary>
    ///     Configuration settings for the Client Credentials OAuth2 flow.
    /// </summary>
    public sealed class ClientCredentialsConfiguration
    {
        /// <summary>
        ///     Set this to true if the <see cref="ClientId"/> and <see cref="ClientSecret"/> should be sent
        ///     basic authorization header rather as form body elements.
        /// </summary>
        public bool UseBasicAuthorizationHeader { get; set; }

        /// <summary>
        ///     Gets or sets the client id to be used with authentications that needs it.
        /// </summary>
        public string ClientId { get; set; } = default!;

        /// <summary>
        ///     Gets or sets the client secret to be used with authentications that needs it.
        /// </summary>
        public string ClientSecret { get; set; } = default!;

    }
}
