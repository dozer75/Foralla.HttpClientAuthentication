// Copyright © 2024 Rune Gulbrandsen.
// All rights reserved. Licensed under the MIT License; see LICENSE.txt.

using KISS.HttpClientAuthentication.Constants;

namespace KISS.HttpClientAuthentication.Configuration
{
    /// <summary>
    ///     Configuration settings for OAuth2 authorization.
    /// </summary>
    public sealed class OAuth2Configuration
    {
        /// <summary>
        ///     Gets or sets the authorization endpoint used by some <see cref="AuthenticationProvider"/>
        ///     configuration.
        /// </summary>
        /// <remarks>
        ///     Obsolete: Use <see cref="TokenEndpoint"/> instead.
        /// </remarks>
        [Obsolete("Use TokenEndpoint instead.")]
        public Uri AuthorizationEndpoint { get => TokenEndpoint; set => TokenEndpoint = value; }

        /// <summary>
        ///     Gets or sets the authorization scheme to use if <see cref="AuthenticationHeader"/> is 
        ///     Authorization or the selected <see cref="AuthenticationProvider"/> uses Authorization as default header.
        ///     
        ///     If this is unset, it will be implied by the <see cref="AuthenticationProvider"/> if needed.
        /// </summary>
        public string? AuthorizationScheme { get; set; }

        /// <summary>
        ///     Gets or sets if the access token should be cached or not.
        /// </summary>
        public bool DisableTokenCache { get; set; }

        /// <summary>
        ///     Gets or sets the type of grant flow to be used.
        /// </summary>
        public OAuth2GrantType GrantType { get; set; }

        /// <summary>
        ///     Configuration when the <see cref="GrantType"/> is <see cref="OAuth2GrantType.ClientCredentials"/>.
        /// </summary>
        public ClientCredentialsConfiguration? ClientCredentials { get; set; }

        /// <summary>
        ///     Gets or sets the scopes to be needed when requesting the authentication for those <see cref="AuthenticationProvider"/>
        ///     that requires it.
        /// </summary>
        /// <remarks>
        ///     Scopes must be separated with a space.
        /// </remarks>
        public string? Scope { get; set; }

        /// <summary>
        ///     Gets or sets the token endpoint.
        /// </summary>
        /// <remarks>
        ///     Replaces <see cref="AuthorizationEndpoint"/>.
        /// </remarks>
        public Uri TokenEndpoint { get; set; } = default!;
    }
}
