// Copyright © 2023 Rune Gulbrandsen.
// All rights reserved. Licensed under the MIT License; see LICENSE.txt.

using System.Text.Json.Serialization;

namespace KISS.HttpClientAuthentication.Helpers
{
    /// <summary>
    ///     The OAuth2 token information.
    /// </summary>
    internal sealed class AccessTokenResponse
    {
        /// <summary>
        ///     The OAuth2 access token.
        /// </summary>
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; } = default!;

        /// <summary>
        ///     The OAuth2 token type
        /// </summary>
        [JsonPropertyName("token_type")]
        public string TokenType { get; set; } = default!;

        /// <summary>
        ///     The number of seconds the <see cref="AccessToken"/> is valid.
        /// </summary>
        [JsonPropertyName("expires_in")]
        public int? ExpiresIn { get; set; }
    }
}
