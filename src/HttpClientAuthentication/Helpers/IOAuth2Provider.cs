// Copyright © 2025 Rune Gulbrandsen.
// All rights reserved. Licensed under the MIT License; see LICENSE.txt.

using KISS.HttpClientAuthentication.Configuration;

namespace KISS.HttpClientAuthentication.Helpers
{
    /// <summary>
    ///     OAuth2 related provider methods.
    /// </summary>
    internal interface IOAuth2Provider
    {
        /// <summary>
        ///     Requests a <see cref="AccessTokenResponse"/> using OAuth2 client credentials authorization grant based on the specified
        ///     <paramref name="configuration"/>.
        /// </summary>
        /// <param name="configuration">A <see cref="OAuth2Configuration"/> instance with OAuth2 relevant configuration information.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> used to control the operation.</param>
        /// <returns>An instance of <see cref="AccessTokenResponse"/> containing token information if found, otherwise null.</returns>
        ValueTask<AccessTokenResponse?> GetClientCredentialsAccessTokenAsync(OAuth2Configuration configuration, CancellationToken cancellationToken = default);
    }
}
